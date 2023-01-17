using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class cityObjectMember
{
    List<bldgBuilding> buildings = new List<bldgBuilding>();

    public cityObjectMember(XmlNode CityObjectMemberNode)
    {
        foreach (XmlNode node in CityObjectMemberNode.ChildNodes)
        {
            if (node.LocalName == "Building")
            {
                bldgBuilding building = new bldgBuilding(node);
                buildings.Add(building);
            }
        }
    }
    public void CreateGameObjects(GameObject parent)
    {
        //GameObject go = new GameObject("cityObject");
        //go.transform.parent = parent.transform;
        //foreach (bldgBuilding building in buildings)
        //{
        //    building.CreateGameObjects(parent);
        //}
    }
}
