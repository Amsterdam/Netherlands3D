/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/3DAmsterdam/blob/master/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/

using System;
using UnityEngine;

namespace Netherlands3D.Coordinates
{
    public static class CoordinateConverter
    {
        public static float zeroGroundLevelY
        {
            get => EPSG7415.zeroGroundLevelY;
            set => EPSG7415.zeroGroundLevelY = value;
        }

        public static Vector2RD relativeCenterRD {
            get => EPSG7415.relativeCenter;
            set => EPSG7415.relativeCenter = value;
        }

        public static Coordinate ConvertTo(Coordinate coordinate, int targetCrs)
        {
            // Nothing to do if the coordinate system didn't change.
            if (coordinate.CoordinateSystem == targetCrs) return coordinate;

            return coordinate.CoordinateSystem switch
            {
                (int)CoordinateSystem.WGS84 => WGS84.ConvertTo(coordinate, targetCrs),
                (int)CoordinateSystem.RD => EPSG7415.ConvertTo(coordinate, targetCrs),
                (int)CoordinateSystem.EPSG_4936 => EPSG4936.ConvertTo(coordinate, targetCrs),
                _ => throw new ArgumentOutOfRangeException(
                    $"Conversion between CRS ${coordinate.CoordinateSystem} and ${targetCrs} is not yet supported")
            };
        }

        public static Coordinate ConvertTo(Coordinate coordinate, CoordinateSystem targetCrs)
        {
            return ConvertTo(coordinate, (int)targetCrs);
        }

        /// <summary>
        /// Converts WGS84-coordinate to UnityCoordinate
        /// </summary>
        /// <param name="coordinate">Vector2 WGS-coordinate</param>
        /// <returns>Vector3 Unity-Coordinate (y=0)</returns>
        [Obsolete("WGS84toUnity() is deprecated, please use WGS84.ToUnity()")]
        public static Vector3 WGS84toUnity(Vector2 coordinate)
        {
            return WGS84.ToUnity(coordinate.x, coordinate.y);
        }

        /// <summary>
        /// Converts WGS84-coordinate to UnityCoordinate
        /// </summary>
        /// <param name="coordinate">Vector3 WGS-coordinate</param>
        /// <returns>Vector3 Unity-Coordinate</returns>
        [Obsolete("WGS84toUnity() is deprecated, please use WGS84.ToUnity()")]
        public static Vector3 WGS84toUnity(Vector3 coordinate)
        {
            return WGS84.ToUnity(coordinate);
        }

        /// <summary>
        /// Converts WGS84-coordinate to UnityCoordinate
        /// </summary>
        /// <param name="coordinate">Vector3RD WGS-coordinate</param>
        /// <returns>Vector Unity-Coordinate</returns>
        [Obsolete("WGS84toUnity() is deprecated, please use WGS84.ToUnity()")]
        public static Vector3 WGS84toUnity(Vector3WGS coordinate)
        {
            return WGS84.ToUnity(coordinate);
        }

        /// <summary>
        /// Converts WGS84-coordinate to UnityCoordinate
        /// </summary>
        /// <param name="lon">double lon (east-west)</param>
        /// <param name="lat">double lat (south-north)</param>
        /// <returns>Vector3 Unity-Coordinate at 0-NAP</returns>
        ///
        [Obsolete("WGS84toUnity() is deprecated, please use WGS84.ToUnity()")]
        public static Vector3 WGS84toUnity(double lon, double lat)
        {
            return WGS84.ToUnity(lon, lat);
        }

        /// <summary>
        /// Convert RD-coordinate to Unity-Coordinate
        /// </summary>
        /// <param name="coordinates">RD-coordinate</param>
        /// <returns>UnityCoordinate</returns>
        [Obsolete("RDtoUnity() is deprecated, please use EPSG7415.ToUnity()")]
        public static Vector3 RDtoUnity(Vector3 coordinates)
        {
            return EPSG7415.ToUnity(coordinates);
        }

        /// <summary>
        /// Convert RD-coordinate to Unity-Coordinate
        /// </summary>
        /// <param name="coordinate">Vector3RD RD-Coordinate XYH</param>
        /// <returns>Vector3 Unity-Coordinate</returns>
        [Obsolete("RDtoUnity() is deprecated, please use EPSG7415.ToUnity()")]
        public static Vector3 RDtoUnity(Vector3RD coordinate)
        {
            return EPSG7415.ToUnity(coordinate);
        }

        /// <summary>
        /// Convert RD-coordinate to Unity-Coordinate
        /// </summary>
        /// <param name="coordinate">Vector3RD RD-Coordinate XYH</param>
        /// <returns>Vector3 Unity-Coordinate</returns>
        [Obsolete("RDtoUnity() is deprecated, please use EPSG7415.ToUnity()")]
        public static Vector3 RDtoUnity(Vector2RD coordinate)
        {
            return EPSG7415.ToUnity(coordinate);
        }

        /// <summary>
        /// Convert RD-coordinate to Unity-coordinate
        /// </summary>
        /// <param name="coordinate">RD-coordinate XYH</param>
        /// <returns>Unity-Coordinate</returns>
        [Obsolete("RDtoUnity() is deprecated, please use EPSG7415.ToUnity()")]
        public static Vector3 RDtoUnity(Vector2 coordinate)
        {
            return EPSG7415.ToUnity(coordinate);
        }

        /// <summary>
        /// Convert RD-coordinate to Unity-coordinate
        /// </summary>
        /// <param name="x">RD X-coordinate</param>
        /// <param name="y">RD Y-coordinate</param>
        /// <param name="y">RD eleveation</param>
        /// <returns>Unity-Coordinate</returns>
        [Obsolete("RDtoUnity() is deprecated, please use EPSG7415.ToUnity()")]
        public static Vector3 RDtoUnity(double x, double y, double z)
        {
            return EPSG7415.ToUnity(x, y, z);
        }

        /// <summary>
        /// Converts Unity-Coordinate to WGS84-Coordinate
        /// </summary>
        /// <param name="coordinate">Unity-coordinate XHZ</param>
        /// <returns>WGS-coordinate</returns>
        [Obsolete("UnitytoWGS84() is deprecated, please use Unity.ToWGS84()")]
        public static Vector3WGS UnitytoWGS84(Vector3 coordinate)
        {
            return Unity.ToWGS84(coordinate);
        }

        /// <summary>
        /// Converts Unity-Coordinate to RD-coordinate
        /// </summary>
        /// <param name="coordinate">Unity-Coordinate</param>
        /// <returns>RD-coordinate</returns>
        [Obsolete("UnitytoRD() is deprecated, please use Unity.ToEPSG7415()")]
        public static Vector3RD UnitytoRD(Vector3 coordinate)
        {
            return Unity.ToEPSG7415(coordinate);
        }

        /// <summary>
        /// Converts RD-coordinate to WGS84-cordinate using the "benaderingsformules" from http://home.solcon.nl/pvanmanen/Download/Transformatieformules.pdf
        /// and X, Y, and Z correctiongrids
        /// </summary>
        /// <param name="x">RD-coordinate X</param>
        /// <param name="y">RD-coordinate Y</param>
        /// <returns>WGS84-coordinate</returns>
        [Obsolete("RDtoWGS84() is deprecated, please use ConvertTo()")]
        public static Vector3WGS RDtoWGS84(double x, double y, double nap = 0)
        {
            return EPSG7415.ToWGS84(x, y, nap);
        }

        /// <summary>
        /// Converts WGS84-coordinate to RD-coordinate using the "benaderingsformules" from http://home.solcon.nl/pvanmanen/Download/Transformatieformules.pdf
        /// and X, Y, and Z correctiongrids
        /// </summary>
        /// <param name="lon">Longitude (East-West)</param>
        /// <param name="lat">Lattitude (South-North)</param>
        /// <returns>RD-coordinate xyH</returns>
        ///
        [Obsolete("WGS84toRD() is deprecated, please use ConvertTo()")]
        public static Vector3RD WGS84toRD(double lon, double lat)
        {
            return WGS84.ToEPSG7415(lon, lat);
        }

        [Obsolete("ecefRotionToUp() is deprecated, please use ECEF.RotationToUp()")]
        public static Quaternion ecefRotionToUp()
        {
            return EPSG4936.RotationToUp();
        }

        [Obsolete("ECEFToUnity() is deprecated, please use ECEF.ToUnity()")]
        public static Vector3 ECEFToUnity(Vector3ECEF coordinate)
        {
            return EPSG4936.ToUnity(coordinate);
        }

        [Obsolete("UnityToECEF() is deprecated, please use Unity.ToECEF()")]
        public static Vector3ECEF UnityToECEF(Vector3 point)
        {
            return Unity.ToECEF(point);
        }

        [Obsolete("WGS84toECEF() is deprecated, please use ConvertTo()")]
        public static Vector3ECEF WGS84toECEF(Vector3WGS wgsCoordinate)
        {
            return WGS84.ToECEF(wgsCoordinate);
        }

        [Obsolete("ECEFtoWGS84() is deprecated, please use ConvertTo()")]
        public static Vector3WGS ECEFtoWGS84(Vector3ECEF coordinate)
        {
            return EPSG4936.ToWGS84(coordinate);
        }

        [Obsolete("RotationToUnityUP() is deprecated, please use WGS84.RotationToUp()")]
        public static Vector3 RotationToUnityUP(Vector3WGS position)
        {
            return WGS84.RotationToUp(position);
        }

        /// <summary>
        /// checks if RD-coordinate is within the defined valid region
        /// </summary>
        /// <param name="coordinate">RD-coordinate</param>
        /// <returns>true if coordinate is valid</returns>
        [Obsolete("RDIsValid() is deprecated, please use EPSG7415.IsValid()")]
        public static bool RDIsValid(Vector3RD coordinate)
        {
            return EPSG7415.IsValid(coordinate);
        }

        /// <summary>
        /// checks if WGS-coordinate is valid
        /// </summary>
        /// <param name="coordinate">Vector3 WGS84-coordinate</param>
        /// <returns>True if coordinate is valid</returns>
        [Obsolete("WGS84IsValid() is deprecated, please use WGS84.IsValid()")]
        public static bool WGS84IsValid(Vector3WGS coordinate)
        {
            return WGS84.IsValid(coordinate);
        }
    }
}
