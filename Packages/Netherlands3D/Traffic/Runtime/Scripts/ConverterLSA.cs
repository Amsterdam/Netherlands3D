using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Netherlands3D.Traffic.VISSIM
{
    /// <summary>
    /// For converting vissim .lsa files
    /// </summary>
    public static class ConverterLSA
    {
        /// <summary>
        /// The line the converter looks for before converting data
        /// </summary>
        private static readonly string requiredTemplate = "$SIGNALHEAD:SIMSEC;SG;SGID;COLOR";

        public static IEnumerator Convert(string filePath, Dictionary<int, SignalHeadData> signalHeads, Action<Dictionary<int, SignalHeadData>> callback)
        {
            // Convert filePath to fileContent
            using StreamReader sr = new StreamReader(filePath);
            bool readyToConvert = false;
            string line;
            LSA data;
            List<LSA> lsaData = new List<LSA>();
            // Read and display lines from the file until the end of the file is reached.
            while((line = sr.ReadLine()) != null)
            {
                // Check line contents
                if(readyToConvert && !string.IsNullOrEmpty(line))
                {
                    data = StringToData(line);
                    lsaData.Add(data);
                }

                // Check if the line template string is the same as the requiredTemplate
                if(line == requiredTemplate)
                {
                    readyToConvert = true;
                }
            }

            // Loop trough lsa data and sort it on group
            foreach(var item in lsaData)
            {
                // Check if id is available
                if(!signalHeads.ContainsKey(item.signalGroupID)) continue;

                // Signal head exists, check simsec
                if(signalHeads[item.signalGroupID].schedule.ContainsKey(item.simsec)) continue; // Already exists, dupe

                // Add simsec with colorIndex
                signalHeads[item.signalGroupID].schedule.Add(item.simsec, item.colorIndex);
            }

            // Callback data
            callback(signalHeads);

            yield break;
        }

        private static LSA StringToData(string dataString)
        {
            string[] array = dataString.Split(';');

            // Parse simsec
            if(!float.TryParse(array[0], out float simsec)) Debug.LogError("[ConverterLSA] Failed to parse simsec! Check if the .lsa file is correct");
            // Signal group
            if(!int.TryParse(array[3], out int signalGroupID)) Debug.LogError("[ConverterLSA] Failed to parse signalGroupID! Check if the .lsa file is correct");
            // Color index
            int colorIndex = 0;
            switch(array[4].ToLower())
            {
                case "amber": colorIndex = 1;break;
                case "green": colorIndex = 2; break;
                default: break;
            }

            return new LSA(simsec, signalGroupID, colorIndex);
        }

        /// <summary>
        /// Temp class for converting
        /// </summary>
        private class LSA
        {
            public float simsec;
            public int signalGroupID;
            public int colorIndex;

            public LSA(float simsec, int signalGroupID, int colorIndex)
            {
                this.simsec = simsec;
                this.signalGroupID = signalGroupID;
                this.colorIndex = colorIndex;
            }
        }
    }
}
