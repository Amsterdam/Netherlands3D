using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
public class opening
{
    public string name;
    public List<MultiSurface> lod3MultiSurface;
    public List<MultiSurface> lod4MultiSurface;

    public opening(XmlNode node)
    {
        foreach (XmlNode child in node.ChildNodes)
        {
            name = child.LocalName;

            foreach (XmlNode grandchild in child)
            {
                switch (grandchild.LocalName)
                {
                    case "lod3MultiSurface":
                        if (lod3MultiSurface == null)
                        {
                            lod3MultiSurface = new List<MultiSurface>();
                        }
                        lod3MultiSurface.Add(new MultiSurface(grandchild, name));
                        break;
                    case "lod4MultiSurface":
                        if (lod4MultiSurface == null)
                        {
                            lod4MultiSurface = new List<MultiSurface>();
                        }
                        lod4MultiSurface.Add(new MultiSurface(grandchild, name));
                        break;
                    default:
                        break;
                }
            }
        }
    }
        public void CreateGameObjects(GameObject parent)
        {
            if (lod3MultiSurface != null)
            {
                foreach (MultiSurface ms in lod3MultiSurface)
                {
                    ms.CreateGameObjects(parent);
                }
            }
        if (lod4MultiSurface != null)
        {
            foreach (MultiSurface ms in lod4MultiSurface)
            {
                ms.CreateGameObjects(parent);
            }
        }
    }
    }
