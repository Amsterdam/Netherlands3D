using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class BoundarySurface
{
    public string name;
    public string nodename;

    public List<MultiSurface> Surfaces = new List<MultiSurface>();
    public List<opening> openingen = new List<opening>();
public Dictionary<string, string> attributes = new Dictionary<string, string>();
    public BoundarySurface(XmlNode Innode)
    {
                 
        foreach (XmlAttribute att in Innode.Attributes)
        {
            attributes.Add(att.LocalName, att.Value);
        }


nodename = Innode.LocalName;
        foreach (XmlNode node in Innode.ChildNodes)
        {


            switch (node.LocalName)
            {
                case "name":
                    name = node.InnerText;
                    break;
                case "lod3MultiSurface":
                    Surfaces.Add(new MultiSurface(node,name));
                    break;
                case "lod2MultiSurface":
                    Surfaces.Add(new MultiSurface(node, name));
                    break;
                case "opening":
                    openingen.Add(new opening(node));
                    break;
                default:
                    break;
            }
        }
    }

    public void CreateGameObjects(GameObject parent)
    {
        foreach (MultiSurface bs in Surfaces)
        {
            bs.CreateGameObjects(parent);
        }
        foreach (opening op in openingen)
        {
            op.CreateGameObjects(parent);
        }
    }
}

