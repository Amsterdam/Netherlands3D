using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Netherlands3D.Core;
using System.Linq;
using System.IO;
using System;
using Netherlands3D.Events;

namespace Netherlands3D.Traffic
{
    /// <summary>
    /// For converting .fzp files
    /// </summary>
    /// <remarks>
    /// FOR THIS VISSIM SIMULATION, USE THE STANDARD TEMPLATE WITH THE FOLLOWING PARAMETERS ONLY.         
    /// $VEHICLE:SIMSEC;NO;VEHTYPE;COORDFRONT;COORDREAR;WIDTH
    /// </remarks>
    public static class ConverterFZP
    {
        private static readonly string requiredTemplate = "$VEHICLE:SIMSEC;NO;VEHTYPE;COORDFRONT;COORDREAR;WIDTH";

        /// <summary>
        /// Reads the file.fzp with VISSIM data and converts it to useable vissim data
        /// </summary>
        /// <param name="filePath">The file to convert</param>
        /// <param name="maxDataCount">The max amount of data to be extracted. -1 = infinity</param>
        /// <param name="callback">The callback that gets triggerd when the data is collected.</param>
        public static IEnumerator Convert(string filePath, int maxDataCount, Action<Dictionary<int, Data>> callback)
        {
            // Convert filePath to fileContent
            using StreamReader sr = new StreamReader(filePath);
            bool readyToConvert = false;
            string line;
            DataRaw dataRaw;
            Dictionary<int, Data> convertedData = new Dictionary<int, Data>();
            // Read and display lines from the file until the end of the file is reached.
            while((line = sr.ReadLine()) != null)
            {
                // Check if limit has been reached
                if(maxDataCount != -1 && convertedData.Count >= maxDataCount) break;

                // Check line contents
                if(readyToConvert && !string.IsNullOrEmpty(line))
                {
                    dataRaw = ConvertToDataRaw(line);
                    // Check if dataRaw id is already in convertedData
                    if(convertedData.ContainsKey(dataRaw.id))
                    {
                        // Already exists, add new simulation second to its coordinates
                        // But check if coordinates arent already filled in if file contains data bugs/duplicates
                        if(convertedData[dataRaw.id].coordinates.ContainsKey(dataRaw.simulationSecond))
                        {
                            Debug.LogWarning("[Traffic] Found a coordination duplicate. Make sure that each entity has its own unique simulation seconds.");
                            // This is caused by the entity id having 2 of the same simulation seconds in the data
                            // Make sure that each entity id has unique simsec (simulation seconds)
                            continue;
                        }

                        // Add the new simulation second to its coordinates
                        convertedData[dataRaw.id].coordinates.Add(dataRaw.simulationSecond, new Data.Coordinates(dataRaw.coordinatesFront, dataRaw.coordinatesRear));
                    }
                    else
                    {
                        // Doesnt exist, add it
                        convertedData.Add(dataRaw.id, new Data(dataRaw.id, dataRaw.vehicleTypeIndex, dataRaw.width,
                            new Dictionary<float, Data.Coordinates>()
                            { { dataRaw.simulationSecond, new Data.Coordinates(dataRaw.coordinatesFront, dataRaw.coordinatesRear) } }));
                    }

                    //yield return null; // Wait a frame to not make the project freeze // Not needed since its fast now, maybe toggle it when handing very large data all at once?
                }

                // Check if the line template string is the same as the requiredTemplate, if so start adding //TODO can this be improved by regex?
                if(line == requiredTemplate)
                {
                    readyToConvert = true;
                }
            }

            // Add data to VISSIM
            callback(convertedData);

            yield break;
        }

        /// <summary>
        /// Converts a data string to data
        /// </summary>
        /// <param name="dataString">The string containing data to be converted</param>
        /// <returns>DataRaw</returns>
        private static DataRaw ConvertToDataRaw(string dataString)
        {
            string[] array = dataString.Split(';');
            float simulationSeconds = float.Parse(array[0], CultureInfo.InvariantCulture);
            int vehicleTypeIndex = int.Parse(array[2]);
            // Check if ID isn't set, then store it in missingEntityIDs
            //VISSIMManager.CheckEntityTypeIndex(vehicleTypeIndex); TODO

            return new DataRaw(simulationSeconds, int.Parse(array[1]), vehicleTypeIndex, StringToVector3(array[3]), StringToVector3(array[4]), float.Parse(array[5])); //TODO error handling if parsing doesnt work
        }

        /// <summary>
        /// Get the vector3 from a string
        /// </summary>
        /// <param name="s">The string to convert</param>
        /// <returns>Vector3</returns>
        private static Vector3 StringToVector3(string s)
        {
            string[] splitString = s.Split(' '); // Splits the string into individual vectors
            double x = double.Parse(splitString[0], CultureInfo.InvariantCulture);
            double y = double.Parse(splitString[1], CultureInfo.InvariantCulture);
            double z = double.Parse(splitString[2], CultureInfo.InvariantCulture);

            return new Vector3((float)x, (float)z, (float)y);
        }
    }
}
