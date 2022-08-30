using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_WEBGL
using Netherlands3D.JavascriptConnection;
#endif
using Netherlands3D.Events;

#if UNITY_STANDALONE || UNITY_EDITOR
using Netherlands3D.FileImporter.SFB;
#endif

namespace Netherlands3D.FileImporter
{
    /// <summary>
    /// A script that can be added to a button component for importing files depending on the platform type
    /// </summary>
    [AddComponentMenu("Netherlands3D/File Importer/Import File Button On Click")]
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(RectTransform))]
    public class ImportFileButtonOnClick : MonoBehaviour
    {
        /// <summary>
        /// Method that gets called from javascript
        /// </summary>
        /// <param name="inputName"></param>
        /// <param name="fileExtension"></param>
        /// <param name="multiSelect"></param>
        [DllImport("__Internal")]        
        private static extern void AddFileInput(string inputName, string fileExtension, bool multiSelect);

        [Tooltip("HTML DOM ID")]
        [SerializeField] private string fileInputName = "fileInput";
        [Tooltip("The allowed file extention to load. Don't put a '.' at the start")]
        [SerializeField] string fileExtention = "obj";
        [Tooltip("Allow user to select multiple files")]
        [SerializeField] private bool multiSelect;
        
        /// <summary>
        /// The string event that gets called upon loading a file
        /// </summary>
        [SerializeField] private StringEvent eventFileLoaderFileImported;

        /// <summary>
        /// The button component attached to this same gameobject
        /// </summary>
        private Button button;

        // Start is called before the first frame update
        void Start()
        {
            button = GetComponent<Button>();
            // Set file input name with generated id to avoid html conflictions
            fileInputName += "_" + gameObject.GetInstanceID();
            name = fileInputName;

            // Execute setup based on platform
            // Standalone
#if UNITY_STANDALONE || UNITY_EDITOR
            SetupUnityStandalone();
#endif
            // Unity Editor
#if UNITY_EDITOR
            //SetupUnityEditor(); // Doesnt allow for multiple file selection
#endif
            // WebGL
#if UNITY_WEBGL && !UNITY_EDITOR
            SetupWebGL();
#endif
        }


        // Standalone
#if UNITY_STANDALONE || UNITY_EDITOR
                
        private void SetupUnityStandalone()
        {
            button.onClick.AddListener(OnButtonClickUnityStandalone);
        }

        /// <summary>
        /// When the user clicks the button in standalone mode
        /// </summary>
        private void OnButtonClickUnityStandalone()
        {
            var result = StandaloneFileBrowser.OpenFilePanel("Select File", "", fileExtention, multiSelect);
            if(result.Length != 0)
            {
                // Invoke the event with joined string values
                eventFileLoaderFileImported.Invoke(string.Join(",", result));
            }
        }
#endif

        // Unity Editor
#if UNITY_EDITOR

        private void SetupUnityEditor()
        {
            button.onClick.AddListener(OnButtonClickUnityEditor);
        }

        /// <summary>
        /// When the user clicks the button in editor mode
        /// </summary>
        private void OnButtonClickUnityEditor()
        {
            string filePath = UnityEditor.EditorUtility.OpenFilePanel("Select File", "", fileExtention);
            if(filePath.Length != 0)
            {
                UnityEngine.Debug.Log("[File Importer] Import file from file path: " + filePath);
                eventFileLoaderFileImported.Invoke(filePath);
            }
        }
#endif

        // WebGL
#if UNITY_WEBGL && !UNITY_EDITOR

        private void SetupWebGL()
        {
            AddFileInput(fileInputName, fileExtention, multiSelect);
            gameObject.AddComponent<DrawHTMLOverCanvas>().AlignObjectID(fileInputName);
            // A html button gets generated over this button so the pivot has to be 0,0 (bottom left) since it gets generated from the bottom left
            GetComponent<RectTransform>().pivot = Vector2.zero;
        }

        /// <summary>
        /// If the click is registerd from the HTML overlay side, this method triggers the onClick events on the button
        /// </summary>
        public void ClickNativeButton()
        {
            if(button != null)
            {
                Debug.Log("Invoked native Unity button click event on " + this.gameObject.name);
                button.onClick.Invoke();
            }
        }
#endif

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Check if user didn't put a . in the file extention
            if(fileExtention.Contains(".")) fileExtention = fileExtention.Replace(".", "");
        }
#endif
    }
}