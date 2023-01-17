using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class BuildingInstallation 
{
    public string name;
    public string classcode;
    public string functioncode;
    public string usagecode;
    public List<SurfaceMember> lod2Geometry;
    public List<SurfaceMember> lod3Geometry;
    public List<SurfaceMember> lod4Geometry;
    public List<BoundedBy> boundedBy;

    public BuildingInstallation(XmlNode node)
    {
        if (node.HasChildNodes==false)
        {
            return;
        }
        XmlNode buildinginstallationNode = node["bldg:BuildingInstallation"];


        foreach (XmlNode child in buildinginstallationNode.ChildNodes)
        {
            switch (child.LocalName)
            {
                case "name":
                    name = child.InnerText;
                    break;
                case "class":
                    classcode = child.InnerText;
                    break;
                case "function":
                    functioncode = child.InnerText;
                    break;
                case "usage":
                    usagecode = child.InnerText;
                    break;
                case "lod2Geometry":
                    if (lod2Geometry==null)
                    {
                        lod2Geometry = new List<SurfaceMember>();
                    }
                    lod2Geometry.Add(new SurfaceMember(child,"BuildingInstallation"));
                    break;
                case "lod3Geometry":
                    if (lod3Geometry == null)
                    {
                        lod3Geometry = new List<SurfaceMember>();
                    }
                    lod3Geometry.Add(new SurfaceMember(child, "BuildingInstallation"));
                    break;
                case "lod4Geometry":
                    if (lod4Geometry == null)
                    {
                        lod4Geometry = new List<SurfaceMember>();
                    }
                    lod4Geometry.Add(new SurfaceMember(child, "BuildingInstallation"));
                    break;
                case "boundedBy":
                    if (boundedBy == null)
                    {
                        boundedBy = new List<BoundedBy>();
                    }
                    boundedBy.Add(new BoundedBy(child));
                    break;
                default:
                    break;
            }
        }
    }
    public void CreateGameObjects(GameObject parent)
    {
        GameObject go = new GameObject(name);
        go.transform.parent = parent.transform;
        if (lod2Geometry!=null)
        {
            foreach (SurfaceMember bs in lod2Geometry)
            {
                bs.CreateGameObjects(go);
            }
        }
        if (lod3Geometry!=null)
        {
            foreach (SurfaceMember bs in lod3Geometry)
            {
                bs.CreateGameObjects(go);
            }
        }
        if (lod4Geometry!=null)
        {
            foreach (SurfaceMember bs in lod4Geometry)
            {
                bs.CreateGameObjects(go);
            }
        }
        if (boundedBy!=null)
        {
            foreach (BoundedBy bs in boundedBy)
            {
                bs.CreateGameObjects(go);
            }
        }

    }
}
