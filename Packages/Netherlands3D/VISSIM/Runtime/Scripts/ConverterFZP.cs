using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Netherlands3D.Core;

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
        public static float frameCounter = 0.0f;
        public static float timeBetweenFrames = 0.0f;

        /// <summary>
        /// Reads the file with VISSIM data and converts it to useable vissim data
        /// </summary>
        /// <param name="file"></param>
        public static IEnumerator Convert(string file)
        {
            // Split the file into lines
            string[] lines = file.Split((System.Environment.NewLine + "\n" + "\r").ToCharArray());
            
            // Go through each line and add the data to VISSIMManager
            bool readyToConvert = false;
            foreach(string line in lines)
            {
                // Check line
                if(readyToConvert && !string.IsNullOrEmpty(line))
                {
                    VISSIMManager.AddData(line);//TODO change -> save in list, then add when all is over, then call visualizer updateData shizz
                    // Wait a frame to not make the project freeze
                    yield return null;
                }
                // Check if the line template string is the same as the requiredTemplate, if so start adding //TODO can this be improved by regex?
                if(line == VISSIMManager.RequiredTemplate)
                {
                    readyToConvert = true;
                }

                // Check if limit has been reached
                if(VISSIMManager.DatasReachedMaxCount) break;
            }

            // Automatically calculates the time between the frames.
            foreach(Data data in VISSIMManager.Datas) //TODO this can be calculated in VISSIMManager.AddData
            {
                if(data.simulationSeconds != VISSIMManager.Datas[0].simulationSeconds)
                {
                    timeBetweenFrames = data.simulationSeconds - VISSIMManager.Datas[0].simulationSeconds;
                    break; // after calculating the correct framerate of the simulation, exit the loop.
                }
            }

            // Check if there are missing Vissim entity ids
            if(VISSIMManager.MissingEntityIDs.Count > 0)
            {
                //vissimConfiguration.OpenInterface(missingVissimTypes); // opens missing visism interface
            }
            else
            {
                //StartVissim(); // starts animation
            }

            // Set the current VISSIM file start parameters
            frameCounter = VISSIMManager.Datas[0].simulationSeconds - timeBetweenFrames; // Some simulations start at a different simsec depending on the population of the simulation. This makes sure that it will always start at the 1st frame

            // 

            yield break;
        }

        /// <summary>
        /// Converts a data string to data
        /// </summary>
        /// <param name="dataString"></param>
        /// <returns>Data</returns>
        public static Data ConvertToData(string dataString)
        {
            string[] array = dataString.Split(';');
            float simulationSeconds = float.Parse(array[0], CultureInfo.InvariantCulture);
            int vehicleTypeIndex = int.Parse(array[2]);
            // Check if ID isn't set, then store it in missingEntityIDs
            if(!VISSIMManager.Instance.availableEntitiesData.ContainsKey(vehicleTypeIndex) && !VISSIMManager.Instance.missingEntityIDs.Contains(vehicleTypeIndex)) VISSIMManager.Instance.missingEntityIDs.Add(vehicleTypeIndex);

            return new Data(simulationSeconds, int.Parse(array[1]), vehicleTypeIndex, StringToVector3(array[3]), StringToVector3(array[4]), float.Parse(array[5])); //TODO error handling if parsing doesnt work
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
            Vector3 convertedCoordinates = CoordConvert.RDtoUnity(rdVector);
            // Y Coordinates will be calculated by the vehicle to connect with the Map (Maaiveld).

            return convertedCoordinates;
        }

    }
}
