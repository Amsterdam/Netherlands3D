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
    [AddComponentMenu("Netherlands3D/Traffic/Traffic File Importer")] // Used to change the script inspector name
    public class File : MonoBehaviour
    {
        [Header("Options")]
        [Tooltip("Show the debug messages")]
        [SerializeField] private bool showDebugLog;
        [Tooltip("The max amount of data that can be loaded")]
        [SerializeField] private int maxDataCount = 200;

        [Header("Components")]
        [Tooltip("Event that fires when files are imported")]
        [SerializeField] private StringEvent eventFilesImported;
        [Tooltip("Event that fires when the database needs to be cleared")]
        [SerializeField] private BoolEvent eventClearDatabase;
        [Tooltip("The loading process expressed in a float ranging 0 to 1 with 1 being completed")]
        /// <remarks>
        /// Currently only displays the progress of files being loaded (x of total), if it where to show on what line it is off the file it would slow down the loading alot
        /// Would need to create a more raw loading float that invokes every x lines of a file
        /// </remarks>
        [SerializeField] private FloatEvent eventLoadingProgress;
        [Tooltip("The database holding 'Data' classes")]
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
        /// Open a file and on success load it
        /// </summary>
        public void Open()
        {
#if UNITY_EDITOR
            // For unity editor opening
            string filePath = UnityEditor.EditorUtility.OpenFilePanel("Select .FZP File", "", "fzp");
            if(filePath.Length != 0)
            {
                UnityEngine.Debug.Log("[Traffic Testing] Selected .fzp file from: " + filePath);
                Load(filePath);
            }
            return;
#endif
            // The actual opening/loading (in a build) of a file is triggerd by the StringEvent eventFilesImported
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

            // Loading event
            int fileIndex = 1;
            eventLoadingProgress.started.Invoke(0);

            // Check if there are multiple files
            string[] paths = filePaths.Split(',');
            if(showDebugLog) UnityEngine.Debug.Log(string.Format("[Traffic File Importer] Loading {0} file(s)...", paths.Length));
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
                        UnityEngine.Debug.LogError(string.Format("[Traffic File Importer] Cannot load file because {0} isn't supported", pathExtension));
                        break;
                }

                eventLoadingProgress.started.Invoke(fileIndex / paths.Length);
                fileIndex++;
            }

            eventClearDatabase.started.Invoke(true);
            sw.Stop();
            if(showDebugLog) UnityEngine.Debug.Log(string.Format("[Traffic File Importer] Loaded {0} file(s) in {1}ms", paths.Length - failedFiles, sw.ElapsedMilliseconds));
            yield break;
        }
    }
}
