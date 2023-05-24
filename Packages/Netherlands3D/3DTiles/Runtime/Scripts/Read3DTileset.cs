using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Networking;
using System;
using Netherlands3D.Core;
using System.IO;
using System.Linq;
using System.Collections.Specialized;
using System.Web;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Netherlands3D.Tiles3D
{
    [RequireComponent(typeof(ReadSubtree))]
    public class Read3DTileset : MonoBehaviour
    {
        public string tilesetUrl = "https://storage.googleapis.com/ahp-research/maquette/kadaster/3dbasisvoorziening/test/landuse_1_1/tileset.json";
        private string absolutePath = "";
        private string rootPath = "";
        private NameValueCollection queryParameters;

        public Tile root;
        public double[] transformValues;
        private Vector3ECEF positionECEF;

        TilingMethod tilingMethod = TilingMethod.explicitTiling;

        public ImplicitTilingSettings implicitTilingSettings;

        public int tileCount;
        public int nestingDepth;

        [Tooltip("Limits amount of detail higher resolution would cause to load.")]
        public int maxScreenHeightInPixels = 1080;
        public int maximumScreenSpaceError = 5;

        [SerializeField] private float sseComponent = -1;
        private List<Tile> visibleTiles = new List<Tile>();

        [SerializeField] private TilePrioritiser tilePrioritiser;
        private bool usingPrioritiser = false;

        private Camera currentCamera;
        private Vector3 lastCameraPosition;
        private Quaternion lastCameraRotation;
        private Vector3 currentCameraPosition;
        private Quaternion currentCameraRotation;
        private float lastCameraAngle = 60;

        private string tilesetFilename = "tileset.json";

        private bool nestedTreeLoaded = false;

        private static readonly Dictionary<string, BoundingVolumeType> boundingVolumeTypes = new()
        {
            { "region", BoundingVolumeType.Region },
            { "box", BoundingVolumeType.Box },
            { "sphere", BoundingVolumeType.Sphere }
        };

        private void OnEnable()
        {
            currentCamera = Camera.main;
        }

        void Start()
        {
            if (usingPrioritiser)
            {
                tilePrioritiser.SetCamera(currentCamera);
            }

            ExtractDatasetPaths();

            StartCoroutine(LoadTileset());

            CoordConvert.relativeOriginChanged.AddListener(RelativeCenterChanged);
        }

        private void ExtractDatasetPaths()
        {
            Uri uri = new(tilesetUrl);
            absolutePath = tilesetUrl.Substring(0,tilesetUrl.LastIndexOf("/")+1);

            rootPath = uri.GetLeftPart(UriPartial.Authority);

            queryParameters = HttpUtility.ParseQueryString(uri.Query);
            Debug.Log("Query params: " + queryParameters.ToString());

            foreach (string segment in uri.Segments)
            {
                if (segment.EndsWith(".json"))
                {
                    tilesetFilename = segment;
                    Debug.Log($"Dataset filename: {tilesetFilename}");
                    Debug.Log($"Absolute path: {absolutePath}");
                    Debug.Log($"Root path: {rootPath}");
                    break;
                }
            }
        }

        /// <summary>
        /// Change camera used by tileset 'in view' calculations
        /// </summary>
        /// <param name="camera">Target camera</param>
        public void SetCamera(Camera camera)
        {
            currentCamera = camera;
        }

        /// <summary>
        /// Initialize tileset with these settings.
        /// This allows you initialize this component via code directly.
        /// </summary>
        /// <param name="tilesetUrl">The url pointing to tileset; https://.../tileset.json</param>
        /// <param name="maximumScreenSpaceError">The maximum screen space error for this tileset (default=5)</param>
        /// <param name="tilePrioritiser">Optional tile prioritisation system</param>
        public void Initialize(string tilesetUrl, int maximumScreenSpaceError = 5, TilePrioritiser tilePrioritiser = null)
        {
            currentCamera = Camera.main;
            this.tilesetUrl = tilesetUrl;
            this.maximumScreenSpaceError = maximumScreenSpaceError;

            SetTilePrioritiser(tilePrioritiser);
        }

        /// <summary>
        /// Optional injection of tile prioritiser system
        /// </summary>
        /// <param name="tilePrioritiser">Prioritising system with TilePrioritiser base class. Set to null to disable.</param>
        public void SetTilePrioritiser(TilePrioritiser tilePrioritiser)
        {
            this.tilePrioritiser = tilePrioritiser;
            usingPrioritiser = (tilePrioritiser);
        }

        private void RelativeCenterChanged(Vector3 cameraOffset)
        {
            if (root == null) return;

            ApplyRootOrientation();

            //Flag all calculated bounds to be recalculated when tile bounds is requested
            RecalculateAllTileBounds(root);
        }

        /// <summary>
        /// Move the center ( 0,0,0 ) of the tileset to the proper unity position
        /// and make sure the up is set correctly
        /// </summary>
        /// <param name="root">The root tile</param>
        private void ApplyRootOrientation()
        {
            var tilePositionOrigin = new Vector3ECEF(root.transform[12], root.transform[13], root.transform[14]);
            this.transform.SetPositionAndRotation(
                CoordConvert.ECEFToUnity(tilePositionOrigin),
                CoordConvert.ecefRotionToUp()
            );
        }

        /// <summary>
        /// Recursive recalculation of tile bounds
        /// </summary>
        /// <param name="tile">Starting tile</param>
        private void RecalculateAllTileBounds(Tile tile)
        {
            if (tile == null) return;

            tile.CalculateBounds();

            foreach (var child in tile.children)
            {
                RecalculateAllTileBounds(child);
            }
        }

        /// <summary>
        /// IEnumerator to load tileset.json from url
        /// </summary>
        IEnumerator LoadTileset()
        {
            UnityWebRequest www = UnityWebRequest.Get(tilesetUrl);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                string jsonstring = www.downloadHandler.text;

                JSONNode rootnode = JSON.Parse(jsonstring)["root"];
                ReadTileset(rootnode);
            }
        }

        private void RequestContentUpdate(Tile tile)
        {
            if (!tile.content)
            {
                var newContentGameObject = new GameObject($"{tile.X},{tile.Y},{tile.Z} content");
                newContentGameObject.transform.SetParent(transform, false);
                tile.content = newContentGameObject.AddComponent<Content>();
                tile.content.ParentTile = tile;
                tile.content.uri = GetFullContentUri(tile);

                //Request tile content update via optional prioritiser, or load directly
                if (usingPrioritiser)
                {
                    if(!tile.requestedUpdate)
                        tilePrioritiser.RequestUpdate(tile);
                }
                else
                {
                    tile.content.Load();
                }
            }
        }

        private void RequestDispose(Tile tile)
        {
            if (!tile.content) return;

            if (usingPrioritiser && !tile.requestedDispose)
            {
                tilePrioritiser.RequestDispose(tile);
            }
            else
            {
                tile.content.Dispose();
            }
        }

        private void ReadTileset(JSONNode rootnode)
        {   
            transformValues = new double[16] {1.0, 0.0, 0.0, 0.0,0.0, 1.0, 0.0, 0.0,0.0, 0.0, 1.0, 0.0,0.0, 0.0, 0.0, 1.0 };
            JSONNode transformNode = rootnode["transform"];
            if (transformNode!=null)
            {
                for (int i = 0; i < 16; i++)
                {
                    transformValues[i] = transformNode[i].AsDouble;
                }
            }
            
            JSONNode implicitTilingNode = rootnode["implicitTiling"];
            if (implicitTilingNode != null)
            {
                tilingMethod = TilingMethod.implicitTiling;           
            }

            //setup location and rotation
            switch (tilingMethod)
            {
                case TilingMethod.explicitTiling:
                    Tile rootTile = new Tile();
                    root = ReadExplicitNode(rootnode, rootTile);
                    root.screenSpaceError = float.MaxValue;
                    ApplyRootOrientation();
                    StartCoroutine(LoadInView());
                    break;
                case TilingMethod.implicitTiling:
                    ReadImplicitTiling(rootnode);
                    break;
                default:
                    break;
            }
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

        private void ReadImplicitTiling(JSONNode rootnode)
        {
            implicitTilingSettings = new ImplicitTilingSettings();
            string refine = rootnode["refine"].Value;
            switch (refine)
            {
                case "REPLACE":
                    implicitTilingSettings.refinementType = RefinementType.Replace;
                    break;
                case "ADD":
                    implicitTilingSettings.refinementType = RefinementType.Add;
                    break;
                default:
                    break;
            }
            implicitTilingSettings.geometricError = rootnode["geometricError"].AsFloat;
            implicitTilingSettings.boundingRegion = new double[6];
            for (int i = 0; i < 6; i++)
            {
                implicitTilingSettings.boundingRegion[i] = rootnode["boundingVolume"]["region"][i].AsDouble;
            }
            implicitTilingSettings.contentUri = rootnode["content"]["uri"].Value;
            JSONNode implicitTilingNode = rootnode["implicitTiling"];
            string subdivisionScheme = implicitTilingNode["subsivisionScheme"].Value;
            switch (subdivisionScheme)
            {
                case "QUADTREE":
                    implicitTilingSettings.subdivisionScheme = SubdivisionScheme.Quadtree;
                    break;
                default:
                    implicitTilingSettings.subdivisionScheme = SubdivisionScheme.Octree;
                    break;
            }
            implicitTilingSettings.subtreeLevels = implicitTilingNode["subtreeLevels"];
            implicitTilingSettings.subtreeUri = implicitTilingNode["subtrees"]["uri"].Value;


            ReadSubtree subtreeReader = GetComponent<ReadSubtree>();
            string subtreeURL = tilesetUrl.Replace(tilesetFilename, implicitTilingSettings.subtreeUri)
                                .Replace("{level}", "0")
                                .Replace("{x}", "0")
                                .Replace("{y}", "0");

            Debug.Log("Load subtree: " + subtreeURL);
            subtreeReader.DownloadSubtree(subtreeURL, implicitTilingSettings, ReturnTiles);
        }

        private void ReturnTiles(Tile rootTile)
        {
            root = rootTile;
            ApplyRootOrientation();

            StartCoroutine(LoadInView());
        }

        /// <summary>
        /// Check what tiles should be loaded/unloaded based on view recursively
        /// </summary>
        private IEnumerator LoadInView()
        {
            yield return new WaitUntil(() => root != null);
            while (true)
            {
                //If camera changed, recalculate what tiles are be in view
                currentCamera.transform.GetPositionAndRotation(out currentCameraPosition, out currentCameraRotation);

                if (nestedTreeLoaded || CameraChanged())
                {
                    nestedTreeLoaded = false;

                    lastCameraAngle = (currentCamera.orthographic ? currentCamera.orthographicSize : currentCamera.fieldOfView);
                    currentCamera.transform.GetPositionAndRotation(out lastCameraPosition, out lastCameraRotation);

                    SetSSEComponent(currentCamera);
                    DisposeTilesOutsideView(currentCamera);

                    root.IsInViewFrustrum(currentCamera);
                    yield return LoadInViewRecursively(root, currentCamera);
                }

                yield return null;
            }
        }

        /// <summary>
        /// Returns if current camera changed in position/rotation/fov or size in the last frame
        /// </summary>
        private bool CameraChanged()
        {
            return 
                (currentCamera.orthographic == true && lastCameraAngle != currentCamera.orthographicSize) || 
                (currentCamera.orthographic == false && lastCameraAngle != currentCamera.fieldOfView) || 
                lastCameraPosition != currentCameraPosition || 
                lastCameraRotation != currentCameraRotation;
        }

        /// <summary>
        /// Check for tiles in our visibile tiles list that moved out of the view / max distance.
        /// Request dispose for tiles that moved out of view
        /// </summary>
        /// <param name="currentCamera">Camera to use for visibility check</param>
        private void DisposeTilesOutsideView(Camera currentCamera)
        {
            //Clean up list op previously loaded tiles outside of view
            for (int i = visibleTiles.Count - 1; i >= 0; i--)
            {
                var child = visibleTiles[i];
                var closestPointOnBounds = child.ContentBounds.ClosestPoint(currentCamera.transform.position); //Returns original point when inside the bounds
                CalculateTileScreenSpaceError(child, currentCamera, closestPointOnBounds);

                if ((child.screenSpaceError <= maximumScreenSpaceError && child.ChildrenHaveContent()) || !child.IsInViewFrustrum(currentCamera))
                {
                    RequestDispose(child);
                    visibleTiles.RemoveAt(i);
                }
            }
        }

        private void CalculateTileScreenSpaceError(Tile child, Camera currentMainCamera, Vector3 closestPointOnBounds)
        {
            var sse = (sseComponent * (float)child.geometricError) / Vector3.Distance(currentMainCamera.transform.position, closestPointOnBounds);
            if (sse == float.PositiveInfinity || sse == 0)
                sse = float.MaxValue;
            child.screenSpaceError = sse;
        }

        private IEnumerator LoadInViewRecursively(Tile parentTile, Camera currentCamera)
        {
            yield return LoadNestedTileset(parentTile);

            foreach (var tile in parentTile.children)
            {
                if (visibleTiles.Contains(tile)) continue;

                var closestPointOnBounds = tile.ContentBounds.ClosestPoint(currentCamera.transform.position); //Returns original point when inside the bounds
                CalculateTileScreenSpaceError(tile, currentCamera, closestPointOnBounds);

                //Smaller geometric error? Too detailed for our current view so Dispose!
                if (tile.geometricError <= sseComponent && tile.content)
                {
                    RequestDispose(tile);
                }
                else
                {
                    var tileIsInView = tile.IsInViewFrustrum(currentCamera);
                    
                    //Check for children ( and if closest child can refine )
                    var canRefineToChildren = GetCanRefineToChildren(tile);

                    if (tileIsInView)
                    {
                        if (canRefineToChildren)
                        {
                            yield return LoadInViewRecursively(tile, currentCamera);
                        }
                        else if (!canRefineToChildren && !tile.contentUri.Contains(".json") && tile.contentUri.Length > 0)
                        {
                            RequestContentUpdate(tile);
                            visibleTiles.Add(tile);
                        }
                    }
                    else if(tile.content)
                    {
                        RequestDispose(tile);
                    }
                }
            }
        }

        private IEnumerator LoadNestedTileset(Tile tile)
        {
            if (tilingMethod == TilingMethod.explicitTiling)
            {
                if (tile.contentUri.Contains(".json") && !tile.nestedTilesLoaded)
                {
                    string nestedJsonPath = GetFullContentUri(tile);
                    Debug.Log($"Nested {nestedJsonPath}");
                    UnityWebRequest www = UnityWebRequest.Get(nestedJsonPath);
                    yield return www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.Log(www.error);
                    }
                    else
                    {
                        string jsonstring = www.downloadHandler.text;
                        tile.nestedTilesLoaded = true;

                        JSONNode node = JSON.Parse(jsonstring)["root"];
                        ReadExplicitNode(node, tile);

                        nestedTreeLoaded = true;
                    }
                }
                else if (tile.contentUri.Length == 0)
                {
                    Debug.LogWarning(tile.contentUri);
                }
            }
            else if (tilingMethod == TilingMethod.implicitTiling)
            {
                //Possible future nested subtree support.
            }
        }

        private string GetFullContentUri(Tile tile)
        {
            var relativeContentUrl = tile.contentUri;

            if (tilingMethod == TilingMethod.implicitTiling)
            {
                relativeContentUrl = (implicitTilingSettings.contentUri.Replace("{level}", tile.X.ToString()).Replace("{x}", tile.Y.ToString()).Replace("{y}", tile.Z.ToString()));
            }

            //RDam specific temp fix.
            relativeContentUrl = relativeContentUrl.Replace("../", "");

            var fullPath = (tile.contentUri.StartsWith("/")) ? rootPath + relativeContentUrl : absolutePath + relativeContentUrl;

            //Combine query to pass on session id and API key (Google Maps 3DTiles API style)
            UriBuilder uriBuilder = new(fullPath);
            NameValueCollection contentQueryParameters = HttpUtility.ParseQueryString(uriBuilder.Query);
            foreach (string key in contentQueryParameters.Keys)
            {
                if (!queryParameters.AllKeys.Contains(key))
                {
                    queryParameters.Add(key, contentQueryParameters[key]);
                }
            }

            uriBuilder.Query = queryParameters.ToString();
            var url = uriBuilder.ToString();

            return url;
        }

        private bool GetCanRefineToChildren(Tile tile)
        {
            if (tile.screenSpaceError > maximumScreenSpaceError){

                if (tilingMethod == TilingMethod.implicitTiling)
                {
                    return tile.children.Count > 0 && (tile.screenSpaceError / 2.0f > maximumScreenSpaceError);
                }
                else if (tilingMethod == TilingMethod.explicitTiling)
                {
                    return tile.children.Count > 0 || tile.contentUri.Contains(".json");
                }
            }
            return false;
        }

        /// <summary>
        /// Screen-space error component calculation.
        /// Screen height is clamped to limit the amount of geometry that
        /// would be loaded on very high resolution displays.
        /// </summary>
        public void SetSSEComponent(Camera currentCamera)
        {
            if(usingPrioritiser) maxScreenHeightInPixels = tilePrioritiser.MaxScreenHeightInPixels;

            var screenHeight = (maxScreenHeightInPixels > 0) ? Mathf.Min(maxScreenHeightInPixels,Screen.height) : Screen.height;

            if (currentCamera.orthographic)
            {
                sseComponent = screenHeight / currentCamera.orthographicSize;
            }
            else
            {
                var coverage = 2 * Mathf.Tan((Mathf.Deg2Rad * currentCamera.fieldOfView) / 2);
                sseComponent = screenHeight / coverage;
            }
        }
    }

    public enum TilingMethod
    {
        explicitTiling,
        implicitTiling
    }

    public enum RefinementType
    {
        Replace,
        Add
    }
    public enum SubdivisionScheme
    {
        Quadtree,
        Octree
    }

    [System.Serializable]
    public class ImplicitTilingSettings
    {
        public RefinementType refinementType;
        public SubdivisionScheme subdivisionScheme;
        public int subtreeLevels;
        public string subtreeUri;
        public string contentUri;
        public float geometricError;
        public double[] boundingRegion;
    }
}
