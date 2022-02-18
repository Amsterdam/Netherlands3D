using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Netherlands3D.Core;
using System.Linq;
using System.IO;
using System;

namespace Netherlands3D.VISSIM
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
        /// <param name="file"></param>
        public static IEnumerator Convert(string filePath)
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
                if(VISSIMManager.MaxDatasCount != -1 && convertedData.Count >= VISSIMManager.MaxDatasCount) break;

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
                            Debug.LogWarning("[VISSIM] Found a coordination duplicate.");
                            // This is caused by the entity id having 2 of the same simulation seconds in the data
                            // Make sure that each entity id has unique simsec (simulation seconds)
                            continue;
                        }

                        convertedData[dataRaw.id].coordinates.Add(dataRaw.simulationSecond, new Data.Coordinates(dataRaw.coordinatesFront, dataRaw.coordinatesRear));
                    }
                    else
                    {
                        // Doesnt exist, add it
                        convertedData.Add(dataRaw.id, new Data(dataRaw.id, dataRaw.vehicleTypeIndex, dataRaw.width,
                            new Dictionary<float, Data.Coordinates>()
                            { { dataRaw.simulationSecond, new Data.Coordinates(dataRaw.coordinatesFront, dataRaw.coordinatesRear) } }));
                        Debug.Log("add data");
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
            VISSIMManager.AddData(convertedData);

            // Check if there are missing Vissim entity ids //TODO move to mangager.add?
            if(VISSIMManager.MissingEntityIDs.Count > 0)
            {
                //vissimConfiguration.OpenInterface(missingVissimTypes); // opens missing visism interface
            }
            else
            {
                //StartVissim(); // starts animation
            }
                        
            yield break;
        }

        /// <summary>
        /// Converts a data string to data
        /// </summary>
        /// <param name="dataString"></param>
        /// <returns>Data</returns>
        public static DataRaw ConvertToDataRaw(string dataString)
        {
            string[] array = dataString.Split(';');
            float simulationSeconds = float.Parse(array[0], CultureInfo.InvariantCulture);
            int vehicleTypeIndex = int.Parse(array[2]);
            // Check if ID isn't set, then store it in missingEntityIDs
            VISSIMManager.CheckEntityTypeIndex(vehicleTypeIndex);

            return new DataRaw(simulationSeconds, int.Parse(array[1]), vehicleTypeIndex, StringToVector3(array[3]), StringToVector3(array[4]), float.Parse(array[5])); //TODO error handling if parsing doesnt work
        }

        /// <summary>
        /// Converts the VISSIM RD coordinate string into a Vector3.
        /// </summary>
        /// <param name="s">The string to convert</param>
        /// <returns>Vector3</returns>
        public static Vector3 StringToVector3(string s)
        {
            //0 value is X
            //1 value is Y
            //2 value is Z
            //stringVector = stringVector.Replace(".", ","); // Transforms decimal from US standard which uses a Period to European with a Comma

            string[] splitString = s.Split(' '); // Splits the string into individual vectors
            double x = double.Parse(splitString[0], CultureInfo.InvariantCulture);
            double y = double.Parse(splitString[1], CultureInfo.InvariantCulture);
            double z = double.Parse(splitString[2], CultureInfo.InvariantCulture);
            Vector3RD rdVector = new Vector3RD(x, y, z); // Creates the Double Vector
            // Convert fzp vector3(x,z,y) to unity vector3(x,y,z)
            Vector3 convertedCoordinates = CoordConvert.RDtoUnity(rdVector);
            // Y Coordinates will be calculated by the vehicle to connect with the Map (Maaiveld).

            return convertedCoordinates;
        }
    }
}
