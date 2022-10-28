using UnityEngine;

[CreateAssetMenu(fileName = "ellipsoid", menuName = "ScriptableObjects/ellipsoid", order = 1)]
public class Ellipsoid: ScriptableObject
{
    public string name = "Bessel1841";
    public double semimajorAxis = 6377397.155;
    public double eccentricity = 0.08169683;
    public double inverseFlattening = 299.15281;
}


