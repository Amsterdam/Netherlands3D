using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;

namespace Netherlands3D.T3DPipeline
{
    /// <summary>
    /// 3. A Geometry object is a JSON object for which the type member’s value is one of the following
    /// </summary>
    public enum GeometryType
    {
        MultiPoint = 00,
        MultiLineString = 10,
        MultiSurface = 20,
        CompositeSurface = 21,
        Solid = 30,
        MultiSolid = 40,
        CompositeSolid = 41,
    }

    /// <summary>
    /// A CityGeometry Object has the same structure as described in the specs: https://www.cityjson.org/specs/1.0.3/#geometry-objects
    /// </summary>
    public class CityGeometry
    {
        public GeometryType Type { get; private set; }
        public int Lod { get; private set; }
        public CityBoundary BoundaryObject { get; private set; }
        public bool IncludeSemantics { get; set; }
        public bool IncludeMaterials { get; set; } //todo: Materials currently not implemented yet
        public bool IncludeTextures { get; set; } //todo: Textures currently not implemented yet

        //Certain CityObjectTypes can only have certain types of geometry. This is described in the specs
        public static bool IsValidType(CityObjectType cityObjectType, GeometryType geometryType)
        {
            switch (cityObjectType)
            {
                case CityObjectType.Building:
                    return geometryType == GeometryType.Solid || geometryType == GeometryType.MultiSolid || geometryType == GeometryType.MultiSurface;
                case CityObjectType.Bridge:
                    return geometryType == GeometryType.Solid || geometryType == GeometryType.CompositeSolid || geometryType == GeometryType.MultiSurface;
                case CityObjectType.CityObjectGroup:
                    return true;
                case CityObjectType.CityFurniture:
                    return geometryType == GeometryType.MultiPoint || geometryType == GeometryType.MultiLineString || geometryType == GeometryType.MultiSurface || geometryType == GeometryType.CompositeSurface || geometryType == GeometryType.Solid || geometryType == GeometryType.CompositeSolid;
                case CityObjectType.GenericCityObject:
                    return geometryType == GeometryType.MultiPoint || geometryType == GeometryType.MultiLineString || geometryType == GeometryType.MultiSurface || geometryType == GeometryType.CompositeSurface || geometryType == GeometryType.Solid || geometryType == GeometryType.CompositeSolid;
                case CityObjectType.LandUse:
                    return geometryType == GeometryType.MultiSurface || geometryType == GeometryType.CompositeSurface;
                case CityObjectType.PlantCover:
                    return geometryType == GeometryType.MultiSurface || geometryType == GeometryType.MultiSolid;
                case CityObjectType.Railway:
                    return geometryType == GeometryType.MultiSurface || geometryType == GeometryType.CompositeSurface || geometryType == GeometryType.MultiLineString;
                case CityObjectType.Road:
                    return geometryType == GeometryType.MultiSurface || geometryType == GeometryType.CompositeSurface || geometryType == GeometryType.MultiLineString;
                case CityObjectType.SolitaryVegetationObject:
                    return geometryType == GeometryType.MultiPoint || geometryType == GeometryType.MultiLineString || geometryType == GeometryType.MultiSurface || geometryType == GeometryType.CompositeSurface || geometryType == GeometryType.Solid || geometryType == GeometryType.CompositeSolid;
                case CityObjectType.TINRelief:
                    return geometryType == GeometryType.CompositeSurface;
                case CityObjectType.TransportSquare:
                    return geometryType == GeometryType.MultiSurface || geometryType == GeometryType.CompositeSurface || geometryType == GeometryType.MultiLineString;
                case CityObjectType.Tunnel:
                    return geometryType == GeometryType.Solid || geometryType == GeometryType.CompositeSolid || geometryType == GeometryType.MultiSurface;
                case CityObjectType.WaterBody:
                    return geometryType == GeometryType.MultiLineString || geometryType == GeometryType.MultiSurface || geometryType == GeometryType.CompositeSurface || geometryType == GeometryType.CompositeSolid;
                case CityObjectType.BuildingPart:
                    return geometryType == GeometryType.Solid || geometryType == GeometryType.MultiSolid || geometryType == GeometryType.MultiSurface;
                case CityObjectType.BuildingInstallation:
                    return true;
                case CityObjectType.BridgePart:
                    return geometryType == GeometryType.Solid || geometryType == GeometryType.CompositeSolid || geometryType == GeometryType.MultiSurface;
                case CityObjectType.BridgeInstallation:
                    return true;
                case CityObjectType.BridgeConstructionElement:
                    return true;
                case CityObjectType.TunnelPart:
                    return geometryType == GeometryType.Solid || geometryType == GeometryType.CompositeSolid || geometryType == GeometryType.MultiSurface;
                case CityObjectType.TunnelInstallation:
                    return true;
                default:
                    return false;
            }
        }

        public CityGeometry(GeometryType geometryType, int lod, bool includeSemantics, bool includeMaterials, bool includeTextures)
        {
            //mandatory
            Type = geometryType;
            Lod = lod;
            BoundaryObject = CreateBoundaryObject(geometryType);

            //optional
            IncludeSemantics = includeSemantics;
            IncludeMaterials = includeMaterials;
            IncludeTextures = includeTextures;
        }

        //create a Boundary object based on the geometry type.
        private CityBoundary CreateBoundaryObject(GeometryType geometryType)
        {
            switch (geometryType)
            {
                case GeometryType.MultiPoint:
                    return new CityMultiPoint();
                case GeometryType.MultiLineString:
                    return new CityMultiLineString();
                case GeometryType.MultiSurface:
                    return new CityMultiOrCompositeSurface();
                case GeometryType.CompositeSurface:
                    return new CityMultiOrCompositeSurface();
                case GeometryType.Solid:
                    return new CitySolid();
                case GeometryType.MultiSolid:
                    return new CityMultiOrCompositeSolid();
                case GeometryType.CompositeSolid:
                    return new CityMultiOrCompositeSolid();
                default:
                    return null;
            }
        }

        //Exports the Geometry as a JSONObject, and renumbers the boundaries to fit the the vertices provided in currentCityJSONVertices. Missing vertices will be added to currentCityJSONVertices
        public virtual JSONObject GetGeometryNodeAndAddVertices(Dictionary<Vector3Double, int> currentCityJSONVertices)
        {
            var geometryNode = new JSONObject();
            geometryNode["type"] = Type.ToString();
            geometryNode["lod"] = Lod;
            geometryNode["boundaries"] = BoundaryObject.GetBoundariesAndAddNewVertices(currentCityJSONVertices);

            if (IncludeSemantics)
                geometryNode["semantics"] = CityGeometrySemantics.GetSemanticObject(BoundaryObject);
            if (IncludeMaterials)
                geometryNode["material"] = GetMaterials();
            if (IncludeTextures)
                geometryNode["texture"] = GetTextures();

            return geometryNode;
        }

        public List<Vector3Double> GetUncombinedVertices()
        {
            return BoundaryObject.GetUncombinedVertices();
        }

        //parse a Geometry JSON into a CityGeometry object
        public static CityGeometry FromJSONNode(JSONNode geometryNode, List<Vector3Double> combinedVertices)
        {
            var type = (GeometryType)Enum.Parse(typeof(GeometryType), geometryNode["type"]);
            var lod = geometryNode["lod"].AsInt;

            //private CityBoundary boundaryObject;
            var semanticsNode = geometryNode["semantics"];
            var materialsNode = geometryNode["materials"];
            var texturesNode = geometryNode["texture"];

            var includeSemantics = semanticsNode.Count > 0;
            var includeMaterials = materialsNode.Count > 0;
            var includeTextures = texturesNode.Count > 0;

            var geometry = new CityGeometry(type, lod, includeSemantics, includeMaterials, includeTextures);
            geometry.BoundaryObject.FromJSONNode(geometryNode["boundaries"].AsArray, combinedVertices);
            if (includeSemantics)
                CityGeometrySemantics.FromJSONNode(semanticsNode, geometry.BoundaryObject);

            return geometry;
        }

        private JSONNode GetMaterials()
        {
            throw new System.NotImplementedException();
        }

        private JSONNode GetTextures()
        {
            throw new System.NotImplementedException();
        }

        public void CreateBoundaryObjectFromMesh(Mesh mesh)
        {
            var multiSurface = new CityMultiOrCompositeSurface();
            multiSurface.FromMesh(mesh);
            BoundaryObject = multiSurface;
        }
    }
}
