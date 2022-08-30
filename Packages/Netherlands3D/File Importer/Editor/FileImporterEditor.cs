#if UNITY_EDITOR // This script is only used in editor
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;

namespace Netherlands3D.FileImporter
{
    /// <summary>
    /// For quickly importing a file with string event for testing purposes in the unity editor enviroment
    /// </summary>
    /// <seealso cref="EditorFileImporterEditor.cs"/>
    [AddComponentMenu("Netherlands3D/File Importer/File Importer Editor")]
    public class FileImporterEditor : MonoBehaviour
    {
        [Tooltip("The string event to trigger when loading the files")]
        public StringEvent eventImportFiles;        
    }
}
#endif