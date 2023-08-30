/* Copyright(C)  X Gemeente
                 X Amsterdam
                 X Economic Services Departments
Licensed under the EUPL, Version 1.2 or later (the "License");
You may not use this work except in compliance with the License. You may obtain a copy of the License at:
https://joinup.ec.europa.eu/software/page/eupl
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" basis, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System;
using UnityEngine;
using UnityEngine.Events;
using Netherlands3D.Events;

/// <summary>
/// Convert coordinates between Unity, WGS84 and RD(EPSG7415)
/// <!-- accuracy: WGS84 to RD  X <0.01m, Y <0.02m H <0.03m, tested in Amsterdam with PCNapTrans-->
/// <!-- accuracy: RD to WGS84  X <0.01m, Y <0.02m H <0.03m, tested in Amsterdam with PCNapTrans-->
/// </summary>
namespace Netherlands3D.Core
{
    /// <summary>
    /// Supported coordinate systems
    /// </summary>
    public enum CoordinateSystem
    {
        Unity,
        WGS84,
        RD
    }
    
    /// <summary>
    /// Vector2 width Double values to represent RD-coordinates (X,Y)
    /// </summary>
    [System.Serializable]
    public struct Vector2RD
    {
        public double x;
        public double y;
        
        public Vector2RD(double X, double Y)
        {
            x = X;
            y = Y;
        }

        public bool IsInThousands 
        {
            get 
            {
                Debug.Log($"x:{x} y:{y}");
                return x % 1000 == 0 && y % 1000 == 0;
            }
        }   
    }

    /// <summary>
    /// Vector3 width Double values to represent RD-coordinates (X,Y,H)
    /// </summary>
    public struct Vector3RD
    {
        public double x;
        public double y;
        public double z;
        public Vector3RD(double X, double Y, double Z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        public override string ToString()
        {
            return $"x:{x} y:{y} z:{z}";
        }
    }

    /// <summary>
    /// Vector3 width Double values to represent WGS84-coordinates (Lon,Lat,H)
    /// </summary>
    public struct Vector3WGS
    {
        public double lat;
        public double lon;
        public double h;
        public Vector3WGS(double Lon, double Lat, double H)
        {
            lat = Lat;
            lon = Lon;
            h = H;
        }
    }
    public struct Vector3ECEF
    {
        public double X;
        public double Y;
        public double Z;
        public Vector3ECEF(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public static class CoordConvert
    {
        private static byte[] RDCorrectionX = Resources.Load<TextAsset>("x2c").bytes;
        private static byte[] RDCorrectionY = Resources.Load<TextAsset>("y2c").bytes;
        private static byte[] RDCorrectionZ = Resources.Load<TextAsset>("nlgeo04").bytes;

        private static Vector3RD output = new Vector3RD();
        public static float zeroGroundLevelY = 0;

        private static Vector2RD relativeCenterRDCoordinate = new Vector2RD();

        static Quaternion ecefRotationToUp;
        public static Vector3ECEF relativeCenterECEF;
        public static Vector2RD relativeCenterRD { 
            get => relativeCenterRDCoordinate; 
            set
            {
                Vector2RD change = new Vector2RD(value.x - relativeCenterRDCoordinate.x, value.y - relativeCenterRDCoordinate.y);

                relativeCenterECEF = WGS84toECEF(RDtoWGS84(value.x, value.y));
                ecefRotationToUp = calculateRotationToUp();
                //TODO: rotation from earth centered earth fixed
                relativeCenterRDCoordinate = value;
            }
        }
        
        public static bool ecefIsSet;
        public static UnityEvent prepareForOriginShift = new UnityEvent();
        public class CenterChangedEvent : UnityEvent<Vector3> { }
        public static CenterChangedEvent relativeOriginChanged = new CenterChangedEvent();

        public static void MoveAndRotateWorld(Vector3 cameraPosition)
        {
            prepareForOriginShift.Invoke();

            var flatCameraPosition = new Vector3(cameraPosition.x, 0, cameraPosition.z);
            var newWGS84 = UnitytoWGS84(flatCameraPosition);
            var newRD = UnitytoRD(cameraPosition);
            relativeCenterRD = new Vector2RD(newRD.x,newRD.y);
           
            var offset = new Vector3(-cameraPosition.x, 0, -cameraPosition.z);
            
            relativeOriginChanged.Invoke(offset);
        }

        /// <summary>
        /// Converts WGS84-coordinate to UnityCoordinate
        /// </summary>
        /// <param name="coordinate">Vector2 WGS-coordinate</param>
        /// <returns>Vector3 Unity-Coordinate (y=0)</returns>
        public static Vector3 WGS84toUnity(Vector2 coordinate)
        {
            Vector3 output = new Vector3();
            output = WGS84toUnity(coordinate.x, coordinate.y);
           
            return output;
        }
        /// <summary>
        /// Converts WGS84-coordinate to UnityCoordinate
        /// </summary>
        /// <param name="coordinate">Vector3 WGS-coordinate</param>
        /// <returns>Vector3 Unity-Coordinate</returns>
        public static Vector3 WGS84toUnity(Vector3 coordinate)
        {
            Vector3 output = WGS84toUnity(coordinate.x, coordinate.y);
            double heightCorrection = RDCorrection(coordinate.x, coordinate.y, "Z", RDCorrectionZ);
            output.y = (float)(coordinate.z - heightCorrection);
            return output;
        }
        /// <summary>
        /// Converts WGS84-coordinate to UnityCoordinate
        /// </summary>
        /// <param name="coordinate">Vector3RD WGS-coordinate</param>
        /// <returns>Vector Unity-Coordinate</returns>
        public static Vector3 WGS84toUnity(Vector3WGS coordinate)
        {
            Vector3 output = WGS84toUnity(coordinate.lon, coordinate.lat);
            double heightCorrection = RDCorrection(coordinate.lon, coordinate.lat, "Z", RDCorrectionZ);
            output.y = (float)( coordinate.h - heightCorrection);
            return output;
        }
        /// <summary>
        /// Converts WGS84-coordinate to UnityCoordinate
        /// </summary>
        /// <param name="lon">double lon (east-west)</param>
        /// <param name="lat">double lat (south-north)</param>
        /// <returns>Vector3 Unity-Coordinate at 0-NAP</returns>
        public static Vector3 WGS84toUnity(double lon, double lat)
        {
            Vector3 output = new Vector3();
            if (WGS84IsValid(new Vector3WGS(lon,lat,0)) == false)
            {
                Debug.Log("<color=red>coordinate " + lon + "," + lat + " is not a valid WGS84-coordinate!</color>");
                return output;
            }
            Vector3RD vectorRD = new Vector3RD();
            vectorRD = WGS84toRD(lon, lat);
            vectorRD.z = zeroGroundLevelY;
            output = RDtoUnity(vectorRD);
            return output;
        }

        /// <summary>
        /// Convert RD-coordinate to Unity-Coordinate
        /// </summary>
        /// <param name="coordinaat">RD-coordinate</param>
        /// <returns>UnityCoordinate</returns>
        public static Vector3 RDtoUnity(Vector3 coordinaat)
        {
            return RDtoUnity(coordinaat.x, coordinaat.y, coordinaat.z);
        }
        /// <summary>
        /// Convert RD-coordinate to Unity-Coordinate
        /// </summary>
        /// <param name="coordinate">Vector3RD RD-Coordinate XYH</param>
        /// <returns>Vector3 Unity-Coordinate</returns>
        public static Vector3 RDtoUnity(Vector3RD coordinate)
        {
            return RDtoUnity(coordinate.x, coordinate.y, coordinate.z);
        }

        /// <summary>
        /// Convert RD-coordinate to Unity-Coordinate
        /// </summary>
        /// <param name="coordinate">Vector3RD RD-Coordinate XYH</param>
        /// <returns>Vector3 Unity-Coordinate</returns>
        public static Vector3 RDtoUnity(Vector2RD coordinate)
        {
            return RDtoUnity(coordinate.x, coordinate.y, 0);
        }

        /// <summary>
        /// Convert RD-coordinate to Unity-coordinate
        /// </summary>
        /// <param name="coordinate">RD-coordinate XYH</param>
        /// <returns>Unity-Coordinate</returns>
        public static Vector3 RDtoUnity(Vector2 coordinate)
        {
            Vector3 output = RDtoUnity(coordinate.x, coordinate.y,0);
            return output;
        }
        /// <summary>
        /// Convert RD-coordinate to Unity-coordinate
        /// </summary>
        /// <param name="x">RD X-coordinate</param>
        /// <param name="y">RD Y-coordinate</param>
        /// <param name="y">RD eleveation</param>
        /// <returns>Unity-Coordinate</returns>
        public static Vector3 RDtoUnity(double X, double Y, double Z)
        {
            Vector3 output = new Vector3()
            {
                x = (float)(X - relativeCenterRD.x),
                y = (float)(Z + zeroGroundLevelY),
                z = (float)(Y - relativeCenterRD.y)
            };   
            return output;
        }

        /// <summary>
        /// Converts Unity-Coordinate to WGS84-Coordinate 
        /// </summary>
        /// <param name="coordinaat">Unity-coordinate XHZ</param>
        /// <returns>WGS-coordinate</returns>
        public static Vector3WGS UnitytoWGS84(Vector3 coordinaat)
        {
            Vector3RD vectorRD = UnitytoRD(coordinaat);
            Vector3WGS output = RDtoWGS84(vectorRD.x,vectorRD.y);
            double hoogteCorrectie = RDCorrection(output.lon, output.lat, "Z", RDCorrectionZ);
            output.h = vectorRD.z + hoogteCorrectie;
            return output;
        }
        /// <summary>
        /// Converts Unity-Coordinate to RD-coordinate
        /// </summary>
        /// <param name="coordinaat">Unity-Coordinate</param>
        /// <returns>RD-coordinate</returns>
        public static Vector3RD UnitytoRD(Vector3 coordinaat)
        {
            //Vector3WGS wgs = UnitytoWGS84(coordinaat);
            Vector3RD RD = new Vector3RD();
            RD.x = coordinaat.x + relativeCenterRD.x;
            RD.y = coordinaat.z + relativeCenterRD.y;
            RD.z = coordinaat.y - zeroGroundLevelY;
            return RD;
        }


        /// <summary>
        /// Converts RD-coordinate to WGS84-cordinate using the "benaderingsformules" from http://home.solcon.nl/pvanmanen/Download/Transformatieformules.pdf
        /// and X, Y, and Z correctiongrids
        /// </summary>
        /// <param name="x">RD-coordinate X</param>
        /// <param name="y">RD-coordinate Y</param>
        /// <returns>WGS84-coordinate</returns>
        /// 
        //setup coefficients for lattitude-calculation
        private static double[] Kp = new double[] { 0, 2, 0, 2, 0, 2, 1, 4, 2, 4, 1 };
        private static double[] Kq = new double[] { 1, 0, 2, 1, 3, 2, 0, 0, 3, 1, 1 };
        private static double[] Kpq = new double[] { 3235.65389, -32.58297, -0.24750, -0.84978, -0.06550, -0.01709, -0.00738, 0.00530, -0.00039, 0.00033, -0.00012 };
        //setup coefficients for longitude-calculation
        private static double[] Lp = new double[] { 1, 1, 1, 3, 1, 3, 0, 3, 1, 0, 2, 5 };
        private static double[] Lq = new double[] { 0, 1, 2, 0, 3, 1, 1, 2, 4, 2, 0, 0 };
        private static double[] Lpq = new double[] { 5260.52916, 105.94684, 2.45656, -0.81885, 0.05594, -.05607, 0.01199, -0.00256, 0.00128, 0.00022, -0.00022, 0.00026 };

        public static Vector3WGS RDtoWGS84(double x, double y, double nap = 0)
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

            //calculate lattitude
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
        /// Converts WGS84-coordinate to RD-coordinate using the "benaderingsformules" from http://home.solcon.nl/pvanmanen/Download/Transformatieformules.pdf
        /// and X, Y, and Z correctiongrids
        /// </summary>
        /// <param name="lon">Longitude (East-West)</param>
        /// <param name="lat">Lattitude (South-North)</param>
        /// <returns>RD-coordinate xyH</returns>
        /// 
        //setup coefficients for X-calculation
        private static double[] Rp = new double[] { 0, 1, 2, 0, 1, 3, 1, 0, 2 };
        private static double[] Rq = new double[] { 1, 1, 1, 3, 0, 1, 3, 2, 3 };
        private static double[] Rpq = new double[] { 190094.945, -11832.228, -114.221, -32.391, -0.705, -2.340, -0.608, -0.008, 0.148 };
        //setup coefficients for Y-calculation
        private static double[] Sp = new double[] { 1, 0, 2, 1, 3, 0, 2, 1, 0, 1 };
        private static double[] Sq = new double[] { 0, 2, 0, 2, 0, 1, 2, 1, 4, 4 };
        private static double[] Spq = new double[] { 309056.544, 3638.893, 73.077, -157.984, 59.788, 0.433, -6.439, -0.032, 0.092, -0.054 };

        public static Vector3RD WGS84toRD(double lon, double lat)
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

            double correctionX = RDCorrection(X, Y, "X",RDCorrectionX);
            double correctionY = RDCorrection(X,Y, "Y", RDCorrectionY);
            X -= correctionX;
            Y -= correctionY;

            //output result
            output.x = (float)X;
            output.y = (float)Y;
            output.z = 0;
            return output;
        }

        //setup coefficients for ecef-calculation WGS84
        private static double semimajorAxis = 6378137;
        private static double flattening = 0.00335281066;
        private static double eccentricity = 0.08161284189827;

        //setup coefficients for ecef-calculation ETRD89
        /*private static double semimajorAxis = 6378137;
        private static double flattening = 0.003352810681183637418;
        private static double eccentricity = 0.0818191910428;*/

        private static Quaternion calculateRotationToUp()
        {
            Vector3WGS centerWGS = ECEFtoWGS84(relativeCenterECEF);
            Quaternion rotation = Quaternion.identity;
            rotation = Quaternion.AngleAxis(90f - (float)(centerWGS.lat), Vector3.back);
            rotation = Quaternion.AngleAxis((float)(centerWGS.lon) - 90, Vector3.up) * rotation;
            return rotation;


            Vector3 locationVector = new Vector3((float)-relativeCenterECEF.X, (float)relativeCenterECEF.Z, (float)-relativeCenterECEF.Y);

            //Compute the rotation to make the tiles point up on the world up
            Quaternion flatRotation = Quaternion.FromToRotation(locationVector, Vector3.up);

            //A long coordinate pointing east
            var wgs84East = UnitytoWGS84(Vector3.zero);
            wgs84East.lon += 0.0001;
            var ecefEast = WGS84toECEF(wgs84East); // Use a unit vector pointing in the north direction

            Vector3 northVector = new Vector3((float)-ecefEast.X, (float)ecefEast.Z, (float)-ecefEast.Y) - locationVector;
            northVector.z = 0;


            Quaternion northRotation = Quaternion.FromToRotation(northVector, Vector3.right);

            // Combine the two rotations into a single quaternion
            Quaternion result = northRotation * flatRotation;

            return result;
        }

        public static Quaternion ecefRotionToUp()
        {
            return ecefRotationToUp;


        }

        public static Vector3 ECEFToUnity(Vector3ECEF ecef)
        {
            Vector3 result = new Vector3();
            float deltaX = (float)(ecef.X - relativeCenterECEF.X);
            float deltaY = (float)(ecef.Y - relativeCenterECEF.Y);
            float deltaZ = (float)(ecef.Z - relativeCenterECEF.Z);

            result.x = -deltaX;
            result.y = deltaZ;
            result.z = -deltaY;

            //check Rotation
            result = ecefRotionToUp()*result;

            return result;
        }

        public static Vector3ECEF UnityToECEF(Vector3 point)
        {
            var temppoint = Quaternion.Inverse(ecefRotionToUp()) * point;
            Vector3ECEF ecef = new Vector3ECEF();
            ecef.X = -temppoint.x + relativeCenterECEF.X;
            ecef.Y = -temppoint.z + relativeCenterECEF.Y;
            ecef.Z = temppoint.y + relativeCenterECEF.Z;
            return ecef;
        }
        public static Vector3ECEF WGS84toECEF(Vector3WGS wgsCoordinate)
        {
            Vector3ECEF result = new Vector3ECEF();
            double lattitude = wgsCoordinate.lat * Math.PI / 180;
            double longitude = wgsCoordinate.lon * Math.PI / 180;

            //EPSG datset coordinate operation method code 9602)
            double primeVerticalRadius = semimajorAxis / (Math.Sqrt(1 - (Math.Pow(eccentricity, 2) * Math.Pow(Math.Sin(lattitude), 2))));
            result.X = (primeVerticalRadius + wgsCoordinate.h) * Math.Cos(lattitude) * Math.Cos(longitude);
            result.Y = (primeVerticalRadius + wgsCoordinate.h) * Math.Cos(lattitude) * Math.Sin(longitude);
            result.Z = ((1 - Math.Pow(eccentricity, 2)) * primeVerticalRadius + wgsCoordinate.h) * Math.Sin(lattitude);

            return result;
        }
        public static Vector3WGS ECEFtoWGS84(Vector3ECEF ecefCoordinate)
        {
            
            double eta = Math.Pow(eccentricity, 2) / (1 - Math.Pow(eccentricity, 2));
            double b = semimajorAxis * (1 - flattening);
            double p = Math.Sqrt(Math.Pow(ecefCoordinate.X, 2) + Math.Pow(ecefCoordinate.Y, 2));
            double q = Math.Atan2((ecefCoordinate.Z * semimajorAxis), p * b);

            double lattitude = Math.Atan2((ecefCoordinate.Z + eta * b * Math.Pow(Math.Sin(q), 3)), p - Math.Pow(eccentricity, 2) * semimajorAxis * Math.Pow(Math.Cos(q), 3));
            double longitude = Math.Atan2(ecefCoordinate.Y, ecefCoordinate.X);
            double primeVerticalRadius = semimajorAxis / (Math.Sqrt(1 - (Math.Pow(eccentricity, 2) * Math.Pow(Math.Sin(lattitude), 2))));
            double height = (p / Math.Cos(lattitude)) - primeVerticalRadius;
            Vector3WGS result = new Vector3WGS( longitude * 180 / Math.PI, lattitude * 180 / Math.PI, height);

            return result;
        }


        public static Vector3 RotationToUnityUP(Vector3WGS position)
        {
            Vector3 rotation = new Vector3((float)position.lon,-90,(float)-(90-position.lat));
            Vector3ECEF positionECEF = WGS84toECEF(position);
            Vector3 direction = new Vector3();
            direction.x = (float)-positionECEF.X;
            direction.y = (float)positionECEF.Z;
            direction.z = (float)positionECEF.Y;
            rotation = Quaternion.FromToRotation(direction, Vector3.up).eulerAngles;
            rotation.y -= 90;
            rotation.x *= -1;

            return rotation;
        }

        /// <summary>
        /// checks if RD-coordinate is within the defined valid region
        /// </summary>
        /// <param name="coordinaat">RD-coordinate</param>
        /// <returns>true if coordinate is valid</returns>
        public static bool RDIsValid(Vector3RD coordinaat)
        {
            if (coordinaat.x > -7000 && coordinaat.x < 300000)
            {
                if (coordinaat.y > 289000 && coordinaat.y < 629000)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// checks if WGS-coordinate is valid
        /// </summary>
        /// <param name="coordinaat">Vector3 WGS84-coordinate</param>
        /// <returns>True if coordinate is valid</returns>
        public static bool WGS84IsValid(Vector3WGS coordinaat)
        {
            bool isValid = true;
            if (coordinaat.lon < -180) { isValid = false; }
            if (coordinaat.lon > 180) { isValid = false; }
            if (coordinaat.lat < -90) { isValid = false; }
            if (coordinaat.lat > 90) { isValid = false; }
            return isValid;
        }

        /// <summary>
        /// correction for RD-coordinatesystem
        /// </summary>
        /// <param name="x">X-value of coordinate when richting is X or Y, else longitude</param>
        /// <param name="y">Y-value of coordinate when richting is X or Y, else lattitude</param>
        /// <param name="direction">X, Y, or Z</param>
        /// <returns>correction for RD X and Y or Elevationdifference between WGS84  and RD</returns>
        public static Double RDCorrection(double x, double y, string direction, byte[] bytes)
        {
            double value = 0;
            //TextAsset txt;

            if (direction == "X")
            {
                //txt = RDCorrectionX;
                value = -0.185;    
            }
            else if (direction == "Y")
            {
                //txt = RDCorrectionY;
                value = -0.232;
            }
            else
            {
                //DeltaH tussen wGS en NAP
                //txt = RDCorrectionZ;
            }

            
            //byte[] bytes = txt.bytes;

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
                
                float myFloat = System.BitConverter.ToSingle(bytes, 56 + (dataNumber * 4));
                value += myFloat;
            }
            //datanummer = 1500;

            return value;
        }

    }
}
