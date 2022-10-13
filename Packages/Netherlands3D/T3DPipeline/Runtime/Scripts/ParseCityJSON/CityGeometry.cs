using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;

namespace Netherlands3D.T3DPipeline
{
    /// <summary>
    /// 
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

    public class CityGeometry
    {
        public static readonly Dictionary<GeometryType, int> GeometryDepth = new Dictionary<GeometryType, int>{
            {GeometryType.MultiPoint, 0 }, //A "MultiPoint" has an array with the indices of the vertices; this array can be empty.
            {GeometryType.MultiLineString, 1 }, //A "MultiLineString" has an array of arrays, each containing the indices of a LineString
            {GeometryType.MultiSurface, 2 }, //A "MultiSurface", or a "CompositeSurface", has an array containing surfaces, each surface is modelled by an array of array, the first array being the exterior boundary of the surface, and the others the interior boundaries.
            {GeometryType.CompositeSurface, 2 },
            {GeometryType.Solid, 3 }, //A "Solid" has an array of shells, the first array being the exterior shell of the solid, and the others the interior shells. Each shell has an array of surfaces, modelled in the exact same way as a MultiSurface/CompositeSurface.
            {GeometryType.MultiSolid, 4 }, //A "MultiSolid", or a "CompositeSolid", has an array containing solids, each solid is modelled as above.
            {GeometryType.CompositeSolid, 4 },
        };

        public GeometryType Type { get; private set; }
        public int Lod { get; private set; }
        private List<CityBoundary> boundaries; // we use this variable for the boundaries regardless of the geometry depth in this class. When requesting the array it is returned by GetBoundaries() in the correct depth
        public bool IncludeSemantics { get; set; }
        public bool IncludeMaterials { get; set; }
        public bool IncludeTextures { get; set; }
        //var Semantics;
        //var Materials;
        //var Textures;

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

        public CityGeometry(CityObjectType cityObjectType, GeometryType geometryType, int lod, bool includeSemantics, bool includeMaterials, bool includeTextures)
        {
            Assert.IsTrue(IsValidType(cityObjectType, geometryType));
            Type = geometryType;
            Lod = lod;
            boundaries = new List<CityBoundary>();
            boundaries.Add(new CityBoundary());
            IncludeSemantics = includeSemantics;
            IncludeMaterials = includeMaterials;
            IncludeTextures = includeTextures;
        }

        public virtual JSONObject GetGeometryNode()
        {
            var geometryNode = new JSONObject();
            geometryNode["type"] = Type.ToString();
            geometryNode["lod"] = Lod;
            geometryNode["boundaries"] = GetBoundariesNode();

            if (IncludeSemantics)
                geometryNode["semantics"] = GetSemantics();
            if(IncludeMaterials)
                geometryNode["material"] = GetMaterials();
            if(IncludeTextures)
                geometryNode["texture"] = GetTextures();

            return geometryNode;
        }

        private JSONArray GetBoundariesNode()
        {
            var boundariesNode = new JSONArray();
            var geometryDepth = GeometryDepth[Type];

            var activeBoundariesNode = boundariesNode;
            for (int i = 0; i < geometryDepth; i++)
            {
                var node = new JSONArray();
                activeBoundariesNode.Add(node);
                activeBoundariesNode = node;
            }

            activeBoundariesNode = boundariesNode;
            for (int solidIndex = 0; solidIndex < boundaries.Count; solidIndex++)
            {
                var solid = boundaries[solidIndex].Boundaries;
                activeBoundariesNode = activeBoundariesNode[solidIndex].AsArray;
                for (int shellIndex = 0; shellIndex < solid.Count; shellIndex++)
                {
                    var shells = solid[shellIndex].Boundaries;
                    activeBoundariesNode = activeBoundariesNode[shellIndex].AsArray;
                    for (int surfaceIndex = 0; surfaceIndex < shells.Count; surfaceIndex++)
                    {
                        var surfaces = shells[surfaceIndex].Boundaries;
                        activeBoundariesNode = activeBoundariesNode[surfaceIndex].AsArray;
                        for (int polygonIndex = 0; polygonIndex < surfaces.Count; polygonIndex++)
                        {
                            var polygons = surfaces[polygonIndex].Polygons;
                            activeBoundariesNode = activeBoundariesNode[polygonIndex].AsArray;
                            for (int surfaceBoundaryIndex = 0; surfaceBoundaryIndex < polygons.Count; surfaceBoundaryIndex++)
                            {
                                var surfaceBoundary = polygons[surfaceBoundaryIndex].LocalBoundaries;
                                activeBoundariesNode = activeBoundariesNode[surfaceBoundaryIndex].AsArray;
                                for (int pointIndex = 0; pointIndex < surfaceBoundary.Length; pointIndex++)
                                {
                                    activeBoundariesNode.Add(surfaceBoundary[pointIndex]);
                                }
                            }
                        }
                    }
                }
            }
            return boundariesNode;
        }

        protected virtual JSONNode GetSemantics()
        {
            throw new System.NotImplementedException();
            var node = new JSONObject();
            var surfaceSemantics = new JSONArray();
            var indices = new JSONArray();
            //todo: fix this
            //for (int i = 0; i < Surfaces[lod].Length; i++)
            //{
            //    surfaceSemantics.Add(Surfaces[lod][i].GetSemanticObject(Surfaces[lod]));
            //    indices.Add(i);
            //}

            node["surfaces"] = surfaceSemantics;
            node["values"] = indices;

            return node;
        }

        private JSONNode GetMaterials()
        {
            throw new System.NotImplementedException();
        }

        private JSONNode GetTextures()
        {
            throw new System.NotImplementedException();
        }
    }
}
