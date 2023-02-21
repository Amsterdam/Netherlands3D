using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
using Netherlands3D.Core;
using System.IO;
using System;
using System.Globalization;

namespace Netherlands3D.Timeline
{
    /// <summary>
    /// For handeling importing files for the time line
    /// </summary>
    [AddComponentMenu("Netherlands3D/Timeline/Timeline File Importer")]
    public class TimelineFileImporter : MonoBehaviour
    {
        [Tooltip("The string event to check for timeline file importing")]
        [SerializeField] private StringEvent fileImported;
        [Tooltip("The timeline data to add it too")]
        [SerializeField] private TimelineData timelineData;

        private void OnEnable()
        {
            fileImported.AddListenerStarted(x => StartCoroutine(OnFilesImported(x)));
        }

        private void OnDisable()
        {
            fileImported.RemoveListenerStarted(x => StartCoroutine(OnFilesImported(x)));
        }

        /// <summary>
        /// Callback when files are imported. Converts the files to usable SO data
        /// </summary>
        /// <param name="filePaths"></param>
        /// <returns></returns>
        private IEnumerator OnFilesImported(string filePaths)
        {
            // Check if there are multiple files
            string[] paths = filePaths.Split(',');

            int failedFiles = 0;
            int fileIndex = 0;
            List<string[]> csvData = new List<string[]>();
            foreach(string path in paths)
            {
                // Check if we can load the file based on file extension
                string pathExtension = path.Substring(path.LastIndexOf('.'));
                switch(pathExtension)
                {
                    case ".csv":
                        yield return CsvParser.StreamReadLines(path, 2, 999, x => { }, x => { }, x => csvData = x);
                        break;
                    default:
                        failedFiles++;
                        UnityEngine.Debug.LogError(string.Format("[Timeline File Importer] Cannot load file because {0} isn't supported", pathExtension));
                        break;
                }

                fileIndex++;
            }

            CSVToTimelineData(csvData);
        }

        /// <summary>
        /// Convert csv to timeline data
        /// </summary>
        /// <param name="csv"></param>
        private void CSVToTimelineData(List<string[]> csv)
        {
            string[] dateFormats = new string[] { "yyyy/MM/dd", "dd/MM/yyyy" };
            // Loop through csv and convert it to time periods
            foreach(string[] row in csv)
            {
                // 0: name, 1: description, 2: startTime, 3: endTime, 4: layer
                print(row[0]);
                print(row[1]);
                print(row[2]);
                print(row[4]);
                if(!DateTime.TryParseExact(row[2], dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime start)) continue;
                if(!DateTime.TryParseExact(row[3], dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime end)) continue;
                TimePeriod t = new TimePeriod(row[0], row[1], start, end, row[4]);
                
                timelineData.timePeriods.Add(t);
            }
            timelineData.OrderTimePeriods();
        }
    }
}
