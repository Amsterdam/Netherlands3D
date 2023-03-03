using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

public class WMSStyle
{
    // The style's name is the name optimized for the computer, but sometimes a bit less readable for people
    public string Name;
    // The style's title is a name formatted to create readability for people.
    public string Title;

    public string LegendURL;

    public override string ToString()
    {
        return $"WMS Style :{Name}, with title: {Title} and LegendURL: {LegendURL}";
    }
}
