using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public struct GeoCentricCoordinates
{
    public double X;
    public double Y;
    public double Z;
    public GeoCentricCoordinates(double X, double Y, double Z)
    {
        this.X = X;
        this.Y = Y;
        this.Z = Z;
    }
}

public class CoordinateFrameRotation
{



    //Position Vector transformation (geog2D domain), EPSG method code 9606
    // Step #    Step Method Name                                   EPSG Method Code   GN7-2 section
    // 1            Geographic 2D to Geographic 3D                  9659                4.1.4
    // 2            Geographic 3D to Geocentric                     9602                4.1.1
    // 3            Coordinate Frame rotation(geocentric domain)    1033                4.2.3
    // 4            Geocentric to Geographic 3D                     9602                4.1.1
    // 5            Geographic 3D to Geographic 2D                  9659                4.1.4
    //

    public Ellipsoid ellipsoid;
    

    public CoordinateFrameRotation(Ellipsoid ellipsoid)
    {
        this.ellipsoid = ellipsoid;
    }

    void step1()
    {
        //skip elevation
    }

    Vector3Double step2(Vector3LatLong latlon)
    {
        double primeVerticalRadius = ellipsoid.semimajorAxis / Math.Sqrt((1 - Math.Pow(ellipsoid.eccentricity, 2) * Math.Pow(Math.Sin(latlon.lattitudeRad), 2)));
        double X = (primeVerticalRadius + latlon.ellipsoidalHeight) * Math.Cos(latlon.lattitudeRad) * Math.Cos(latlon.longitudeRad);
        double Y = (primeVerticalRadius + latlon.ellipsoidalHeight) * Math.Cos(latlon.lattitudeRad) * Math.Sin(latlon.longitudeRad);
        double Z = ((1 - Math.Pow(ellipsoid.eccentricity, 2)) * primeVerticalRadius + latlon.ellipsoidalHeight) * Math.Sin(latlon.lattitudeRad);
        return new Vector3Double(X, Y, Z);
    }
    Vector3LatLong step2Reverse(Vector3Double XYZ)
    {

        double eccentricity = Math.Sqrt(1 - (Math.Pow(6356752.30, 2) / Math.Pow(6378137, 2)));
        double inverseFlattening = 298.257223563;
        double semimajorAxis = 63781370;

        double eccentiricitySquared = Math.Pow(eccentricity, 2);
        double eta = eccentiricitySquared / (1-eccentiricitySquared);
        double b = semimajorAxis * (1 - (1 / inverseFlattening));
        double p = Math.Sqrt(Math.Pow(XYZ.East, 2) + Math.Pow(XYZ.North, 2));
        double q = Math.Atan2((XYZ.Height * semimajorAxis), (p * b));

        double lattitude = Math.Atan2(XYZ.Height + (eta * b * Math.Pow(Math.Sin(q), 3)), (p - Math.Pow(eccentricity, 2) * semimajorAxis * Math.Pow(Math.Cos(q), 3)));
        double primeVerticalRadius = semimajorAxis / Math.Sqrt((1 - Math.Pow(eccentricity, 2) * Math.Pow(Math.Sin(lattitude), 2)));
        double longitude = Math.Atan2(XYZ.North,XYZ.East);
        double height = (p / Math.Cos(lattitude)) - primeVerticalRadius;

        return new Vector3LatLong(180*lattitude/Math.PI, 180*longitude/Math.PI, height);
    }

    Vector3Double step3(Vector3Double XYZ)
    {
        double tX = 565.4171;
        double tY = 50.3319;
        double tZ = 465.5524;

        double rX = 1.9342 * 4.84813681109536 * Math.Pow(10, -6);
        double rY = -1.6677 * 4.84813681109536 * Math.Pow(10, -6);
        double rZ = 9.1019 * 4.84813681109536 * Math.Pow(10, -6);

        double dS = 4.0725 * Math.Pow(10, -6);

        double M = 1 + dS;

        double X = M*(XYZ.North - rZ * XYZ.East + rY * XYZ.Height) + tX;
        double Y = M*(rZ * XYZ.North + XYZ.East - rX * XYZ.Height) + tY;
        double Z = M*(-rY * XYZ.North + rX * XYZ.East + XYZ.Height)+tZ;

        return new Vector3Double(X, Y, Z);

    }

    //Vector3Double step3Reverse(Vector3Double XYZ)
    //{
    //    double tX = 565.4171;
    //    double tY = 50.3319;
    //    double tZ = 465.5524;

    //    double rX = -1.9342 * Math.PI / (180 * 3600);
    //    double rY = 1.6677 * Math.PI / (180 * 3600);
    //    double rZ = -9.1019 * Math.PI / (180 * 3600);

    //    double dS = 4.0725 * Math.Pow(10, -6);

    //    double M = 1 + dS;



    //}

    public  Vector3LatLong ToWGS84(Vector3LatLong latlong)
    {
        Vector3Double XYZ = step2(latlong);
        XYZ = step3(XYZ);
        Vector3LatLong result = step2Reverse(XYZ);
        return result;
    }

 

    
}
