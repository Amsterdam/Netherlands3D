using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class CityModel
{
    public enum locatie
    {
        geen,
        helsinki

    }

    public locatie loc = locatie.helsinki;
    public List<cityObjectMember> cityObjectMembers = new List<cityObjectMember>();


    public CityModel(string filepath)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(filepath);
        foreach (XmlNode xmlNode in doc.DocumentElement)
        {
            if (xmlNode.Name == "cityObjectMember")
            {
                cityObjectMember cityobjectmember = new cityObjectMember(xmlNode);
                cityObjectMembers.Add(cityobjectmember);
            }
        }
    }

    public void CreateGameObjects()
    {
        GameObject go = new GameObject("CityModel");
        foreach (cityObjectMember com in cityObjectMembers)
        {
            com.CreateGameObjects(go);
        }
    }
}
