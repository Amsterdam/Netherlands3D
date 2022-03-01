using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Traffic
{
    /// <summary>
    /// Extension class for Data
    /// </summary>
    public static class DataExtensions
    {
        /// <summary>
        /// Add coordinates to a data class
        /// </summary>
        /// <param name="data">The data class to add it to</param>
        /// <param name="newCoordinates">The new Data.coordinates</param>
        public static void AddCoordinates(this Data data, Dictionary<float, Data.Coordinates> newCoordinates)
        {
            foreach(var item in newCoordinates)
            {
                // Check for duplicate coordinates
                if(data.coordinates.ContainsKey(item.Key)) continue; // Or override them?

                data.coordinates.Add(item.Key, item.Value);
            }
        }
    }
}
