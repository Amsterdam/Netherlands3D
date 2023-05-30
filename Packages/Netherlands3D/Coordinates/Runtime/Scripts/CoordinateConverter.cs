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
    /// <summary>
    /// A facade that allows for easy conversion of a Coordinate in one CoordinateSystem to another using the ConvertTo
    /// method.
    /// </summary>
    public static class CoordinateConverter
    {
        [Obsolete("zeroGroundLevelY() is deprecated, please use EPSG7415.zeroGroundLevelY()")]
        public static float zeroGroundLevelY
        {
            get => EPSG7415.zeroGroundLevelY;
            set => EPSG7415.zeroGroundLevelY = value;
        }

        [Obsolete("relativeCenterRD() is deprecated, please use EPSG7415.relativeCenterRD()")]
        public static Vector2RD relativeCenterRD {
            get => EPSG7415.relativeCenter;
            set => EPSG7415.relativeCenter = value;
        }

        /// <summary>
        /// Convert the given Coordinate to its the given Coordinate System, represented by the identifier provided by
        /// the EPSG; such as 7415 for EPSG:7415, also known as 3D Rijksdriehoek coordinates (RD).
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="targetCrs">The identifier provided by the EPSG</param>
        /// <exception cref="ArgumentOutOfRangeException">If conversion for the involved Coordinate Systems is not supported.</exception>
        public static Coordinate ConvertTo(Coordinate coordinate, int targetCrs)
        {
            // Nothing to do if the coordinate system didn't change.
            if (coordinate.CoordinateSystem == targetCrs) return coordinate;

            // In this iteration of the package, this is a hardcoded switch. Martijn is working on a new conversion
            // backend and as soon as we integrate that the hardcoded conversions can be removed
            return coordinate.CoordinateSystem switch
            {
                (int)CoordinateSystem.Unity => Unity.ConvertTo(coordinate, targetCrs),
                (int)CoordinateSystem.WGS84 => WGS84.ConvertTo(coordinate, targetCrs),
                (int)CoordinateSystem.RD => EPSG7415.ConvertTo(coordinate, targetCrs),
                (int)CoordinateSystem.EPSG_4936 => EPSG4936.ConvertTo(coordinate, targetCrs),
                _ => throw new ArgumentOutOfRangeException(
                    $"Conversion between CRS ${coordinate.CoordinateSystem} and ${targetCrs} is not yet supported")
            };
        }

        /// <summary>
        /// Convert the given Coordinate to its the given Coordinate System, represented by a value in the
        /// CoordinateSystem enum.
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="targetCrs"></param>
        /// <exception cref="ArgumentOutOfRangeException">If conversion for the involved Coordinate Systems is not supported.</exception>
        public static Coordinate ConvertTo(Coordinate coordinate, CoordinateSystem targetCrs)
        {
            return ConvertTo(coordinate, (int)targetCrs);
        }

        /// <summary>
        /// Converts WGS84-coordinate to UnityCoordinate
        /// </summary>
        /// <param name="coordinate">Vector2 WGS-coordinate</param>
        /// <returns>Vector3 Unity-Coordinate (y=0)</returns>
        [Obsolete("WGS84toUnity() is deprecated, please use ConvertTo()")]
        public static Vector3 WGS84toUnity(Vector2 coordinate)
        {
            var source = new Coordinate(CoordinateSystem.WGS84, coordinate.x, coordinate.y, 0);

            return ConvertTo(source, CoordinateSystem.Unity).ToVector3();
        }

        /// <summary>
        /// Converts WGS84-coordinate to UnityCoordinate
        /// </summary>
        /// <param name="coordinate">Vector3 WGS-coordinate</param>
        /// <returns>Vector3 Unity-Coordinate</returns>
        [Obsolete("WGS84toUnity() is deprecated, please use ConvertTo")]
        public static Vector3 WGS84toUnity(Vector3 coordinate)
        {
            var source = new Coordinate(CoordinateSystem.WGS84, coordinate.x, coordinate.y, coordinate.z);

            return ConvertTo(source, CoordinateSystem.Unity).ToVector3();
        }

        /// <summary>
        /// Converts WGS84-coordinate to UnityCoordinate
        /// </summary>
        /// <param name="coordinate">Vector3RD WGS-coordinate</param>
        /// <returns>Vector Unity-Coordinate</returns>
        [Obsolete("WGS84toUnity() is deprecated, please use ConvertTo()")]
        public static Vector3 WGS84toUnity(Vector3WGS coordinate)
        {
            var source = new Coordinate(CoordinateSystem.WGS84, coordinate.lon, coordinate.lat, coordinate.h);

            return ConvertTo(source, CoordinateSystem.Unity).ToVector3();
        }

        /// <summary>
        /// Converts WGS84-coordinate to UnityCoordinate
        /// </summary>
        /// <param name="lon">double lon (east-west)</param>
        /// <param name="lat">double lat (south-north)</param>
        /// <returns>Vector3 Unity-Coordinate at 0-NAP</returns>
        ///
        [Obsolete("WGS84toUnity() is deprecated, please use ConvertTo()")]
        public static Vector3 WGS84toUnity(double lon, double lat)
        {
            var source = new Coordinate(CoordinateSystem.WGS84, lon, lat, 0);

            return ConvertTo(source, CoordinateSystem.Unity).ToVector3();
        }

        /// <summary>
        /// Convert RD-coordinate to Unity-Coordinate
        /// </summary>
        /// <param name="coordinates">RD-coordinate</param>
        /// <returns>UnityCoordinate</returns>
        [Obsolete("RDtoUnity() is deprecated, please use ConvertTo()")]
        public static Vector3 RDtoUnity(Vector3 coordinate)
        {
            var source = new Coordinate(CoordinateSystem.EPSG_7415, coordinate.x, coordinate.y, coordinate.z);

            return ConvertTo(source, CoordinateSystem.Unity).ToVector3();
        }

        /// <summary>
        /// Convert RD-coordinate to Unity-Coordinate
        /// </summary>
        /// <param name="coordinate">Vector3RD RD-Coordinate XYH</param>
        /// <returns>Vector3 Unity-Coordinate</returns>
        [Obsolete("RDtoUnity() is deprecated, please use ConvertTo()")]
        public static Vector3 RDtoUnity(Vector3RD coordinate)
        {
            var source = new Coordinate(CoordinateSystem.EPSG_7415, coordinate.x, coordinate.y, coordinate.z);

            return ConvertTo(source, CoordinateSystem.Unity).ToVector3();
        }

        /// <summary>
        /// Convert RD-coordinate to Unity-Coordinate
        /// </summary>
        /// <param name="coordinate">Vector3RD RD-Coordinate XYH</param>
        /// <returns>Vector3 Unity-Coordinate</returns>
        [Obsolete("RDtoUnity() is deprecated, please use ConvertTo()")]
        public static Vector3 RDtoUnity(Vector2RD coordinate)
        {
            var source = new Coordinate(CoordinateSystem.EPSG_7415, coordinate.x, coordinate.y, 0);

            return ConvertTo(source, CoordinateSystem.Unity).ToVector3();
        }

        /// <summary>
        /// Convert RD-coordinate to Unity-coordinate
        /// </summary>
        /// <param name="coordinate">RD-coordinate XY</param>
        /// <returns>Unity-Coordinate</returns>
        [Obsolete("RDtoUnity() is deprecated, please use ConvertTo()")]
        public static Vector3 RDtoUnity(Vector2 coordinate)
        {
            var source = new Coordinate(CoordinateSystem.EPSG_7415, coordinate.x, coordinate.y, 0);

            return ConvertTo(source, CoordinateSystem.Unity).ToVector3();
        }

        /// <summary>
        /// Convert RD-coordinate to Unity-coordinate
        /// </summary>
        /// <param name="x">RD X-coordinate</param>
        /// <param name="y">RD Y-coordinate</param>
        /// <param name="z">RD elevation</param>
        /// <returns>Unity-Coordinate</returns>
        [Obsolete("RDtoUnity() is deprecated, please use ConvertTo()")]
        public static Vector3 RDtoUnity(double x, double y, double z)
        {
            var source = new Coordinate(CoordinateSystem.EPSG_7415, x, y, z);

            return ConvertTo(source, CoordinateSystem.Unity).ToVector3();
        }

        /// <summary>
        /// Converts Unity-Coordinate to WGS84-Coordinate
        /// </summary>
        /// <param name="coordinate">Unity-coordinate XHZ</param>
        /// <returns>WGS-coordinate</returns>
        [Obsolete("UnitytoWGS84() is deprecated, please use ConvertTo")]
        public static Vector3WGS UnitytoWGS84(Vector3 coordinate)
        {
            var source = new Coordinate(CoordinateSystem.Unity, coordinate.x, coordinate.y, coordinate.z);

            return ConvertTo(source, CoordinateSystem.WGS84).ToVector3WGS();
        }

        /// <summary>
        /// Converts Unity-Coordinate to RD-coordinate
        /// </summary>
        /// <param name="coordinate">Unity-Coordinate</param>
        /// <returns>RD-coordinate</returns>
        [Obsolete("UnitytoRD() is deprecated, please use ConvertTo()")]
        public static Vector3RD UnitytoRD(Vector3 coordinate)
        {
            var source = new Coordinate(CoordinateSystem.Unity, coordinate.x, coordinate.y, coordinate.z);

            return ConvertTo(source, CoordinateSystem.EPSG_7415).ToVector3RD();
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
            var source = new Coordinate(CoordinateSystem.EPSG_7415, x, y, nap);

            return ConvertTo(source, CoordinateSystem.WGS84).ToVector3WGS();
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
            var source = new Coordinate(CoordinateSystem.WGS84, lon, lat, 0);

            return ConvertTo(source, CoordinateSystem.EPSG_7415).ToVector3RD();
        }

        [Obsolete("ecefRotionToUp() is deprecated, please use ECEF.RotationToUp()")]
        public static Quaternion ecefRotionToUp()
        {
            return EPSG4936.RotationToUp();
        }

        [Obsolete("ECEFToUnity() is deprecated, please use ConvertTo()")]
        public static Vector3 ECEFToUnity(Vector3ECEF coordinate)
        {
            var source = new Coordinate(CoordinateSystem.EPSG_4936, coordinate.X, coordinate.Y, coordinate.Z);

            return ConvertTo(source, CoordinateSystem.Unity).ToVector3();
        }

        [Obsolete("UnityToECEF() is deprecated, please use ConvertTo")]
        public static Vector3ECEF UnityToECEF(Vector3 coordinate)
        {
            var source = new Coordinate(CoordinateSystem.Unity, coordinate.x, coordinate.y, coordinate.z);

            return ConvertTo(source, CoordinateSystem.EPSG_4936).ToVector3ECEF();
        }

        [Obsolete("WGS84toECEF() is deprecated, please use ConvertTo()")]
        public static Vector3ECEF WGS84toECEF(Vector3WGS coordinate)
        {
            var source = new Coordinate(CoordinateSystem.WGS84, coordinate.lon, coordinate.lat, coordinate.h);

            return ConvertTo(source, CoordinateSystem.EPSG_4936).ToVector3ECEF();
        }

        [Obsolete("ECEFtoWGS84() is deprecated, please use ConvertTo()")]
        public static Vector3WGS ECEFtoWGS84(Vector3ECEF coordinate)
        {
            var source = new Coordinate(CoordinateSystem.EPSG_4936, coordinate.X, coordinate.Y, coordinate.Z);

            return ConvertTo(source, CoordinateSystem.WGS84).ToVector3WGS();
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
