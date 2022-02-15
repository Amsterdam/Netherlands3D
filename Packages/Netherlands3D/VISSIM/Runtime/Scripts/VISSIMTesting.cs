using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.VISSIM
{
    /// <summary>
    /// For testing/interacting with the VISSIM Manager within the Unity editor enviroment
    /// </summary>
    public class VISSIMTesting : MonoBehaviour
    {
        /// <summary>
        /// Opens a file and runs it through the VISSIMManager
        /// </summary>
        /// <param name="path"></param>
        public void OpenFile(string filePath)
        {
            VISSIMManager.InvokeEventFilesImported(filePath);            
        }
    }
}
