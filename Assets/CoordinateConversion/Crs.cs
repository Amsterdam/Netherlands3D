
public class Crs
{
 
    public OperationMethod conversion;
    public CrsSettings settings;

    public Crs(CrsSettings crsSettings)
    {
        settings = crsSettings;
        switch (crsSettings.conversionMethod)
        {
            case operationMethod.None:
                break;
            case operationMethod.ObliqueStereographic:
                conversion = new ObliqueStereographic(crsSettings.lattitudeOfNaturalOrigin, crsSettings.longitudeOfNaturalOrigin, 
                    crsSettings.scaleFactorAtNaturalOrigin, crsSettings.falseEasting, crsSettings.falseNorthing, crsSettings.ellipsoid);
                break;
            case operationMethod.LambertConicConformal:
                conversion = new LambertConicConformal(crsSettings.lattitudeOfNaturalOrigin, crsSettings.longitudeOfNaturalOrigin, 
                    crsSettings.scaleFactorAtNaturalOrigin, crsSettings.falseEasting, crsSettings.falseNorthing,crsSettings.PrimeMeridian, crsSettings.ellipsoid);
                break;
            default:
                break;
        }
        
    }

}
public enum operationMethod
{
    None,
    ObliqueStereographic,
    LambertConicConformal,
    CoordinateFrameRotation
}


