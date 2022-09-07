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


        [DllImport("__Internal")]        
        private static extern void AddFileInput(string inputName, string fileExtension, bool multiSelect);
        [Tooltip("HTML DOM ID")]
        [SerializeField] private string fileInputName = "fileInput";
        FileInputIndexedDB javaScriptFileInputHandler;


        [Tooltip("The allowed file extention to load. Don't put a '.' at the start")]
        [SerializeField] private string fileExtention = "csv";
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

#if UNITY_WEBGL && !UNITY_EDITOR
            javaScriptFileInputHandler = FindObjectOfType<FileInputIndexedDB>();
            if (javaScriptFileInputHandler == null)
            {
                GameObject go = new GameObject("UserFileUploads");
                go.AddComponent<FileInputIndexedDB>();
            }

            
            // Set file input name with generated id to avoid html conflictions
            fileInputName += "_" + gameObject.GetInstanceID();
            name = fileInputName;
            
            AddFileInput(fileInputName, fileExtention, multiSelect);
            gameObject.AddComponent<DrawHTMLOverCanvas>().AlignObjectID(fileInputName);
            // A html button gets generated over this button so the pivot has to be 0,0 (bottom left) since it gets generated from the bottom left
            GetComponent<RectTransform>().pivot = Vector2.zero;
#endif

            // Execute setup based on platform
            // Standalone
#if UNITY_STANDALONE && !UNITY_EDITOR
            button.onClick.AddListener(OnButtonClickUnityStandalone);
#endif

#if UNITY_EDITOR
            button.onClick.AddListener(OnButtonClickUnityEditor);
#endif
        }



#if UNITY_WEBGL && !UNITY_EDITOR

        /// <summary>
        /// If the click is registerd from the HTML overlay side, this method triggers the onClick events on the button
        /// </summary>
        public void ClickNativeButton()
        {
            if(button != null)
            {
                javaScriptFileInputHandler.SetCallbackAdress(SendResults);
                Debug.Log("Invoked native Unity button click event on " + this.gameObject.name);
                button.onClick.Invoke();
            }
        }
#endif

        // Standalone
#if UNITY_STANDALONE && !UNITY_EDITOR

        /// <summary>
        /// When the user clicks the button in standalone mode
        /// </summary>
        private void OnButtonClickUnityStandalone()
        {
            string[] result = StandaloneFileBrowser.OpenFilePanel("Select File", "", fileExtention, multiSelect);
            if(result.Length != 0)
            {
                for (int i = 0; i < result.Length; i++)
                {
                    if (result[i]=="") continue;
                    string filename = System.IO.Path.GetFileName(result[i]);
                    string newFilepath = System.IO.Path.Combine(Application.persistentDataPath, filename);
                    result[i] = filename;
                    System.IO.File.Copy(result[i], newFilepath,true);
                }
                // Invoke the event with joined string values
                eventFileLoaderFileImported.Invoke(string.Join(",", result));
            }
        }
#endif

        // Unity Editor
#if UNITY_EDITOR

        /// <summary>
        /// When the user clicks the button in editor mode
        /// </summary>
        private void OnButtonClickUnityEditor()
        {
            string filePath = UnityEditor.EditorUtility.OpenFilePanel("Select File", "", fileExtention);
            if (filePath.Length != 0)
            {
                string filename = System.IO.Path.GetFileName(filePath);
                string newFilePath = System.IO.Path.Combine(Application.persistentDataPath, filename);
                System.IO.File.Copy(filePath, newFilePath,true);
                UnityEngine.Debug.Log("[File Importer-unityEditor] Import file from file path: " + filename);
                eventFileLoaderFileImported.Invoke(filename);
            }
        }
#endif

        // WebGL


        public void SendResults(string filePaths)
        {
            
            Debug.Log("button received: " + filePaths);
            eventFileLoaderFileImported.Invoke(filePaths);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Check if user didn't put a . in the file extention
            if(fileExtention.Contains(".")) fileExtention = fileExtention.Replace(".", "");
        }
#endif
    }
}