using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class AbstractBuilding 
{
    public string name;
    public string classcode;
    public List<string> functioncode;
    public List<string> usagecode;
    public int yearOfConstruction;
    public int yearOfDemolition;
    public string rooftypecode;
    public float measuredHeight;
    public int StoreysAboveGround;
    public int StoreysBelowGround;
    public List<float> StoreyHeightsAboveGround;
    public List<float> StoreyHeightsBelowGround;
    public MultiSurface lod0FootPrint;
    public MultiSurface lod0RoofEdge;
    public Solid lod1Solid;
    public MultiSurface lod1MultiSurface;
    //public MultiCurve lod1TerrainIntersection;
    public Solid lod2Solid;
    public MultiSurface lod2MultiSurface;
    //public MultiCurve lod2MultiCurve;
    //public multiCurve lod2TerrainIntersection;
    public List<BuildingInstallation> outerBuildingInstallation;
    //public intBuildingInstallation interiorBuildingInstallation;
    public List<BoundedBy> boundedBy;
    public Solid lod3Solid;
    public MultiSurface lod3MultiSurface;
    //public MultiCurve lod3MultiCurve;
    //public multiCurve lod3TerrainIntersection;
    public Solid lod4Solid;
    public MultiSurface lod4MultiSurface;
    //public MultiCurve lod4MultiCurve;
    //public multiCurve lod4TerrainIntersection;
    //public InteriorRoom interiorRoom;
    //public List<BuildingParts> consistsOfBuildingPart;
    //public List<Address> adress;
    public XmlNode xmlnode;

    public void readNodes(XmlNode xmlNode)
    {
        foreach (XmlNode node in xmlNode.ChildNodes)
        {
            switch (node.LocalName)
            {
                case "name":
                    name = node.InnerText;
                    break;
                case "class":
                    classcode = node.InnerText;
                    break;
                case "function":
                    if (functioncode==null)
                    {
                        functioncode = new List<string>();
                    }
                    functioncode.Add(node.InnerText);
                    break;
                case "usage":
                    if (usagecode == null)
                    {
                        usagecode = new List<string>();
                    }
                    usagecode.Add(node.InnerText);
                    break;
                case "yearOfConstruction":
                    int.TryParse(node.InnerText, out yearOfConstruction);
                    break;
                case "yearOfDemolition":
                    int.TryParse(node.InnerText, out yearOfDemolition);
                    break;
                case "roofType":
                    rooftypecode = node.InnerText;
                    break;
                case "measuredHeight":
                    float.TryParse(node.InnerText, out measuredHeight);
                    break;
                case "storeysAboveGround":
                    int.TryParse(node.InnerText, out StoreysAboveGround);
                    break;
                case "storeysBelowGround":
                    int.TryParse(node.InnerText, out StoreysBelowGround);
                    break;

                case "storeyHeightsAboveGround":
                    if (StoreyHeightsAboveGround == null)  
                    {
                        StoreyHeightsAboveGround = new List<float>();
                    }
                    float hoogte;
                    float.TryParse(node.InnerText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out hoogte);
                    StoreyHeightsAboveGround.Add(hoogte);
                    break;
                case "storeyHeightsBelowGround":
                    if (StoreyHeightsBelowGround == null)
                    {
                        StoreyHeightsBelowGround = new List<float>();
                    }
                    float.TryParse(node.InnerText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out hoogte);
                    StoreyHeightsBelowGround.Add(hoogte);
                    break;
                case "lod0FootPrint":
                    lod0FootPrint = new MultiSurface(node,"floor");
                    break;
                case "lod0RoofEdge":
                    lod0RoofEdge = new MultiSurface(node,"roof");
                    break;
                case "lod1Solid":
                    lod1Solid = new Solid(node);
                    break;
                case "lod1MultiSurface":
                    lod1MultiSurface = new MultiSurface(node,"floor");
                    break;
                case "lod2Solid":
                    lod2Solid = new Solid(node);
                    break;
                case "lod2MultiSurface":
                    lod2MultiSurface = new MultiSurface(node,"floor");
                    break;
                case "boundedBy":
                    if (boundedBy == null)
                    {
                        boundedBy = new List<BoundedBy>();
                    }
                    boundedBy.Add(new BoundedBy(node));
                    break;
                case "outerBuildingInstallation":
                    if (outerBuildingInstallation == null)
                    {
                        outerBuildingInstallation = new List<BuildingInstallation>();
                    }
                    outerBuildingInstallation.Add(new BuildingInstallation(node));
                    break;


                case "lod3Solid":
                    lod3Solid = new Solid(node);
                    break;
                case "lod3MultiSurface":
                    lod3MultiSurface = new MultiSurface(node,"floor");
                    break;
                case "lod4Solid":
                    lod4Solid = new Solid(node);
                    break;
                case "lod4MultiSurface":
                    lod4MultiSurface = new MultiSurface(node,"floor");
                    break;

                default:
                    break;
            }
        }

    }

    
}
