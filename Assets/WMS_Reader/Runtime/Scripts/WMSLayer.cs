using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;


public class WMSLayer
{
    public Dictionary<string, WMSStyle> styles { get; private set; } = new();

    public WMSStyle RetrieveStyleFromDictionary(string styleName)
    {
        return styles.ContainsKey(styleName) ? styles[styleName] : null;
    }
}
