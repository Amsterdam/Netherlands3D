using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFSFeature
{
    public string FeatureName { get; private set; }
    public List<string> CRS = new();
    private List<WFSFeatureData> featureData = new();

    public WFSFeature(string name)
    {
        FeatureName = name;
    }

    public void AddNewFeatureData(WFSFeatureData newData)
    {
        if (featureData.Contains(newData))
        {
            Debug.LogWarning("Attempting to add data that's already in the list!");
            return;
        }
        featureData.Add(newData);
    }
    public List<WFSFeatureData> GetFeatureDataList => featureData;

}
