using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


            yield break;
        }
    }
}
