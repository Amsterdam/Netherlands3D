using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

namespace Netherlands3D.Tiles3D
{
    public static class ParseTileset
    {
        private static readonly Dictionary<string, BoundingVolumeType> boundingVolumeTypes = new()
        {
            { "region", BoundingVolumeType.Region },
            { "box", BoundingVolumeType.Box },
            { "sphere", BoundingVolumeType.Sphere }
        };

        public static Tile ReadTileset(JSONNode rootnode)
        {
            Tile root = new Tile();

            TilingMethod tilingMethod = TilingMethod.explicitTiling;
            double[] transformValues = new double[16] { 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0 };
            JSONNode transformNode = rootnode["transform"];
            if (transformNode != null)
            {
                for (int i = 0; i < 16; i++)
                {
                    transformValues[i] = transformNode[i].AsDouble;
                }
            }
            root.transform = transformValues;
            JSONNode implicitTilingNode = rootnode["implicitTiling"];
            if (implicitTilingNode != null)
            {
                tilingMethod = TilingMethod.implicitTiling;
            }

            //setup location and rotation
            switch (tilingMethod)
            {
                case TilingMethod.explicitTiling:
                    Debug.Log("Explicit tiling");
                    Tile rootTile = new Tile();
                    rootTile.transform = root.transform;
                    root = ReadExplicitNode(rootnode, rootTile);
                    root.screenSpaceError = float.MaxValue;
                    break;
                case TilingMethod.implicitTiling:
                    Debug.Log("Implicit tiling");
                    //ReadImplicitTiling(rootnode);
                    break;
                default:
                    break;
            }
            return root;
        }

        /// <summary>
        /// Recursive reading of tile nodes to build the tiles tree
        /// </summary>
        public static Tile ReadExplicitNode(JSONNode node, Tile tile)
        {

            tile.boundingVolume = new BoundingVolume();
            JSONNode boundingVolumeNode = node["boundingVolume"];
            ParseBoundingVolume(tile, boundingVolumeNode);

            tile.geometricError = double.Parse(node["geometricError"].Value);
            tile.refine = node["refine"].Value;
            JSONNode childrenNode = node["children"];

            tile.children = new List<Tile>();
            if (childrenNode != null)
            {
                for (int i = 0; i < childrenNode.Count; i++)
                {
                    var childTile = new Tile();
                    childTile.transform = tile.transform;
                    childTile.parent = tile;
                    tile.children.Add(ReadExplicitNode(childrenNode[i], childTile));
                }
            }
            JSONNode contentNode = node["content"];
            if (contentNode != null)
            {
                tile.hascontent = true;
                tile.contentUri = contentNode["uri"].Value;
            }

            return tile;
        }

        public static void ParseBoundingVolume(Tile tile, JSONNode boundingVolumeNode)
        {
            if (boundingVolumeNode != null)
            {
                foreach (KeyValuePair<string, BoundingVolumeType> kvp in boundingVolumeTypes)
                {
                    JSONNode volumeNode = boundingVolumeNode[kvp.Key];
                    if (volumeNode != null)
                    {
                        int length = GetBoundingVolumeLength(kvp.Value);
                        if (volumeNode.Count == length)
                        {
                            tile.boundingVolume.values = new double[length];
                            for (int i = 0; i < length; i++)
                            {
                                tile.boundingVolume.values[i] = volumeNode[i].AsDouble;
                            }
                            tile.boundingVolume.boundingVolumeType = kvp.Value;
                            break; // Exit the loop after finding the first valid bounding volume
                        }
                    }
                }
            }

            tile.CalculateBounds();
        }

        public static int GetBoundingVolumeLength(BoundingVolumeType type)
        {
            switch (type)
            {
                case BoundingVolumeType.Region:
                    return 6;
                case BoundingVolumeType.Box:
                    return 12;
                case BoundingVolumeType.Sphere:
                    return 4;
                default:
                    return 0;
            }
        }

        //private void ReadImplicitTiling(JSONNode rootnode)
        //{
        //    implicitTilingSettings = new ImplicitTilingSettings();
        //    string refine = rootnode["refine"].Value;
        //    switch (refine)
        //    {
        //        case "REPLACE":
        //            implicitTilingSettings.refinementType = RefinementType.Replace;
        //            break;
        //        case "ADD":
        //            implicitTilingSettings.refinementType = RefinementType.Add;
        //            break;
        //        default:
        //            break;
        //    }
        //    implicitTilingSettings.geometricError = rootnode["geometricError"].AsFloat;
        //    implicitTilingSettings.boundingRegion = new double[6];
        //    for (int i = 0; i < 6; i++)
        //    {
        //        implicitTilingSettings.boundingRegion[i] = rootnode["boundingVolume"]["region"][i].AsDouble;
        //    }
        //    implicitTilingSettings.contentUri = rootnode["content"]["uri"].Value;
        //    JSONNode implicitTilingNode = rootnode["implicitTiling"];
        //    string subdivisionScheme = implicitTilingNode["subsivisionScheme"].Value;
        //    switch (subdivisionScheme)
        //    {
        //        case "QUADTREE":
        //            implicitTilingSettings.subdivisionScheme = SubdivisionScheme.Quadtree;
        //            break;
        //        default:
        //            implicitTilingSettings.subdivisionScheme = SubdivisionScheme.Octree;
        //            break;
        //    }
        //    implicitTilingSettings.subtreeLevels = implicitTilingNode["subtreeLevels"];
        //    implicitTilingSettings.subtreeUri = implicitTilingNode["subtrees"]["uri"].Value;


        //    ReadSubtree subtreeReader = GetComponent<ReadSubtree>();
        //    string subtreeURL = tilesetUrl.Replace(tilesetFilename, implicitTilingSettings.subtreeUri)
        //                        .Replace("{level}", "0")
        //                        .Replace("{x}", "0")
        //                        .Replace("{y}", "0");

        //    Debug.Log("Load subtree: " + subtreeURL);
        //    subtreeReader.DownloadSubtree(subtreeURL, implicitTilingSettings, ReturnTiles);
        //}
    }
}
