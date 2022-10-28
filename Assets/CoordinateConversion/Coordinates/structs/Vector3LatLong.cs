using System;
public struct Vector3LatLong
{
    public double lattitude;
    public double lattitudeRad;
    public double longitude;
    public double longitudeRad;
    public double ellipsoidalHeight;
    public Vector3LatLong(double lattitude, double longitude, double ellipsoidalHeight=0)
    {
        this.lattitude = lattitude;
        lattitudeRad = lattitude * Math.PI / 180;
        this.longitude = longitude;
        longitudeRad = longitude * Math.PI / 180;
        this.ellipsoidalHeight = ellipsoidalHeight;
    }
}


