using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Netherlands3D.Events;

namespace Netherlands3D.Traffic
{
    /// <summary>
    /// For importing files for Traffic
    /// </summary>
    [AddComponentMenu("Traffic/Traffic File Importer")] // Used to change the script inspector name
    public class File : MonoBehaviour
    {
        [Header("Options")]
        [Tooltip("Show the debug messages")]
        [SerializeField] private bool showDebugLog;

        [SerializeField] private int maxDataCount = 200;

        [Header("Components")]
        [Tooltip("Event that fires when files are imported")]
        [SerializeField] private StringEvent eventFilesImported;
        [Tooltip("Event that fires when the database needs to be cleared")]
        [SerializeField] private BoolEvent eventClearDatabase;

        [SerializeField] private DataDatabase dataDatabase;

        private void OnEnable()
        {
            eventFilesImported.started.AddListener(Load);
        }

        private void OnDisable()
        {
            eventFilesImported.started.RemoveListener(Load);
        }

        /// <summary>
        /// Load a file for traffic
        /// </summary>
        /// <param name="filePaths">The filepaths of files to load</param>
        public void Load(string filePaths)
        {
            StartCoroutine(LoadAsync(filePaths));
        }

        /// <summary>
        /// Load a file for traffic
        /// </summary>
        /// <param name="filePaths"></param>
        /// <returns></returns>
        private IEnumerator LoadAsync(string filePaths)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int failedFiles = 0;
            System.Action<Dictionary<int, Data>> convertedData = null;

            // Check if there are multiple files
            string[] paths = filePaths.Split(',');
            if(showDebugLog) UnityEngine.Debug.Log(string.Format("[File Importer] Loading {0} file(s)...", paths.Length));
            foreach(string path in paths)
            {
                // Check if we can load the file based on file extension
                string pathExtension = path.Substring(path.LastIndexOf('.'));
                switch(pathExtension)
                {
                    case ".fzp":
                        yield return ConverterFZP.Convert(path, maxDataCount, convertedData => 
                        {
                            dataDatabase.AddData(convertedData);
                        });
                        break;
                    default:
                        failedFiles++;
                        UnityEngine.Debug.LogError(string.Format("[File Importer] Cannot load file because {0} isn't supported", pathExtension));
                        break;
                }
            }

            eventClearDatabase.started.Invoke(true);
            sw.Stop();
            if(showDebugLog) UnityEngine.Debug.Log(string.Format("[File Importer] Loaded {0} file(s) in {1}ms", paths.Length - failedFiles, sw.ElapsedMilliseconds));
            yield break;
        }
    }
}
