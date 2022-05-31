using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Netherlands3D.Traffic.VISSIM
{
    /// <summary>
    /// For converting vissim .att files
    /// </summary>
    /// <remarks>
    /// An .att file contains the locations of signal heads
    /// </remarks>
    public static class ConverterATT
    {
        private static readonly string requiredTemplate = "$SIGNALHEAD:NO;SG;LNWID;ROTANGLE;WKTLOC";

        public static IEnumerator Convert(string filePath, int maxDataCount, Action<Dictionary<int, DataATT>> callback)
        {
            // Convert filePath to fileContent
            using StreamReader sr = new StreamReader(filePath);
            bool readyToConvert = false;
            string line;
            DataATT data;
            Dictionary<int, DataATT> convertedData = new Dictionary<int, DataATT>();
            // Read and display lines from the file until the end of the file is reached.
            while((line = sr.ReadLine()) != null)
            {
                // Check line contents
                if(readyToConvert && !string.IsNullOrEmpty(line))
                {
                    data = StringToData(line);
                    // Check if data number is already in convertedData
                    if(convertedData.ContainsKey(data.number))
                    {
                        Debug.LogWarning("[ConverterATT] found duplicate signal head number, ignoring...");
                    }
                    else
                    {
                        // Doesnt exist, add it
                        convertedData.Add(data.number, data);
                    }
                }

                // Check if the line template string is the same as the requiredTemplate
                if(line == requiredTemplate)
                {
                    readyToConvert = true;
                }
            }

            // Add data to VISSIM
            callback(convertedData);

            yield break;
        }

        private static DataATT StringToData(string dataString)
        {
            string[] array = dataString.Split(';');
            int number = -1;
            if(!int.TryParse(array[0], out number)) Debug.LogError("[ConverterATT] Failed to parse number! Check if the .att file is correct");
            float laneWidth;
            if(!float.TryParse(array[2], out laneWidth)) Debug.LogError("[ConverterATT] Failed to parse laneWidth! Check if the .att file is correct");
            float rotationAngle;
            if(!float.TryParse(array[3], out rotationAngle)) Debug.LogError("[ConverterATT] Failed to parse rotation angle! Check if the .att file is correct");
            Vector2 wktLocation;
            string lFilter = Regex.Replace(array[4], "[^0-9. ]", "");
            Debug.Log(lFilter);
            int spaceIndex = lFilter.IndexOf(" ");
            float.TryParse(lFilter.Substring(0, spaceIndex), out wktLocation.x);
            float.TryParse(lFilter.Substring(spaceIndex, lFilter.Length - spaceIndex), out wktLocation.y);
            Debug.Log(wktLocation);
            return new DataATT(number, array[1], laneWidth, rotationAngle, wktLocation);
        }
    }
}
