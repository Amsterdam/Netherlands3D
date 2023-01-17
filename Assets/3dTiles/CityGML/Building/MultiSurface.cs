using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class MultiSurface 
{   public string nodename;
    public string type;
    public List<SurfaceMember> SurfaceMembers = new List<SurfaceMember>();
public Dictionary<string, string> attributes = new Dictionary<string, string>();
    public MultiSurface(XmlNode node, string Typestring)
    {
                 
        foreach (XmlAttribute att in node.Attributes)
        {
            attributes.Add(att.LocalName, att.Value);
        }


        type = Typestring;
        nodename = node.Name;
        switch (node.LocalName)
        {
            case "lod1MultiSurface":
                SurfaceMembers.Add(new SurfaceMember(node,type));
                break;
            case "lod2MultiSurface":
                SurfaceMembers.Add(new SurfaceMember(node,type));
                break;
            case "lod3MultiSurface":
                SurfaceMembers.Add(new SurfaceMember(node,type));
                break;
            case "lod4MultiSurface":
                SurfaceMembers.Add(new SurfaceMember(node,type));
                break;


            default:
                break;
        }
        
    }

    public void CreateGameObjects(GameObject parent)
    {
        //GameObject go = new GameObject(nodename);
        //go.transform.parent = parent.transform;
        //if (attributes.ContainsKey("id"))
        //{
        //    ObjectProperties op = go.AddComponent<ObjectProperties>();
        //    op.gmlID = attributes["id"];
        //}
        foreach (SurfaceMember bs in SurfaceMembers)
        {
            bs.CreateGameObjects(parent);
        }
    }
}
