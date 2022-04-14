using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using Netherlands3D.JavascriptConnection;

namespace Netherlands3D.FileImporter
{
    /// <summary>
    /// A script that can be added to a button component for importing files depending on the platform type
    /// </summary>
    [AddComponentMenu("File Importer/Import File Button On Click")]
    [RequireComponent(typeof(Button))]
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

        [Tooltip("The allowed file extention to load")]
        [SerializeField] private string fileExtention;
        [Tooltip("Allow user to select multiple files")]
        [SerializeField] private bool selectMultipleFiles;

        [Tooltip("HTML DOM ID")]
        [SerializeField] private string fileInputName = "fileInput";

        /// <summary>
        /// The button component attached to this same gameobject
        /// </summary>
        private Button button;

        // Start is called before the first frame update
        void Start()
        {
            button = GetComponent<Button>();
            name = fileInputName;
#if !UNITY_EDITOR && UNITY_WEBGL
            AddFileInput(fileInputName, fileExtention, selectMultipleFiles);
            gameObject.AddComponent<DrawHTMLOverCanvas>().AlignObjectID(fileInputName);
#endif
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
    }
}