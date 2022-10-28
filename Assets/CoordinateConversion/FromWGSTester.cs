using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FromWGSTester : MonoBehaviour
{

    public Crs crs;
    public double Longitude;
    public double Latitude;
    public CrsSettings crsSettings;

    // Start is called before the first frame update
    void Start()
    {
        Crs crs = new Crs(crsSettings);

        Vector3Double xy = crs.conversion.FromWGS84(new Vector3LatLong(Latitude, Longitude));

        Debug.Log("X: " + xy.East);
        Debug.Log("Y: " + xy.North);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
