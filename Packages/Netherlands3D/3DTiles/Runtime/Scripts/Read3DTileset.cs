using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Networking;
using System;
using Netherlands3D.Core;
using System.IO;
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

        private float sseComponent = -1;
        private List<Tile> visibleTiles = new List<Tile>();

        [SerializeField] private TilePrioritiser tilePrioritiser;
        private bool usingPrioritiser = false;

        private Camera currentCamera;
        private Vector3 lastCameraPosition;
        private Quaternion lastCameraRotation;
        private Vector3 currentCameraPosition;
        private Quaternion currentCameraRotation;
        private float lastCameraAngle = 60;

        private const string tilesetFilename = "tileset.json";

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

            absolutePath = tilesetUrl.Replace(tilesetFilename, "");
            StartCoroutine(LoadTileset());

            CoordConvert.relativeOriginChanged.AddListener(RelativeCenterChanged);
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
            //Point set up from new origin
            AlignWithUnityWorld();

            //Flag all calculated bounds to be recalculated when tile bounds is requested
            RecalculateAllTileBounds(root);
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

        private void RequestUpdate(Tile tile)
        {
            if (!tile.content)
            {
                tile.content = gameObject.AddComponent<Content>();
                tile.content.ParentTile = tile;
                if (tilingMethod == TilingMethod.implicitTiling)
                {
                    tile.content.uri = absolutePath + implicitTilingSettings.contentUri.Replace("{level}", tile.X.ToString()).Replace("{x}", tile.Y.ToString()).Replace("{y}", tile.Z.ToString());
                }
                else
                {
                    tile.content.uri = absolutePath + tile.contentUri;
                    if(tile.content.uri.Contains(".json"))
                    {
                        StartCoroutine(LoadNestedDataSet(tile, tile.content.uri));
                    }
                }

                //Request tile content update via optional prioritiser, or load directly
                if (usingPrioritiser && !tile.requestedUpdate)
                {
                    tilePrioritiser.RequestUpdate(tile);
                }
                else
                {
                    tile.content.Load();
                }
            }
        }

        private IEnumerator LoadNestedDataSet(Tile tile, string datasetPath)
        {
            UnityWebRequest www = UnityWebRequest.Get(datasetPath);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                string jsonstring = www.downloadHandler.text;

                JSONNode node = JSON.Parse(jsonstring)["root"];
                ReadExplicitNode(node, tile);
            }
        }

        private void RequestDispose(Tile tile)
        {
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
            positionECEF = new Vector3ECEF(transformValues[12], transformValues[13], transformValues[14]);
            AlignWithUnityWorld();
            switch (tilingMethod)
            {
                case TilingMethod.explicitTiling:
                    Tile rootTile = new Tile();
                    root = ReadExplicitNode(rootnode, rootTile);
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
        private Tile ReadExplicitNode(JSONNode node, Tile tile)
        {
            tile.transform = new double[16] { 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0 };
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

        private void ParseBoundingVolume(Tile tile, JSONNode boundingVolumeNode)
        {
            if (boundingVolumeNode != null)
            {
                Dictionary<string, BoundingVolumeType> boundingVolumeTypes = new()
                {
                    { "region", BoundingVolumeType.Region },
                    { "box", BoundingVolumeType.Box },
                    { "sphere", BoundingVolumeType.Sphere }
                };

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

        private int GetBoundingVolumeLength(BoundingVolumeType type)
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

        private void AlignWithUnityWorld()
        {
            transform.SetPositionAndRotation(
                CoordConvert.ECEFToUnity(positionECEF), 
                CoordConvert.ecefRotionToUp()
            );
        }

        private void ReadImplicitTiling(JSONNode rootnode)
        {
            implicitTilingSettings = new ImplicitTilingSettings();
            string refine = rootnode["refine"].Value;
            switch (refine)
            {
                case "REPLACE":
                    implicitTilingSettings.refinementtype = RefinementType.Replace;
                    break;
                case "ADD":
                    implicitTilingSettings.refinementtype = RefinementType.Add;
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

                if (CameraChanged())
                {
                    lastCameraAngle = (currentCamera.orthographic ? currentCamera.orthographicSize : currentCamera.fieldOfView);
                    currentCamera.transform.GetPositionAndRotation(out lastCameraPosition, out lastCameraRotation);

                    SetSSEComponent(currentCamera);
                    DisposeTilesOutsideView(currentCamera);

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
        /// <param name="currentMainCamera">Camera to use for visibility check</param>
        private void DisposeTilesOutsideView(Camera currentMainCamera)
        {
            //Clean up list op previously loaded tiles outside of view
            for (int i = visibleTiles.Count - 1; i >= 0; i--)
            {
                var child = visibleTiles[i];
                var closestPointOnBounds = child.ContentBounds.ClosestPoint(currentMainCamera.transform.position); //Returns original point when inside the bounds

                var tileScreenSpaceError = (sseComponent * (float)child.geometricError) / Vector3.Distance(currentMainCamera.transform.position, closestPointOnBounds);
                child.screenSpaceError = tileScreenSpaceError;
                if (tileScreenSpaceError <= maximumScreenSpaceError || !child.IsInViewFrustrum(currentMainCamera))
                {
                    RequestDispose(child);
                    visibleTiles.RemoveAt(i);
                }
            }
        }

        private IEnumerator LoadInViewRecursively(Tile parentTile, Camera currentCamera)
        {
            foreach (var tile in parentTile.children)
            {
                if (visibleTiles.Contains(tile)) continue;

                var closestPointOnBounds = tile.ContentBounds.ClosestPoint(currentCamera.transform.position); //Returns original point when inside the bounds
                var tileScreenSpaceError = (sseComponent * (float)tile.geometricError) / Vector3.Distance(currentCamera.transform.position, closestPointOnBounds);
                tile.screenSpaceError = tileScreenSpaceError;
                if (tile.geometricError <= sseComponent && tile.content)
                {
                    RequestDispose(tile);
                }
                else if (tileScreenSpaceError > maximumScreenSpaceError && tile.IsInViewFrustrum(currentCamera))
                {
                    //Check for children ( and if closest child can refine ). Closest child would have same closest point as parent on bounds, so simply divide pixelError by 2
                    var canRefineToChildren = tile.children.Count > 0 && (tileScreenSpaceError / 2.0f > maximumScreenSpaceError);
                    if (canRefineToChildren)
                    {
                        yield return LoadInViewRecursively(tile, currentCamera);
                    }
                    else if (tile.hascontent && !canRefineToChildren)
                    {
                        RequestUpdate(tile);
                        visibleTiles.Add(tile);
                    }
                }
            }
        }

        /// <summary>
        /// Screen-space error component calculation.
        /// Screen height is clamped to limit the amount of geometry that
        /// would be loaded on very high resolution displays.
        /// </summary>
        public void SetSSEComponent(Camera currentCamera)
        {
            if(usingPrioritiser) maxScreenHeightInPixels = tilePrioritiser.MaxScreenHeightInPixels;

            var screenHeight = Mathf.Min(maxScreenHeightInPixels,Screen.height);

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

#if UNITY_EDITOR
        /// <summary>
        /// Editor only methods for loading all tiles from context menu
        /// </summary>
        [ContextMenu("Load all content")]
        private void LoadAll()
        {
            StartCoroutine(LoadAllTileContent());
        }
        private IEnumerator LoadAllTileContent()
        {
            yield return new WaitForEndOfFrame();
            yield return LoadContentInChildren(root);
        }
        private IEnumerator LoadContentInChildren(Tile tile)
        {
            foreach (var child in tile.children)
            {
                if (child.hascontent)
                {
                    RequestUpdate(child);
                }
                yield return new WaitForEndOfFrame();
                yield return LoadContentInChildren(child);
            }
        }

        [ContextMenu("Download entire dataset")]
        public void DownloadEntireDataset()
        {
            StartCoroutine(DownloadTileSet());
        }

        private IEnumerator DownloadTileSet()
        {
            //Main tileset json
            UnityWebRequest webRequest = UnityWebRequest.Get(tilesetUrl);
            yield return webRequest.SendWebRequest();
            var folder = EditorUtility.SaveFolderPanel("Save tileset to folder", "", "");
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                string jsonstring = webRequest.downloadHandler.text;
                File.WriteAllText(folder + "/" + tilesetFilename, jsonstring);
            }

            //Subtree(s)
            var subtreePath = implicitTilingSettings.subtreeUri.Replace("{level}", "0")
                                                               .Replace("{x}", "0")
                                                               .Replace("{y}", "0");

            string subtreeURL = tilesetUrl.Replace(tilesetFilename, subtreePath);

            webRequest = UnityWebRequest.Get(subtreeURL);
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                var data = webRequest.downloadHandler.data;

                var newFile = new FileInfo(folder + "/" + subtreePath);
                newFile.Directory.Create();
                File.WriteAllBytes(folder + "/" + subtreePath, data);
            }

            Debug.Log("<color=green>All done!</color>");
        }

        private IEnumerator DownloadContent(Tile parentTile, string folder)
        {
            foreach (var tile in parentTile.children)
            {
                if (tile.hascontent)
                {
                    var tileContentPath = implicitTilingSettings.contentUri.Replace("{level}", tile.X.ToString()).Replace("{x}", tile.Y.ToString()).Replace("{y}", tile.Z.ToString());
                    var contentUrl = absolutePath + tileContentPath;
                    UnityWebRequest www = UnityWebRequest.Get(contentUrl);
                    yield return www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.Log(www.error);
                    }
                    else
                    {
                        var data = www.downloadHandler.data;
                        var localContentPath = folder + "/" + tileContentPath;
                        Debug.Log("Saving " + localContentPath);

                        var newFile = new FileInfo(localContentPath);
                        newFile.Directory.Create();

                        File.WriteAllBytes(localContentPath, data);
                    }
                }
                yield return new WaitForEndOfFrame();
                yield return DownloadContent(tile, folder);
            }
        }
#endif

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
        public RefinementType refinementtype;
        public SubdivisionScheme subdivisionScheme;
        public int subtreeLevels;
        public string subtreeUri;
        public string contentUri;
        public float geometricError;
        public double[] boundingRegion;
    }
}
