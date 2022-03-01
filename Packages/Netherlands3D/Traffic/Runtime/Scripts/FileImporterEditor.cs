using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Traffic.Editor
{
    /// <summary>
    /// For quickly importing files in Unity Editor
    /// </summary>
    /// <see cref="EditorFileImporterEditor"/>
    [RequireComponent(typeof(File))]
    [AddComponentMenu("Traffic/Traffic File Importer Editor")]
    public class FileImporterEditor : MonoBehaviour
    {
        public File File { get { return GetComponent<File>(); } }
    }
}
