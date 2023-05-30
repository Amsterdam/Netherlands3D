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
    public static class EPSG7415
    {
        //setup coefficients for lattitude-calculation
        private static double[] Kp = { 0, 2, 0, 2, 0, 2, 1, 4, 2, 4, 1 };
        private static double[] Kq = { 1, 0, 2, 1, 3, 2, 0, 0, 3, 1, 1 };
        private static double[] Kpq = { 3235.65389, -32.58297, -0.24750, -0.84978, -0.06550, -0.01709, -0.00738, 0.00530, -0.00039, 0.00033, -0.00012 };
        //setup coefficients for longitude-calculation
        private static double[] Lp = { 1, 1, 1, 3, 1, 3, 0, 3, 1, 0, 2, 5 };
        private static double[] Lq = { 0, 1, 2, 0, 3, 1, 1, 2, 4, 2, 0, 0 };
        private static double[] Lpq = { 5260.52916, 105.94684, 2.45656, -0.81885, 0.05594, -.05607, 0.01199, -0.00256, 0.00128, 0.00022, -0.00022, 0.00026 };

        public static byte[] RDCorrectionX = Resources.Load<TextAsset>("x2c").bytes;
        public static byte[] RDCorrectionY = Resources.Load<TextAsset>("y2c").bytes;
        public static byte[] RDCorrectionZ = Resources.Load<TextAsset>("nlgeo04").bytes;

        public static float zeroGroundLevelY = 0;

        private static Vector2RD relativeCenterCoordinate;
        public static Vector2RD relativeCenter {
            get => relativeCenterCoordinate;
            set
            {
                Vector2RD change = new Vector2RD(value.x - relativeCenterCoordinate.x, value.y - relativeCenterCoordinate.y);

                EPSG4936.relativeCenter = WGS84.ToECEF(ToWGS84(value.x, value.y));

                //TODO: rotation from earth centered earth fixed
                relativeCenterCoordinate = value;
            }
        }

        /// <summary>
        /// Converts Unity-Coordinate to RD-coordinate
        /// </summary>
        /// <param name="coordinates">Unity-Coordinate</param>
        /// <returns>RD-coordinate</returns>
        public static Vector3WGS ToWGS84(double x, double y, double nap = 0)
        {
            //coordinates of basepoint in RD
            double refRDX = 155000;
            double refRDY = 463000;

            //coordinates of basepoint in WGS84
            double refLon = 5.38720621;
            double refLat = 52.15517440;

            double correctionX = RDCorrection(x,y,"X",RDCorrectionX);
            double correctionY = RDCorrection(x, y, "Y", RDCorrectionY);

            double DeltaX = (x+correctionX - refRDX) * Math.Pow(10, -5);
            double DeltaY = (y+correctionY - refRDY) * Math.Pow(10, -5);

            //calculate latitude
            double Deltalat = 0;
            for (int i = 0; i < Kpq.Length; i++)
            {
                Deltalat += Kpq[i] * Math.Pow(DeltaX, Kp[i]) * Math.Pow(DeltaY, Kq[i]);
            }
            Deltalat = Deltalat / 3600;
            double lat = Deltalat + refLat;

            //calculate longitude
            double Deltalon = 0;
            for (int i = 0; i < Lpq.Length; i++)
            {
                Deltalon += Lpq[i] * Math.Pow(DeltaX, Lp[i]) * Math.Pow(DeltaY, Lq[i]);
            }
            Deltalon = Deltalon / 3600;
            double lon = Deltalon + refLon;

            //output result
            Vector3WGS output = new Vector3WGS();
            output.lon = lon;
            output.lat = lat;

            //output height missing
            return output;
        }

        /// <summary>
        /// correction for RD-coordinatesystem
        /// </summary>
        /// <param name="x">X-value of coordinate when richting is X or Y, else longitude</param>
        /// <param name="y">Y-value of coordinate when richting is X or Y, else lattitude</param>
        /// <param name="direction">X, Y, or Z</param>
        /// <returns>correction for RD X and Y or Elevationdifference between WGS84 and RD</returns>
        public static Double RDCorrection(double x, double y, string direction, byte[] bytes)
        {
            double value = 0;

            if (direction == "X")
            {
                value = -0.185;
            }
            else if (direction == "Y")
            {
                value = -0.232;
            }

            double Xmin;
            double Xmax;
            double Ymin;
            double Ymax;
            int sizeX;
            int sizeY;

            int dataNumber;
            sizeX = BitConverter.ToInt16(bytes, 4);
            sizeY = BitConverter.ToInt16(bytes, 6);
            Xmin = BitConverter.ToDouble(bytes, 8);
            Xmax = BitConverter.ToDouble(bytes, 16);
            Ymin = BitConverter.ToDouble(bytes, 24);
            Ymax = BitConverter.ToDouble(bytes, 32);

            double columnWidth = (Xmax - Xmin) / sizeX;
            double locationX = Math.Floor((x - Xmin) / columnWidth);
            double rowHeight = (Ymax - Ymin) / sizeY;
            double locationY = (long)Math.Floor((y - Ymin) / rowHeight);

            if (locationX < Xmin || locationX > Xmax)
            {
                return value;
            }
            if (locationY < Ymin || locationY > Ymax)
            {
                return value;
            }

            dataNumber = (int)(locationY * sizeX + locationX);

            // do linear interpolation on the grid
            if (locationX < sizeX && locationY < sizeY)
            {
                float bottomLeft = BitConverter.ToSingle(bytes, 56 + (dataNumber * 4));
                float bottomRight = BitConverter.ToSingle(bytes, 56 + ((dataNumber+1) * 4));
                float topLeft = BitConverter.ToSingle(bytes, 56 + ((dataNumber+ sizeX) * 4));
                float topRight = BitConverter.ToSingle(bytes, 56 + ((dataNumber + sizeX+1) * 4));

                double YDistance = ((y - Ymin) % rowHeight)/rowHeight;
                double YOrdinaryLeft = ((topLeft-bottomLeft)*YDistance)+bottomLeft;
                double YOrdinaryRigth = ((topRight - bottomRight) * YDistance)+bottomRight;

                double XDistance = ((x - Xmin) % columnWidth)/columnWidth;
                value += ((YOrdinaryRigth - YOrdinaryLeft) * XDistance) + YOrdinaryLeft;
            }
            else
            {
                float myFloat = BitConverter.ToSingle(bytes, 56 + (dataNumber * 4));
                value += myFloat;
            }

            return value;
        }

        /// <summary>
        /// Convert RD-coordinate to Unity-Coordinate
        /// </summary>
        /// <param name="coordinates">RD-coordinate</param>
        /// <returns>UnityCoordinate</returns>
        public static Vector3 ToUnity(Vector3 coordinates)
        {
            return ToUnity(coordinates.x, coordinates.y, coordinates.z);
        }

        /// <summary>
        /// Convert RD-coordinate to Unity-Coordinate
        /// </summary>
        /// <param name="coordinate">Vector3RD RD-Coordinate XYH</param>
        /// <returns>Vector3 Unity-Coordinate</returns>
        public static Vector3 ToUnity(Vector3RD coordinate)
        {
            return ToUnity(coordinate.x, coordinate.y, coordinate.z);
        }

        /// <summary>
        /// Convert RD-coordinate to Unity-Coordinate
        /// </summary>
        /// <param name="coordinate">Vector3RD RD-Coordinate XYH</param>
        /// <returns>Vector3 Unity-Coordinate</returns>
        public static Vector3 ToUnity(Vector2RD coordinate)
        {
            return ToUnity(coordinate.x, coordinate.y,0);
        }

        /// <summary>
        /// Convert RD-coordinate to Unity-coordinate
        /// </summary>
        /// <param name="coordinate">RD-coordinate XYH</param>
        /// <returns>Unity-Coordinate</returns>
        public static Vector3 ToUnity(Vector2 coordinate)
        {
            return ToUnity(coordinate.x, coordinate.y,0);
        }

        /// <summary>
        /// Convert RD-coordinate to Unity-coordinate
        /// </summary>
        /// <param name="x">RD X-coordinate</param>
        /// <param name="y">RD Y-coordinate</param>
        /// <param name="y">RD eleveation</param>
        /// <returns>Unity-Coordinate</returns>
        public static Vector3 ToUnity(double x, double y, double z)
        {
            return new Vector3
            {
                x = (float)(x - relativeCenter.x),
                y = (float)(z + zeroGroundLevelY),
                z = (float)(y - relativeCenter.y)
            };
        }

        public static Coordinate ConvertTo(Coordinate coordinate, int targetCrs)
        {
            if (coordinate.CoordinateSystem != (int)CoordinateSystem.EPSG_7415)
            {
                throw new ArgumentOutOfRangeException(
                    $"Invalid coordinate received, this class cannot convert CRS ${coordinate.CoordinateSystem}"
                );
            }

            var vector3 = new Vector3RD(coordinate.Points[0], coordinate.Points[1], coordinate.Points[2]);

            switch (targetCrs)
            {
                case (int)CoordinateSystem.Unity:
                {
                    var result = ToUnity(vector3);
                    return new Coordinate(targetCrs, result.x, result.y, result.z);
                }
                case (int)CoordinateSystem.WGS84:
                {
                    var result = ToWGS84(vector3.x, vector3.y, vector3.z);
                    return new Coordinate(targetCrs, result.lon, result.lat, result.h);
                }
            }

            throw new ArgumentOutOfRangeException(
                $"Conversion between CRS ${coordinate.CoordinateSystem} and ${targetCrs} is not yet supported"
            );
        }

        /// <summary>
        /// checks if RD-coordinate is within the defined valid region
        /// </summary>
        /// <param name="coordinate">RD-coordinate</param>
        /// <returns>true if coordinate is valid</returns>
        public static bool IsValid(Vector3RD coordinate)
        {
            if (coordinate.x > -7000) return false;
            if (coordinate.x < 300000) return false;
            if (coordinate.y > 289000) return false;
            if (coordinate.y < 629000) return false;

            return true;
        }

        public static Vector3RD ToVector3RD(this Coordinate coordinate)
        {
            return new Vector3RD(
                coordinate.Points[0],
                coordinate.Points[1],
                coordinate.Points[2]
            );
        }
    }
}
