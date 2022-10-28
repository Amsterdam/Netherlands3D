using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tester : MonoBehaviour
{
    public Crs crs;
    public double X;
    public double Y;
    public CrsSettings crsSettings;

    // Start is called before the first frame update
    void Start()
    {
        Crs crs = new Crs(crsSettings);
        Vector3LatLong latlon = new Vector3LatLong(X,Y);
        Vector3Double coords = new Vector3Double(X,Y,0);
        


       

       // coords = crs.conversion.FromWGS84(latlon);

        latlon = crs.conversion.ToWGS84(coords);

        if (crsSettings.abridgedTransformation == operationMethod.CoordinateFrameRotation)
        {
            CoordinateFrameRotation cfr = new CoordinateFrameRotation(crsSettings.ellipsoid);
            latlon = cfr.ToWGS84(latlon);
        }

        Debug.Log("lat: " + latlon.lattitude);
        Debug.Log("lon: " + latlon.longitude);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
