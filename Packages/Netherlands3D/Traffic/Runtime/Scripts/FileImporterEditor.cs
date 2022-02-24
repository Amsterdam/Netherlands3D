using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Traffic.Editor
{
    /// <summary>
    /// For quickly importing files in Unity Editor
    /// </summary>
    [RequireComponent(typeof(File))]
    [AddComponentMenu("Traffic/Traffic File Importer Editor")]
    public class FileImporterEditor : MonoBehaviour
    {
        public File file { get { return GetComponent<File>(); } }

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
