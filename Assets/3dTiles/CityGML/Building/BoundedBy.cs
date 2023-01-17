using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class BoundedBy
{
    public string name;
    public List<BoundarySurface> BoundarySurfaces = new List<BoundarySurface>();
    public List<BoundedBy> openingen = new List<BoundedBy>();
public Dictionary<string, string> attributes = new Dictionary<string, string>();

    public BoundedBy(XmlNode node)
    {
                 
        foreach (XmlAttribute att in node.Attributes)
        {
            attributes.Add(att.LocalName, att.Value);
        }


        foreach (XmlNode child in node.ChildNodes)
        {
            switch (child.LocalName)
            {
                case "RoofSurface":
                    BoundarySurfaces.Add(new BoundarySurface(child));
                    break;
                case "GroundSurface":
                    BoundarySurfaces.Add(new BoundarySurface(child));
                    break;
                case "WallSurface":
                    BoundarySurfaces.Add(new BoundarySurface(child));
                    break;
                case "OuterCeilingSurface":
                    BoundarySurfaces.Add(new BoundarySurface(child));
                    break;
                case "OuterFloorSurface":
                    BoundarySurfaces.Add(new BoundarySurface(child));
                    break;
                case "ClosureSurface":
                    BoundarySurfaces.Add(new BoundarySurface(child));
                    break;
                default:
                    break;
            }
        }
    }

    public void CreateGameObjects(GameObject parent)
    {
        foreach (BoundedBy item in openingen)
        {
            item.CreateGameObjects(parent);
        }
        foreach (BoundarySurface bs in BoundarySurfaces)
        {
            bs.CreateGameObjects(parent);
        }
    }
}
