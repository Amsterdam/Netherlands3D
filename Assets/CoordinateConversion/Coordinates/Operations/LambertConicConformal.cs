using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LambertConicConformal : OperationMethod
{
    public int EpsgCode = 9801;

    private double latitudeOfNaturalOriginRad;
    private double longitudeOfNaturalOriginRad;
    private double scaleFactorAtNaturalOrigin;
    private double falseEasting;
    private double falseNorthing;
    private double primeMeridian;

    private Ellipsoid ellipsoid = new Ellipsoid();

    private double n;
    private double a;
    private double e;
    private double ko;
    private double F;
    private double to;
    private double ro;

    private int iterations = 4;

    public LambertConicConformal(double lattitudeOfNaturalOrigin, double longitudeOfNaturalOrigin, 
        double scaleFactorAtNaturalOrigin, double falseEasting, double falseNorthing, double primeMeridian, Ellipsoid ellipsoid)
    {
        latitudeOfNaturalOriginRad = lattitudeOfNaturalOrigin * Math.PI / 180;
        longitudeOfNaturalOriginRad = longitudeOfNaturalOrigin * Math.PI / 180;

        this.primeMeridian = primeMeridian;
        this.scaleFactorAtNaturalOrigin = scaleFactorAtNaturalOrigin;
        this.falseEasting = falseEasting;
        this.falseNorthing = falseNorthing;
        this.ellipsoid = ellipsoid;
        SetSphereConstants();
    }

    public override Vector3Double FromWGS84(Vector3LatLong latlong)
    {
        double lattitude = latlong.lattitudeRad;
        double longitude = (latlong.longitude - primeMeridian) * Math.PI / 180;

        double t = Math.Tan(Math.PI / 4 - lattitude / 2) / (Math.Pow((1 - e * Math.Sin(lattitude)) / (1 + e * Math.Sin(lattitude)), e / 2));

        double r = a * F * Math.Pow(t, n) * ko;
        double O = n * (longitude - longitudeOfNaturalOriginRad);

        Vector3Double result = new();
        result.East = falseEasting + r * Math.Sin(O);
        result.North = falseNorthing + ro - r * Math.Cos(O);

        return result;
    }

    public override Vector3LatLong ToWGS84(Vector3Double xy)
    {
        double sign = n / Math.Abs(n);

        double ri = (Math.Sqrt(Math.Pow(xy.East - falseEasting, 2) + Math.Pow(ro - (xy.North - falseNorthing), 2))) * sign;
        double ti = Math.Pow(ri / (a * ko * F), 1 / n);

        double Oi;
        if(sign > 0)
        {
            Oi = Math.Atan2(xy.East - falseEasting, ro - (xy.North - falseNorthing));
        }
        else
        {
            Oi = Math.Atan2(-(xy.East - falseEasting), -ro - (xy.North - falseNorthing));
        }



        double longitude = Oi/n + longitudeOfNaturalOriginRad; 
        double latitude = Math.PI / 2 - 2 * Math.Atan(ti);

        for (int i = 0; i < iterations; i++)
        {
            latitude = Math.PI / 2 - 2 * Math.Atan(ti * Math.Pow(1 - e * Math.Sin(latitude), e / 2));
        }

        Vector3LatLong result = new();

        result.lattitude = latitude * 180 / Math.PI;
        result.longitude = (longitude * 180 / Math.PI) + primeMeridian;

        return result;
    }

    private void SetSphereConstants()
    {
        ellipsoid.eccentricity = Math.Sqrt(2 * (1 / ellipsoid.inverseFlattening) - Math.Pow(1 / ellipsoid.inverseFlattening, 2));

        n = Math.Sin(latitudeOfNaturalOriginRad);
        a = ellipsoid.semimajorAxis;
        e = ellipsoid.eccentricity;
        ko = scaleFactorAtNaturalOrigin;

        double mo = Math.Cos(latitudeOfNaturalOriginRad) / Math.Sqrt(1 - Math.Pow(e, 2) * Math.Pow(n, 2));
        to = Math.Tan(Math.PI / 4 - latitudeOfNaturalOriginRad / 2) / (Math.Pow((1 - e * Math.Sin(latitudeOfNaturalOriginRad)) / (1 + e * Math.Sin(latitudeOfNaturalOriginRad)), e / 2));
        F = mo / (n * Math.Pow(to, n));
        ro = a * F * Math.Pow(to, n) * ko;


    }

}
