using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Xml;

public class WMSLayer
{
    public string Name;
    public string Title;
    public string Abstract;

    public bool CoordinatesAreSRS = false;
    public XmlNodeList KeywordList;
    public List<string> CRS = new();

    public Dictionary<string, WMSStyle> styles { get; private set; } = new();

    public WMSStyle activeStyle { get; private set; }

    public WMSStyle RetrieveStyleFromDictionary(string styleName)
    {
        return styles.ContainsKey(styleName) ? styles[styleName] : null;
    }

    public void AddStyleToDictionary(string styleName, WMSStyle styleToAdd)
    {
        if (styles.ContainsKey(styleName))
        {
            Debug.LogError("Attempting to add a style by name that's already added!");
            return;
        }
        styles.Add(styleName, styleToAdd);
    }

    public void SelectStyle(WMSStyle styleToSelect)
    {
        if(styleToSelect == null)
        {
            return;
        }
        if (styles.ContainsKey(styleToSelect.Name))
        {
            activeStyle = styleToSelect;
            return;
        }
        Debug.LogError("Selected style is not available in this layer!");
    }

    public override string ToString()
    {
        return $"WMS Layer: {Name}, with title: {Title}\n{CRS.Count} CRS elements and Abstract: {Abstract}.";
    }

}
