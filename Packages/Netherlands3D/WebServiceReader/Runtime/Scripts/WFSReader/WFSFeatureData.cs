using System.Collections.Generic;
using Netherlands3D.GeoJSON;

public class WFSFeatureData
{
    public GeoJSONStreamReader.GeoJSONGeometryType GeometryType { get; set; }
    private Dictionary<string, object> featureProperties = new();

    public bool AddFeatureProperty(string propertyName, object propertyValue)
    {
        if (!featureProperties.ContainsKey(propertyName))
        {
            featureProperties.Add(propertyName, propertyValue);
            return true;
        }
        return false;
    }

    public bool RemoveFeatureProperty(string propertyName)
    {
        if (featureProperties.ContainsKey(propertyName))
        {
            featureProperties.Remove(propertyName);
            return true;
        }
        return false;
    }

    public bool OverwriteFeatureProperty(string propertyName, object propertyValue)
    {
        if (featureProperties.ContainsKey(propertyName))
        {
            featureProperties[propertyName] = propertyValue;
            return true;
        }
        return false;
    }

    public object GetFeatureProperty(string propertyName)
    {
        if (featureProperties.ContainsKey(propertyName))
        {
            return featureProperties[propertyName];
        }
        return null;
    }

    public bool ContainsValue(object valueToCheck)
    {
        return featureProperties.ContainsValue(valueToCheck);
    }
    public void TransferDictionary(Dictionary<string, object> propertiesDictionary)
    {
        featureProperties = propertiesDictionary;
    }

    public Dictionary<string, object> GetPropertyDictionary()
    {
        return featureProperties;
    }

}
