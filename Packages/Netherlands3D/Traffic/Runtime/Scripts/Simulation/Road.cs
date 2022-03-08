using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Core;

namespace Netherlands3D.Traffic.Simulation
{
    /// <summary>
    /// Contains data about a road and its points
    /// </summary>
    public class Road
    {
        
        /// <summary>
        /// A road point
        /// </summary>
        public class Point
        {
            public Vector3 coordinate;

            public Point(double longitude, double latitude)
            {
                var coords = CoordConvert.WGS84toUnity(longitude, latitude);
                //coords.y = Config.activeConfiguration.zeroGroundLevelY; TODO the config script is not found
                coordinate = coords;
            }
        }
    }
}
