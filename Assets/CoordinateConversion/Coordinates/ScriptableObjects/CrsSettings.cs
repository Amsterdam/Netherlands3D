
using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "crs", menuName = "ScriptableObjects/CRS", order = 1)]
public class CrsSettings : ScriptableObject
{
    public int epsgcode = 28992;
    public string name = "RD-New";
    public double PrimeMeridian = 0;
    public double lattitudeOfNaturalOrigin = 52.1561605555556;
    public double longitudeOfNaturalOrigin = 5.38763888888889;
    public double scaleFactorAtNaturalOrigin = 0.9999079;
    public double falseEasting = 155000;
    public double falseNorthing = 463000;
    public Ellipsoid ellipsoid;
    public operationMethod conversionMethod;
    public operationMethod abridgedTransformation;


}
