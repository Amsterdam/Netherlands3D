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
    /// Convert coordinates between Unity, ECEF, WGS84 and RD(EPSG7415)
    /// <!-- accuracy: WGS84 to RD  X <0.01m, Y <0.02m H <0.03m, tested in Amsterdam with PCNapTrans-->
    /// <!-- accuracy: RD to WGS84  X <0.01m, Y <0.02m H <0.03m, tested in Amsterdam with PCNapTrans-->
    /// </summary>
    public static class Unity
    {
        public static Vector3ECEF ToECEF(Vector3 point)
        {
            var temppoint = Quaternion.Inverse(EPSG4936.RotationToUp()) * point;
            Vector3ECEF ecef = new Vector3ECEF();
            ecef.X = -temppoint.x + EPSG4936.relativeCenter.X;
            ecef.Y = -temppoint.z + EPSG4936.relativeCenter.Y;
            ecef.Z = temppoint.y + EPSG4936.relativeCenter.Z;

            return ecef;
        }

        /// <summary>
        /// Converts Unity-Coordinate to RD-coordinate
        /// </summary>
        /// <param name="coordinates">Unity-Coordinate</param>
        /// <returns>RD-coordinate</returns>
        public static Vector3RD ToEPSG7415(Vector3 coordinates)
        {
            return new Vector3RD
            {
                x = coordinates.x + EPSG7415.relativeCenter.x,
                y = coordinates.z + EPSG7415.relativeCenter.y,
                z = coordinates.y - EPSG7415.zeroGroundLevelY
            };
        }

        /// <summary>
        /// Converts Unity-Coordinate to WGS84-Coordinate
        /// </summary>
        /// <param name="coordinates">Unity-coordinate XHZ</param>
        /// <returns>WGS-coordinate</returns>
        public static Vector3WGS ToWGS84(Vector3 coordinates)
        {
            Vector3RD vectorRD = ToEPSG7415(coordinates);
            Vector3WGS output = EPSG7415.ToWGS84(vectorRD.x,vectorRD.y);
            double hoogteCorrectie = EPSG7415.RDCorrection(output.lon, output.lat, "Z", EPSG7415.RDCorrectionZ);
            output.h = vectorRD.z + hoogteCorrectie;

            return output;
        }

        public static Coordinate ConvertTo(Coordinate coordinate, int targetCrs)
        {
            if (coordinate.CoordinateSystem != (int)CoordinateSystem.Unity)
            {
                throw new ArgumentOutOfRangeException(
                    $"Invalid coordinate received, this class cannot convert CRS ${coordinate.CoordinateSystem}"
                );
            }

            var vector3 = coordinate.ToVector3();

            switch (targetCrs)
            {
                case (int)CoordinateSystem.WGS84:
                {
                    var result = ToWGS84(vector3);
                    return new Coordinate(targetCrs, result.lon, result.lat, result.h);
                }
                case (int)CoordinateSystem.EPSG_7415:
                {
                    var result = ToEPSG7415(vector3);
                    return new Coordinate(targetCrs, result.x, result.y, result.z);
                }
                case (int)CoordinateSystem.EPSG_4936:
                {
                    var result = ToECEF(vector3);
                    return new Coordinate(targetCrs, result.X, result.Y, result.Z);
                }
            }

            throw new ArgumentOutOfRangeException(
                $"Conversion between CRS ${coordinate.CoordinateSystem} and ${targetCrs} is not yet supported"
            );
        }

        public static Vector3 ToVector3(this Coordinate coordinate)
        {
            return new Vector3(
                (float)coordinate.Points[0],
                (float)coordinate.Points[1],
                (float)coordinate.Points[2]
            );
        }
    }
}
