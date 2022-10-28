public abstract class OperationMethod
{
    public abstract Vector3Double FromWGS84(Vector3LatLong latlong);
    public abstract Vector3LatLong ToWGS84(Vector3Double xy);
}



