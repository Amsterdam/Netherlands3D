using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Traffic.Editor
{
    /// <summary>
    /// For quickly importing files in Unity Editor
    /// </summary>
    /// <see cref="EditorFileImporterEditor"/>
    [RequireComponent(typeof(FileImporter))]
    [AddComponentMenu("Traffic/Traffic File Importer Editor")]
    public class FileImporterEditor : MonoBehaviour
    {
        public FileImporter File { get { return GetComponent<FileImporter>(); } }
    }
}
