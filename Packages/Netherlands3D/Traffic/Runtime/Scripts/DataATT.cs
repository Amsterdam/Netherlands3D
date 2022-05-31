using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Traffic.VISSIM
{
    /// <summary>
    /// Data class for an .att file
    /// </summary>
    public class DataATT
    {
        // * Number;Signal group;Lane width;Rotation angle;WKT location
        /// <summary>
        /// The id number of the signal head
        /// </summary>
        public int number;
        /// <summary>
        /// The corresponding group in correct format
        /// </summary>
        public int signalGroup;
        /// <summary>
        /// The entire signal group number
        /// </summary>
        public int signalGroupID;
        /// <summary>
        /// The corresponding group in string fully
        /// </summary>
        public string signalGroupString;
        /// <summary>
        /// The lane width the signal head is assigned too
        /// </summary>
        public float laneWidth;
        /// <summary>
        /// The rotation angle of the lane (north = 0deg, clockwise)
        /// </summary>
        public float rotationAngle;
        /// <summary>
        /// The location of the signal head
        /// </summary>
        public Vector2 wktLocation;

        public DataATT(int number, string signalGroupString, float laneWidth, float rotationAngle, Vector2 wktLocation)
        {
            this.number = number;
            this.signalGroupString = signalGroupString;
            // Convert sgString to sg
            int dashIndex = signalGroupString.IndexOf("-");
            int.TryParse(signalGroupString.Substring(0, dashIndex), out signalGroup);
            int.TryParse(signalGroupString.Substring(dashIndex + 1, signalGroupString.Length - dashIndex - 1), out signalGroupID);
            Debug.Log(signalGroup + " " + signalGroupID);
            this.laneWidth = laneWidth;
            this.rotationAngle = rotationAngle;
            this.wktLocation = wktLocation;
        }
    }
}
