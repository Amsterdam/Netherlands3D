using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Netherlands3D.Events;

namespace Netherlands3D.VISSIM
{
    /// <summary>
    /// For handling a string event
    /// </summary>
    public class StringLoader
    {
        /// <summary>
        /// An event that gets called if the database needs to be cleared
        /// </summary>
        private BoolEvent eventClearDatabase;
        /// <summary>
        /// The extension of the file that gets imported
        /// </summary>
        private readonly string fileExtension = ".fzp";

        /// <summary>
        /// Class constructor
        /// </summary>
        public StringLoader(StringEvent eventFilesImported, BoolEvent eventClearDatabase)
        {
            eventFilesImported.started.AddListener(FileImported);
            this.eventClearDatabase = eventClearDatabase;
        }

        /// <summary>
        /// Load a singular file contents into VISSIM
        /// </summary>
        /// <param name="file"></param>
        public void LoadFile(string file)
        {
            if(!file.EndsWith(fileExtension))
            {
                UnityEngine.Debug.LogError("[VISSIM] Cannot load file because its fileExtension doesnt end with: " + fileExtension);
                return;
            }

            VISSIMManager.Instance.StartCoroutine(LoadVISSIMFromFile(file, 1));
        }

        /// <summary>
        /// Gets called when files are imported and processes them
        /// </summary>
        /// <param name="files">The files to import</param>
        private void FileImported(string files)
        {
            if(VISSIMManager.ShowDebugLog) UnityEngine.Debug.Log("[VISSIM] StringLoader.FilesImported(): " + files.Substring(0, 128) + " ...[Log Cutoff]");

            // Sepperate the files
            string[] importedFiles = files.Split(',');
            foreach(string file in importedFiles)
            {
                if(file.EndsWith(fileExtension))
                {
                    VISSIMManager.Instance.StartCoroutine(LoadVISSIMFromFile(file));
                    return;
                }
            }
        }

        /// <summary>
        /// Load VISSIM data from a file
        /// </summary>
        /// <param name="file">The file to load from</param>
        /// <param name="loadIndex">How the file should be loaded</param>
        /// <returns>yield break</returns>
        private IEnumerator LoadVISSIMFromFile(string file, int loadIndex = 0)
        {
            Stopwatch sw = new Stopwatch();
            if(VISSIMManager.ShowDebugLog) UnityEngine.Debug.Log("Started loading VISSIM from file...");
            sw.Start();

            // Convert file
            string fileContent = "";
            switch(loadIndex)
            {
                case 0: // From persistentDataPath
                    fileContent = File.ReadAllText(Application.persistentDataPath + "/" + file);
                    File.Delete(file); // Why???
                    break;
                case 1: // ReadAllText
                    fileContent = File.ReadAllText(file);
                    break;
                default:
                    UnityEngine.Debug.LogError("[VISSIM] LoadIndex is out of range! Got: " + loadIndex);
                    break;
            }

            if(VISSIMManager.ShowDebugLog) UnityEngine.Debug.Log("[VISSIM] File contents: " + fileContent.Substring(0, 2048) + " ...[Log Cutoff]");
            // Load
            yield return ConverterFZP.Convert(fileContent);
            sw.Stop();
            if(VISSIMManager.ShowDebugLog) UnityEngine.Debug.Log("Loaded VISSIM in " + sw.ElapsedMilliseconds + "ms");

            // Clear database
            eventClearDatabase.started.Invoke(true);

            yield break;
        }
    }
}
