using System.Collections;
using System.Collections.Generic;
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
        private readonly string fileExtension = ".fpz";

        /// <summary>
        /// Class constructor
        /// </summary>
        public StringLoader(StringEvent eventFilesImported, BoolEvent eventClearDatabase)
        {
            eventFilesImported.started.AddListener(FileImported);
            this.eventClearDatabase = eventClearDatabase;
        }

        /// <summary>
        /// Gets called when files are imported and processes them
        /// </summary>
        /// <param name="files">The files to import</param>
        private void FileImported(string files)
        {
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
        /// <returns>yield break</returns>
        private IEnumerator LoadVISSIMFromFile(string file)
        {
            string fileContent = File.ReadAllText(Application.persistentDataPath + "/" + file);
            File.Delete(file); // Why???

            // Load
            yield return ConverterFZP.Convert(fileContent);

            // Clear database
            eventClearDatabase.started.Invoke(true);

            yield break;
        }
    }
}
