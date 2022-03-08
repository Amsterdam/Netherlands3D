using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Core;
using SimpleJSON;

namespace Netherlands3D.Traffic.Simulation
{
    /// <summary>
    /// Contains data about a road and its points
    /// </summary>
    [System.Serializable]
    public class Road
    {
        /// <summary>
        /// The name of the road
        /// </summary>
        public string name;
        /// <summary>
        /// The main coordinate of the road
        /// </summary>
        public Vector3 coordinate;
        /// <summary>
        /// The road points
        /// </summary>
        public List<Point> points = new List<Point>();

        /// <summary>
        /// Road constructor
        /// </summary>
        /// <param name="json">The json data</param>
        public Road(JSONNode json)
        {
            coordinate = CoordConvert.WGS84toUnity(json["geometry"]["lon"], json["geometry"]["lat"]);
            name = json["tags"]["name"];

            // Loop through points
            for(int i = 0; i < json["geometry"].Count; i++)
            {
                points.Add(new Point(json["geometry"][i]["lon"], json["geometry"][i]["lat"], coordinate));
            }
        }

        /// <summary>
        /// A road point
        /// </summary>
        [System.Serializable]
        public class Point
        {
            /// <summary>
            /// The coordinate
            /// </summary>
            public Vector3 coordinate;
            /// <summary>
            /// The coordinate but converted into unity measurement
            /// </summary>
            public Vector3 coordinateUnity;

            public Point(double longitude, double latitude, Vector3 roadCoord)
            {
                coordinate = CoordConvert.WGS84toUnity(longitude, latitude);
                //coords.y = Config.activeConfiguration.zeroGroundLevelY; TODO the config script is not found
                coordinateUnity = CoordConvert.RDtoUnity(coordinate);
            }
        }
    }
}
