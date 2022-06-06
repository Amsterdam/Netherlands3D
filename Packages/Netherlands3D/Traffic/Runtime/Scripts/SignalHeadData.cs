using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Traffic.VISSIM
{
    /// <summary>
    /// Data class for an .att file
    /// </summary>
    [System.Serializable]
    public class SignalHeadData
    {
        // * Number;Signal group;Lane width;Rotation angle;WKT location
        /// <summary>
        /// The id number of the signal head
        /// </summary>
        public int number;
        /// <summary>
        /// The corresponding group in correct format 123
        /// </summary>
        public int group;
        /// <summary>
        /// The entire signal group number 12301
        /// </summary>
        public int groupID;
        /// <summary>
        /// The corresponding group in string fully
        /// </summary>
        public string groupString;
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

        /// <summary>
        /// Tells what color the signal head needs to be at a simulation second
        /// </summary>
        /// <remarks><simulationsecond, index of color></remarks>
        public Dictionary<float, int> schedule = new Dictionary<float, int>();

        public SignalHeadData(int number, string groupString, float laneWidth, float rotationAngle, Vector2 wktLocation)
        {
            this.number = number;
            this.groupString = groupString;
            // Convert sgString to sg
            int dashIndex = groupString.IndexOf("-");
            int.TryParse(groupString.Substring(0, dashIndex), out group);
            int.TryParse(groupString.Substring(dashIndex + 1, groupString.Length - dashIndex - 1), out groupID);
            Debug.Log(group + " " + groupID);
            this.laneWidth = laneWidth;
            this.rotationAngle = rotationAngle;
            this.wktLocation = wktLocation;
        }
    }
}
