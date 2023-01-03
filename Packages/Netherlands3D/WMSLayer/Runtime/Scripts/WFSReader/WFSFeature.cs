using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFSFeature
{
    public string FeatureName { get; private set; }
    public List<string> CRS = new();
    public WFSFeature(string name)
    {
        FeatureName = name;
    }

}
