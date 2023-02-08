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
namespace Netherlands3D.Core.Tiles
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

        public int maxPixelError = 5;
        private float sseComponent = -1;

        private List<Tile> visibleTiles = new List<Tile>();

        [SerializeField] private TilePrioritiser tilePrioritiser;
        private Camera currentCamera;
        private Vector3 lastCameraPosition;
        private Quaternion lastCameraRotation;
        private Vector3 currentCameraPosition;
        private Quaternion currentCameraRotation;

        private void OnEnable()
        {
            currentCamera = Camera.main;
        }

        /// <summary>
        /// Optional injection of tile prioritiser system
        /// </summary>
        /// <param name="tilePrioritiser">Prioritising system with TilePrioritiser base class</param>
        public void SetTilePrioritiser(TilePrioritiser tilePrioritiser)
        {
            this.tilePrioritiser = tilePrioritiser;
        }

        void Start()
        {
            if(tilePrioritiser) tilePrioritiser.SetCamera(currentCamera);

            absolutePath = tilesetUrl.Replace("tileset.json", "");
            StartCoroutine(LoadTileset());

            CoordConvert.relativeOriginChanged.AddListener(RelativeCenterChanged);
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
                tile.content.uri = absolutePath + implicitTilingSettings.contentUri.Replace("{level}", tile.X.ToString()).Replace("{x}", tile.Y.ToString()).Replace("{y}", tile.Z.ToString());

                if (tilePrioritiser != null && !tile.requestedUpdate)
                {
                    tilePrioritiser.RequestUpdate(tile);
                }
                else
                {
                    tile.content.Load();
                }
            }
        }

        private void DisposeDirectly(Tile tile)
        {
            if (tilePrioritiser != null)
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
            JSONNode transformNode = rootnode["transform"];
            transformValues = new double[16];
            for (int i = 0; i < 16; i++)
            {
                transformValues[i] = transformNode[i].AsDouble;
            }
            JSONNode implicitTilingNode = rootnode["implicitTiling"];
            if (implicitTilingNode != null)
            {
                ReadImplicitTiling(rootnode);
            }

            //setup location and rotation
            positionECEF = new Vector3ECEF(transformValues[12], transformValues[13], transformValues[14]);
            AlignWithUnityWorld();
        }

        private void AlignWithUnityWorld()
        {
            transform.position = CoordConvert.ECEFToUnity(positionECEF);
            transform.rotation = CoordConvert.ecefRotionToUp();
        }

        private void ReadImplicitTiling(JSONNode rootnode)
        {
            tilingMethod = TilingMethod.implicitTiling;
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
            string subtreeURL = tilesetUrl.Replace("tileset.json", implicitTilingSettings.subtreeUri)
                                .Replace("{level}", "0")
                                .Replace("{x}", "0")
                                .Replace("{y}", "0");

            Debug.Log("Load subtree: " + subtreeURL);
            subtreeReader.DownloadSubtree(subtreeURL, implicitTilingSettings, ReturnTiles);
        }

        public void ReturnTiles(Tile rootTile)
        {
            root = rootTile;
            StartCoroutine(LoadInView());
        }

        /// <summary>
        /// Check what tiles should be loaded/unloaded based on view
        /// </summary>
        private IEnumerator LoadInView()
        {
            yield return new WaitUntil(() => root != null);
            while (true)
            {
                //If camera changed, recalculate what tiles are be in view
                currentCamera.transform.GetPositionAndRotation(out currentCameraPosition, out currentCameraRotation);
                if (lastCameraPosition != currentCameraPosition || lastCameraRotation != currentCameraRotation)
                {
                    currentCamera.transform.GetPositionAndRotation(out lastCameraPosition, out lastCameraRotation);                 

                    SetSSEComponent(currentCamera);
                    DisposeTilesOutsideView(currentCamera);

                    yield return LoadInViewRecursively(root, currentCamera);
                }

                yield return null;
            }
        }

        private void DisposeTilesOutsideView(Camera currentMainCamera)
        {
            //Clean up list op previously loaded tiles outside of view
            for (int i = visibleTiles.Count - 1; i >= 0; i--)
            {
                var child = visibleTiles[i];
                var closestPointOnBounds = child.ContentBounds.ClosestPoint(currentMainCamera.transform.position); //Returns original point when inside the bounds

                var screenSpaceError = (sseComponent * child.geometricError) / Vector3.Distance(currentMainCamera.transform.position, closestPointOnBounds);
                child.screenSpaceError = screenSpaceError;
                if (screenSpaceError <= maxPixelError || !child.IsInViewFrustrum(currentMainCamera))
                {
                    DisposeDirectly(child);
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
                var pixelError = (sseComponent * tile.geometricError) / Vector3.Distance(currentCamera.transform.position, closestPointOnBounds);

                if (tile.geometricError <= sseComponent && tile.content)
                {
                    tile.Dispose();
                }
                else if (pixelError > maxPixelError && tile.IsInViewFrustrum(currentCamera))
                {
                    //Check for children ( and if closest child can refine ). Closest child would have same closest point as parent on bounds, so simply divide pixelError by 2
                    var canRefineToChildren = tile.children.Count > 0 && (pixelError / 2.0f > maxPixelError);
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
        /// Screen-space error component calculation
        /// </summary>
        public void SetSSEComponent(Camera currentCamera)
        {
            sseComponent = Screen.height / (2 * Mathf.Tan(Mathf.Deg2Rad * currentCamera.fieldOfView / 2));
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
            UnityWebRequest www = UnityWebRequest.Get(tilesetUrl);
            yield return www.SendWebRequest();
            var folder = EditorUtility.SaveFolderPanel("Save tileset to folder", "", "");
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                string jsonstring = www.downloadHandler.text;
                File.WriteAllText(folder + "/tileset.json", jsonstring);
            }

            //Content
            //yield return DownloadContent(root, folder);

            //Subtree(s)
            var subtreePath = implicitTilingSettings.subtreeUri.Replace("{level}", "0")
                                                               .Replace("{x}", "0")
                                                               .Replace("{y}", "0");

            string subtreeURL = tilesetUrl.Replace("tileset.json", subtreePath);

            www = UnityWebRequest.Get(subtreeURL);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                var data = www.downloadHandler.data;

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