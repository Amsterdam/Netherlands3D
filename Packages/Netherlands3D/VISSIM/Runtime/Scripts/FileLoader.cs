using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Netherlands3D.Events;

namespace Netherlands3D.VISSIM
{
    /// <summary>
    /// For loading files for VISSIM
    /// </summary>
    public static class FileLoader
    {
        /// <summary>
        /// Load a file for VISSIM
        /// </summary>
        /// <param name="file">The file or files to load</param>
        public static void Load(string filePaths)
        {
            VISSIMManager.Instance.StartCoroutine(LoadAysnc(filePaths));
        }

        /// <summary>
        /// Load a file for VISSIM IEnumerator to prevent project from freezing
        /// </summary>
        /// <param name="filePaths"></param>
        /// <returns></returns>
        private static IEnumerator LoadAysnc(string filePaths)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int failedFiles = 0;

            // Check if there are multiple files
            string[] paths = filePaths.Split(',');
            if(VISSIMManager.ShowDebugLog) UnityEngine.Debug.Log(string.Format("[VISSIM] Loading {0} file(s)...", paths.Length));
            foreach(string path in paths)
            {
                // Check if we can load the file based on file extension
                string pathExtension = path.Substring(path.LastIndexOf('.'));
                switch(pathExtension)
                {
                    case ".fzp":
                        yield return ConverterFZP.Convert(path);
                        break;
                    default:
                        failedFiles++;
                        UnityEngine.Debug.LogError(string.Format("[VISSIM] Cannot load file because {0} isn't supported", pathExtension));
                        break;
                }
            }

            sw.Stop();
            if(VISSIMManager.ShowDebugLog) UnityEngine.Debug.Log(string.Format("[VISSIM] Loaded {0} file(s) in {1}ms", paths.Length - failedFiles, sw.ElapsedMilliseconds));
            yield break;
        }        
    }
}
