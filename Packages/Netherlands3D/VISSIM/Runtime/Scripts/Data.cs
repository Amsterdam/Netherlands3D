using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.VISSIM
{
    /// <summary>
    /// Class containing VISSIM data
    /// </summary>
    [System.Serializable]
    public class Data
    {
        // $VEHICLE:SIMSEC;NO;VEHTYPE;COORDFRONT;COORDREAR;WIDTH

        /// <summary>
        /// The simulation time in seconds
        /// </summary>
        [Tooltip("The simulation time in seconds")]
        public float simulationSeconds;
        /// <summary>
        /// Unique vehicle number (in data called "NO")
        /// </summary>
        [Tooltip("Unique vehicle number (in data called \"NO\")")]
        public int id;
        /// <summary>
        /// The vehicle type 100 = Car; 200 = Truck; 300 = Bus; 400 = Tram; 500 = Pedestrian; 600 = Cycle; 700 = Van;
        /// </summary>
        [Tooltip("The vehicle type 100 = Car; 200 = Truck; 300 = Bus; 400 = Tram; 500 = Pedestrian; 600 = Cycle; 700 = Van;")]
        public int vehicleTypeIndex;
        /// <summary>
        /// Coordinate of front end of vehicle at the end of the time step
        /// </summary>
        [Tooltip("Coordinate of front end of vehicle at the end of the time step")]
        public Vector3 coordinatesFront;
        /// <summary>
        /// Coordinate of rear end position of vehicle at the end of the time step
        /// </summary>
        [Tooltip("Coordinate of rear end position of vehicle at the end of the time step")]
        public Vector3 coordinatesRear;
        /// <summary>
        /// Vehicle width in meters, depending on 2D/3D model distribution. The width is relevant for overtaking within the lane
        /// </summary>
        [Tooltip("Vehicle width in meters, depending on 2D/3D model distribution. The width is relevant for overtaking within the lane")]
        public float width;

        public Data(float simulationSeconds, int id, int vehicleType, Vector3 coordinatesFront, Vector3 coordinatesRear, float width)
        {
            this.simulationSeconds = simulationSeconds;
            this.id = id;
            this.vehicleTypeIndex = vehicleType;
            this.coordinatesFront = coordinatesFront;
            this.coordinatesRear = coordinatesRear;
            this.width = width;
        }
    }
}
