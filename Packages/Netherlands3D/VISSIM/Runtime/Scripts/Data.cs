using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        /// Unique vehicle number (in data called "NO")
        /// </summary>
        [Tooltip("Unique entity number (in data called \"NO\")")]
        public int id;
        /// <summary>
        /// The vehicle type 100 = Car; 200 = Truck; 300 = Bus; 400 = Tram; 500 = Pedestrian; 600 = Cycle; 700 = Van;
        /// </summary>
        [Tooltip("The entity type 100 = Car; 200 = Truck; 300 = Bus; 400 = Tram; 500 = Pedestrian; 600 = Cycle; 700 = Van;")]
        public int entityTypeIndex;
        /// <summary>
        /// Vehicle width in meters, depending on 2D/3D model distribution. The width is relevant for overtaking within the lane
        /// </summary>
        [Tooltip("Entity width in meters (x-axis), depending on 2D/3D model distribution. The width is relevant for overtaking within the lane")]
        public float width;
        [Tooltip("The entity length in meters (z-axis)")]
        public float length;
        /// <summary>
        /// The coordinates of the entity with corresponding key simulation second <simulationSecond, StartEndPos
        /// </summary>
        /// <remarks>
        /// float: The simulation time in seconds
        /// FrontEndCoordinates: contains the coordinatesFront and coordinatesRear
        /// </remarks>
        public Dictionary<float, Coordinates> coordinates; //TODO make this showup in inspector

        public Data(int id, int entityTypeIndex, float width, Dictionary<float, Coordinates> coordinates)
        {
            this.id = id;
            this.entityTypeIndex = entityTypeIndex;
            this.width = width;
            if(coordinates.ContainsKey(coordinates.First().Key))
            {
                this.length = Vector3.Distance(coordinates.First().Value.coordinatesRear, coordinates.First().Value.coordinatesFront);
            }
            this.coordinates = coordinates;
        }
        
        
        [System.Serializable]
        public class Coordinates
        {
            /// <summary>
            /// Coordinate of front end of the entity at the end of the time step
            /// </summary>
            public Vector3 coordinatesFront;
            /// <summary>
            /// Coordinate of rear end position of the entity at the end of the time step
            /// </summary>
            public Vector3 coordinatesRear;
            /// <summary>
            /// The entitys center of mass position calculated by front/rear
            /// </summary>
            public Vector3 center;
            /// <summary>
            /// The direction of the entity
            /// </summary>
            public Vector3 direction;

            public Coordinates(Vector3 coordinatesFront, Vector3 coordinatesRear)
            {
                this.coordinatesFront = coordinatesFront;
                this.coordinatesRear = coordinatesRear;
                center = (coordinatesFront + coordinatesRear) / 2;
                direction = (coordinatesFront - coordinatesRear).normalized;
            }
        }        
    }

    /// <summary>
    /// Class to contain raw data before turning it into the class Data
    /// </summary>
    public class DataRaw
    {
        public float simulationSecond;
        public int id;
        public int vehicleTypeIndex;
        public Vector3 coordinatesFront;
        public Vector3 coordinatesRear;
        public float width;

        public DataRaw(float simulationSecond, int id, int vehicleTypeIndex, Vector3 coordinatesFront, Vector3 coordinatesRear, float width)
        {
            this.simulationSecond = simulationSecond;
            this.id = id;
            this.vehicleTypeIndex = vehicleTypeIndex;
            this.coordinatesFront = coordinatesFront;
            this.coordinatesRear = coordinatesRear;
            this.width = width;
        }
    }
}
