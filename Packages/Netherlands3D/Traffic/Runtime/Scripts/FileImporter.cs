using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Netherlands3D.Events;

namespace Netherlands3D.Traffic.VISSIM
{
    /// <summary>
    /// For importing files for Traffic
    /// </summary>
    [AddComponentMenu("Netherlands3D/Traffic/Traffic File Importer")] // Used to change the script inspector name
    public class FileImporter : MonoBehaviour
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
        [SerializeField] private Database dataBase;

        private void OnEnable()
        {
            eventFilesImported.AddListenerStarted(Load);
        }

        private void OnDisable()
        {
            eventFilesImported.RemoveListenerStarted(Load);
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
            eventLoadingProgress.InvokeStarted(0.001f);
            yield return new WaitForEndOfFrame();

            // Check if there are multiple files
            string[] paths = filePaths.Split(',');
            if(showDebugLog) UnityEngine.Debug.Log(string.Format("[Traffic File Importer] Loading {0} file(s)...", paths.Length));
            foreach(string path in paths)
            {
                // Check if we can load the file based on file extension
                string pathExtension = path.Substring(path.LastIndexOf('.'));
                switch(pathExtension.ToLower())
                {
                    case ".att":
                        yield return ConverterATT.Convert(path, convertedData =>
                        {
                            dataBase.AddSignalHeads(convertedData);
                        });
                        break;
                    case ".fzp":
                        yield return ConverterFZP.Convert(path, maxDataCount, convertedData => 
                        {
                            dataBase.AddData(convertedData);
                        });
                        break;
                    case ".lsa":
                        yield return ConverterLSA.Convert(path, dataBase.SignalHeads, convertedData =>
                        {
                            dataBase.SignalHeads = convertedData;
                            dataBase.OnSignalHeadsChanged.Invoke();
                        });
                        break;
                    default:
                        failedFiles++;
                        UnityEngine.Debug.LogError(string.Format("[Traffic File Importer] Cannot load file because {0} isn't supported", pathExtension));
                        break;
                }

                eventLoadingProgress.InvokeStarted(fileIndex / paths.Length);
                yield return new WaitForEndOfFrame();
                fileIndex++;
            }

            eventClearDatabase.InvokeStarted(true);
            sw.Stop();
            if(showDebugLog) UnityEngine.Debug.Log(string.Format("[Traffic File Importer] Loaded {0} file(s) in {1}ms", paths.Length - failedFiles, sw.ElapsedMilliseconds));
            yield break;
        }
    }
}
