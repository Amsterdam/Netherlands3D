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
    /// Convert coordinates between Unity, WGS84 and RD(EPSG7415)
    /// <!-- accuracy: WGS84 to RD  X <0.01m, Y <0.02m H <0.03m, tested in Amsterdam with PCNapTrans-->
    /// <!-- accuracy: RD to WGS84  X <0.01m, Y <0.02m H <0.03m, tested in Amsterdam with PCNapTrans-->
    /// </summary>
    public static class WGS84
    {
        //setup coefficients for ecef-calculation ETRD89
        private static double semimajorAxis = 6378137;
        private static double flattening = 0.003352810681183637418;
        private static double eccentricity = 0.0818191910428;

        //setup coefficients for X-calculation
        private static double[] Rp = new double[] { 0, 1, 2, 0, 1, 3, 1, 0, 2 };
        private static double[] Rq = new double[] { 1, 1, 1, 3, 0, 1, 3, 2, 3 };
        private static double[] Rpq = new double[] { 190094.945, -11832.228, -114.221, -32.391, -0.705, -2.340, -0.608, -0.008, 0.148 };
        //setup coefficients for Y-calculation
        private static double[] Sp = new double[] { 1, 0, 2, 1, 3, 0, 2, 1, 0, 1 };
        private static double[] Sq = new double[] { 0, 2, 0, 2, 0, 1, 2, 1, 4, 4 };
        private static double[] Spq = new double[] { 309056.544, 3638.893, 73.077, -157.984, 59.788, 0.433, -6.439, -0.032, 0.092, -0.054 };

        public static Vector3ECEF ToECEF(Vector3WGS wgsCoordinate)
        {
            Vector3ECEF result = new Vector3ECEF();
            double lattitude = wgsCoordinate.lat * Math.PI / 180;
            double longitude = wgsCoordinate.lon * Math.PI / 180;

            //EPSG dataset coordinate operation method code 9602)
            double primeVerticalRadius = semimajorAxis / (Math.Sqrt(1 - (Math.Pow(eccentricity, 2) * Math.Pow(Math.Sin(lattitude), 2))));
            result.X = (primeVerticalRadius + wgsCoordinate.h) * Math.Cos(lattitude) * Math.Cos(longitude);
            result.Y = (primeVerticalRadius + wgsCoordinate.h) * Math.Cos(lattitude) * Math.Sin(longitude);
            result.Z = ((1 - Math.Pow(eccentricity, 2)) * primeVerticalRadius + wgsCoordinate.h) * Math.Sin(lattitude);

            return result;
        }

        /// <summary>
        /// Converts WGS84-coordinate to RD-coordinate using the "benaderingsformules" from http://home.solcon.nl/pvanmanen/Download/Transformatieformules.pdf
        /// and X, Y, and Z correctiongrids
        /// </summary>
        /// <param name="lon">Longitude (East-West)</param>
        /// <param name="lat">Lattitude (South-North)</param>
        /// <returns>RD-coordinate xyH</returns>
        ///
        public static Vector3RD ToEPSG7415(double lon, double lat)
        {
            //coordinates of basepoint in RD
            double refRDX = 155000;
            double refRDY = 463000;

            //coordinates of basepoint in WGS84
            double refLon = 5.38720621;
            double refLat = 52.15517440;

            double DeltaLon = 0.36 * (lon - refLon);
            double DeltaLat = 0.36 * (lat - refLat);

            //calculate X
            double DeltaX = 0;
            for (int i = 0; i < Rpq.Length; i++)
            {
                DeltaX += Rpq[i] * Math.Pow(DeltaLat, Rp[i]) * Math.Pow(DeltaLon, Rq[i]);
            }
            double X = DeltaX + refRDX;

            //calculate Y
            double DeltaY = 0;
            for (int i = 0; i < Spq.Length; i++)
            {
                DeltaY += Spq[i] * Math.Pow(DeltaLat, Sp[i]) * Math.Pow(DeltaLon, Sq[i]);
            }
            double Y = DeltaY + refRDY;

            double correctionX = EPSG7415.RDCorrection(X, Y, "X", EPSG7415.RDCorrectionX);
            double correctionY = EPSG7415.RDCorrection(X,Y, "Y", EPSG7415.RDCorrectionY);
            X -= correctionX;
            Y -= correctionY;

            return new Vector3RD
            {
                x = (float)X,
                y = (float)Y,
                z = 0
            };
        }

        /// <summary>
        /// Converts WGS84-coordinate to UnityCoordinate
        /// </summary>
        /// <param name="coordinate">Vector3RD WGS-coordinate</param>
        /// <returns>Vector Unity-Coordinate</returns>
        public static Vector3 ToUnity(Vector3WGS coordinate)
        {
            Vector3 output = ToUnity(coordinate.lon, coordinate.lat);
            double heightCorrection = EPSG7415.RDCorrection(coordinate.lon, coordinate.lat, "Z", EPSG7415.RDCorrectionZ);
            output.y = (float)( coordinate.h - heightCorrection);

            return output;
        }

        /// <summary>
        /// Converts WGS84-coordinate to UnityCoordinate
        /// </summary>
        /// <param name="coordinate">Vector3 WGS-coordinate</param>
        /// <returns>Vector3 Unity-Coordinate</returns>
        public static Vector3 ToUnity(Vector3 coordinate)
        {
            Vector3 output = ToUnity(coordinate.x, coordinate.y);
            double heightCorrection = EPSG7415.RDCorrection(coordinate.x, coordinate.y, "Z", EPSG7415.RDCorrectionZ);
            output.y = (float)(coordinate.z - heightCorrection);

            return output;
        }

        /// <summary>
        /// Converts WGS84-coordinate to UnityCoordinate
        /// </summary>
        /// <param name="lon">double lon (east-west)</param>
        /// <param name="lat">double lat (south-north)</param>
        /// <returns>Vector3 Unity-Coordinate at 0-NAP</returns>
        public static Vector3 ToUnity(double lon, double lat)
        {
            Vector3 output = new Vector3();
            if (IsValid(new Vector3WGS(lon,lat,0)) == false)
            {
                Debug.Log("<color=red>coordinate " + lon + "," + lat + " is not a valid WGS84-coordinate!</color>");
                return output;
            }

            Vector3RD vectorRD = new Vector3RD();
            vectorRD = ToEPSG7415(lon, lat);
            vectorRD.z = EPSG7415.zeroGroundLevelY;

            return EPSG7415.ToUnity(vectorRD);
        }

        /// <summary>
        /// checks if WGS-coordinate is valid
        /// </summary>
        /// <param name="coordinate">Vector3 WGS84-coordinate</param>
        /// <returns>True if coordinate is valid</returns>
        public static bool IsValid(Vector3WGS coordinate)
        {
            if (coordinate.lon < -180) return false;
            if (coordinate.lon > 180) return false;
            if (coordinate.lat < -90) return false;
            if (coordinate.lat > 90) return false;

            return true;
        }

        public static Vector3 RotationToUp(Vector3WGS position)
        {
            Vector3 rotation = new Vector3((float)position.lon,-90,(float)-(90-position.lat));
            Vector3ECEF positionECEF = ToECEF(position);
            Vector3 direction = new Vector3();
            direction.x = (float)-positionECEF.X;
            direction.y = (float)positionECEF.Z;
            direction.z = (float)positionECEF.Y;
            rotation = Quaternion.FromToRotation(direction, Vector3.up).eulerAngles;
            rotation.y -= 90;
            rotation.x *= -1;

            return rotation;
        }

        public static Coordinate ConvertTo(Coordinate coordinate, int targetCrs)
        {
            if (coordinate.CoordinateSystem != (int)CoordinateSystem.EPSG_3857)
            {
                throw new ArgumentOutOfRangeException(
                    $"Invalid coordinate received, this class cannot convert CRS ${coordinate.CoordinateSystem}"
                );
            }

            var vector3 = new Vector3WGS(coordinate.Points[0], coordinate.Points[1], coordinate.Points[2]);

            switch (targetCrs)
            {
                case (int)CoordinateSystem.Unity:
                {
                    var result = ToUnity(vector3);
                    return new Coordinate(targetCrs, result.x, result.y, result.z);
                }
                case (int)CoordinateSystem.EPSG_7415:
                {
                    var result = ToEPSG7415(vector3.lon, vector3.lat);
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

        public static Vector3WGS ToVector3WGS(this Coordinate coordinate)
        {
            return new Vector3WGS(
                coordinate.Points[0],
                coordinate.Points[1],
                coordinate.Points[2]
            );
        }
    }
}
