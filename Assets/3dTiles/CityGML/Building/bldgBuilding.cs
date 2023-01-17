using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class bldgBuilding: AbstractBuilding
{
    public string GmlID;
    public Dictionary<string, string> attributes = new Dictionary<string, string>();
    public bldgBuilding(XmlNode xmlnode)
    {
  
        foreach (XmlAttribute att in xmlnode.Attributes)
        {
            attributes.Add(att.LocalName, att.Value);
        }
        readNodes(xmlnode);

    }
    //public void CreateGameObjects(GameObject parent)
    //{
    //    GameObject go = new GameObject(name);
    //    if (attributes.ContainsKey("id"))
    //    {
    //        ObjectProperties op = go.AddComponent<ObjectProperties>();
    //        op.gmlID = attributes["id"];
    //    }
    //    go.transform.parent = parent.transform;

    //    foreach (BoundedBy bby in boundedBy)
    //    {
    //        bby.CreateGameObjects(go);
    //    }
    //    if (outerBuildingInstallation is null)
    //        { }
    //        else
    //        {
    //        foreach (BuildingInstallation bi in outerBuildingInstallation)
    //        {
    //            bi.CreateGameObjects(go);
    //        }
    //    }

    //}
}

