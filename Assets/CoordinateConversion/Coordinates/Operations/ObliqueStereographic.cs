using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObliqueStereographic: OperationMethod

{
    public int epsgcode = 9809;
    double lattitudeOfNaturalOriginRad = 0.910296727;
    double longitudeOfNaturalOriginRad = 0.094032038;
    double scaleFactorAtNaturalOrigin = 0.9999079;
    double falseEasting = 155000;
    double falseNorthing = 463000;
    Ellipsoid ellipsoid = new Ellipsoid();

    // conformal sphere constants 
    

    double R;
    double n;
    double c;
    double conformalLattitudeOfOrigin;
    double delta;

    public ObliqueStereographic(double lattitudeOfNaturalOrigin, double longitudeOfNaturalOrigin,double scaleFactorAtNaturalOrigin, double falseEasting, double falseNorthing,Ellipsoid ellipsoid )
    {

        lattitudeOfNaturalOriginRad = lattitudeOfNaturalOrigin * Math.PI / 180;
        longitudeOfNaturalOriginRad = longitudeOfNaturalOrigin * Math.PI / 180;
        this.scaleFactorAtNaturalOrigin = scaleFactorAtNaturalOrigin;
        this.falseEasting = falseEasting;
        this.falseNorthing = falseNorthing;
        this.ellipsoid = ellipsoid;
        SetSphereConstants();
    }

   

    override public Vector3Double FromWGS84(Vector3LatLong latlong)
    {
        double lattitude = latlong.lattitudeRad;
        double longitude = latlong.longitudeRad;


        double Sa = (1+Math.Sin(lattitude)) / (1-Math.Sin(lattitude));
        double Sb = (1-(ellipsoid.eccentricity*Math.Sin(lattitude))) / (1 + (ellipsoid.eccentricity * Math.Sin(lattitude)));
        double w = c * Math.Pow(Sa*Math.Pow(Sb,ellipsoid.eccentricity), n);
        double conformalLattitude = Math.Asin((w - 1) / (w + 1));

        double deltaLabda = n * (longitude - longitudeOfNaturalOriginRad);
        double B = 1 + (Math.Sin(conformalLattitude) * Math.Sin(conformalLattitudeOfOrigin)) + (Math.Cos(conformalLattitude)*Math.Cos(conformalLattitudeOfOrigin)*Math.Cos(deltaLabda));

        Vector3Double result = new Vector3Double();
        result.East = falseEasting + (2 * R * scaleFactorAtNaturalOrigin * Math.Cos(conformalLattitude) * Math.Sin(deltaLabda) / B);
        result.North = falseNorthing + 2 * R * scaleFactorAtNaturalOrigin * (((Math.Sin(conformalLattitude)*Math.Cos(conformalLattitudeOfOrigin))-(Math.Cos(conformalLattitude)*Math.Sin(conformalLattitudeOfOrigin)*Math.Cos(deltaLabda)))/ B);

        return result;
    }

    override public Vector3LatLong ToWGS84(Vector3Double xy)
    {
        double g = 2 * R * scaleFactorAtNaturalOrigin * Math.Tan((Math.PI/4)-(conformalLattitudeOfOrigin/2));
        
        double h = 4 * R * scaleFactorAtNaturalOrigin * Math.Tan(conformalLattitudeOfOrigin) + g;
        double i = Math.Atan2((xy.East-falseEasting),(h+(xy.North-falseNorthing)));
        double j = Math.Atan2((xy.East - falseEasting), (g - (xy.North - falseNorthing)))-i;
       

        double lattitude = conformalLattitudeOfOrigin + (2*Math.Atan(((xy.North-falseNorthing)-(xy.East-falseEasting)*Math.Tan(j/2))/(2*R*scaleFactorAtNaturalOrigin)));

        
        double isometricLattitude = 0.5 * Math.Log((1+Math.Sin(lattitude))/(c*(1-Math.Sin(lattitude)))) / n;


        double phi1 = 2 * Math.Atan(Math.Pow(Math.E,isometricLattitude)) - (Math.PI / 2);
        //phi1 = calculatePhi(phi1, isometricLattitude, isometricLattitude);


        for (int q = 0; q < 4; q++)
        {
            
            double NewisometricLattitude = calculateIsometricLattitude(phi1);

            phi1 = calculateNectPhi(phi1, NewisometricLattitude, isometricLattitude);


        }
        Vector3LatLong result = new Vector3LatLong();
        result.lattitude = phi1 * 180/Math.PI;


        // now calculate the longitude
        double deltaLabda = j+(2*i);
        double lon = (deltaLabda / n + longitudeOfNaturalOriginRad) * 180 / Math.PI;
        result.longitude = lon;


        return result;

    }
    double calculateNectPhi(double phi, double NewIsometricLattitude, double IsometricLattitude)
    {
        return phi - (NewIsometricLattitude-IsometricLattitude)*Math.Cos(phi)*(1-Math.Pow(ellipsoid.eccentricity,2)*Math.Pow(Math.Sin(phi),2))/(1-Math.Pow(ellipsoid.eccentricity,2));
    }
    double calculateIsometricLattitude(double phi1)
    {
        return Math.Log(Math.Tan((phi1 / 2) + (Math.PI / 4)) * Math.Pow((1 - (ellipsoid.eccentricity * Math.Sin(phi1))) / (1 + (ellipsoid.eccentricity * Math.Sin(phi1))), (ellipsoid.eccentricity / 2)));
    }


    public void SetSphereConstants()
    {

        ellipsoid.eccentricity = Math.Sqrt(2 * (1 / ellipsoid.inverseFlattening) - Math.Pow(1 / ellipsoid.inverseFlattening, 2));

        double rho0 = ellipsoid.semimajorAxis * (1 - (Math.Pow(ellipsoid.eccentricity, 2))) / Math.Pow((1 - Math.Pow(ellipsoid.eccentricity, 2) * Math.Pow(Math.Sin(lattitudeOfNaturalOriginRad), 2)), 1.5);
        double nu0 = ellipsoid.semimajorAxis / Math.Sqrt((1 - Math.Pow(ellipsoid.eccentricity, 2) * Math.Pow(Math.Sin(lattitudeOfNaturalOriginRad), 2)));

        R = Math.Sqrt(rho0 * nu0);
        n = Math.Sqrt(1+((Math.Pow(ellipsoid.eccentricity,2)*Math.Pow(Math.Cos(lattitudeOfNaturalOriginRad),4))/(1-(Math.Pow(ellipsoid.eccentricity,2)))));

        double s1 = (1+Math.Sin(lattitudeOfNaturalOriginRad)) / (1-Math.Sin(lattitudeOfNaturalOriginRad));
        double s2 = (1-ellipsoid.eccentricity* Math.Sin(lattitudeOfNaturalOriginRad)) / (1 + ellipsoid.eccentricity * Math.Sin(lattitudeOfNaturalOriginRad));
        double w1 = Math.Pow((s1*Math.Pow(s2,ellipsoid.eccentricity)), n);
        double sinChi00 = (w1 - 1) / (w1 + 1);

        c = ((n + Math.Sin(lattitudeOfNaturalOriginRad)) * (1 - sinChi00)) / ((n- Math.Sin(lattitudeOfNaturalOriginRad))*(1+sinChi00));


        double w2 = c * w1;

        conformalLattitudeOfOrigin = Math.Asin((w2-1)/(w2+1));

       
    }


}


