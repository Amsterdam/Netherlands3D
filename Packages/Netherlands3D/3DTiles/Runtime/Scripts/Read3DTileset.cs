using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.Networking;
using System;
using Netherlands3D.Core;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Netherlands3D.Tiles3D
{
    [RequireComponent(typeof(ReadSubtree))]
    public class Read3DTileset : MonoBehaviour
    {

        public string tilesetUrl = "https://storage.googleapis.com/ahp-research/maquette/kadaster/3dbasisvoorziening/test/landuse_1_1/tileset.json";
        public string publicKey;
        public string personalKey;
        private string absolutePath = "";
        private string rootPath = "";
        private NameValueCollection queryParameters;

        public Tile root;
        public double[] transformValues;

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
        private bool usingPrioritiser = true;

        private Camera currentCamera;
        private Vector3 lastCameraPosition;
        private Quaternion lastCameraRotation;
        private Vector3 currentCameraPosition;
        private Quaternion currentCameraRotation;
        private float lastCameraAngle = 60;

        private string tilesetFilename = "tileset.json";

        private bool nestedTreeLoaded = false;

        private void Awake()
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(personalKey)==false)
            {
                tilesetUrl = tilesetUrl + "?key=" + personalKey;
            }

#else
if (string.IsNullOrEmpty(publicKey)==false)
            {
            tilesetUrl = tilesetUrl + "?key=" + publicKey;
            }
#endif
        }

        private void OnEnable()
        {
            currentCamera = Camera.main;
            StartCoroutine(LoadInView());
        }

        void Start()
        {
            if (usingPrioritiser)
            {
                tilePrioritiser.SetCamera(currentCamera);
            }

            ExtractDatasetPaths();

            StartCoroutine(LoadTileset());
            
            SetGlobalRDOrigin globalOrigin = FindObjectOfType<SetGlobalRDOrigin>();
            if (globalOrigin != null)
            {
                //globalOrigin.relativeOriginChanged.AddListener(RelativeCenterChanged);
            }
        }

        private void ExtractDatasetPaths()
        {
            Uri uri = new(tilesetUrl);
            absolutePath = tilesetUrl.Substring(0,tilesetUrl.LastIndexOf("/")+1);
            if (tilesetUrl.StartsWith("file://"))
            {
                rootPath = absolutePath;
            }
            else
            {
                rootPath = uri.GetLeftPart(UriPartial.Authority);
            }
            queryParameters = ParseQueryString(uri.Query);
            Debug.Log($"Query url {ToQueryString(queryParameters)}");
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
        /// TODO: Use existing nl3d query parser / or move to Uri extention?
        /// </summary>
        /// <param name="queryString">?param=value&otherparam=othervalue</param>
        public NameValueCollection ParseQueryString(string queryString)
        {
            // Remove leading '?' if present
            if (queryString.StartsWith("?"))
                queryString = queryString.Substring(1);

            NameValueCollection queryParameters = new NameValueCollection();

            string[] querySegments = queryString.Split('&');
            for (int i = 0; i < querySegments.Length; i++)
            {
                string[] parts = querySegments[i].Split('=');
                if (parts.Length > 1)
                {
                    string key = UnityWebRequest.UnEscapeURL(parts[0]);
                    string value = UnityWebRequest.UnEscapeURL(parts[1]);
                    queryParameters.Add(key, value);
                }
            }

            return queryParameters;
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
                Debug.Log($"Could not load tileset from url:{tilesetUrl} Error:{www.error}");
            }
            else
            {
                string jsonstring = www.downloadHandler.text;

                JSONNode rootnode = JSON.Parse(jsonstring)["root"];
               root = ParseTileset.ReadTileset(rootnode);
                
            }
        }

        private void RequestContentUpdate(Tile tile)
        {
            //tile.parent.IncrementLoadingChildren();
            //foreach (var child in tile.children)
            //{
            //    child.IncrementLoadingParents();
            //}
            if (!tile.content)
            {
                var newContentGameObject = new GameObject($"{tile.X},{tile.Y},{tile.Z} content");
                newContentGameObject.transform.SetParent(transform, false);
                newContentGameObject.layer = 11;
                tile.content = newContentGameObject.AddComponent<Content>();
                tile.content.State = Content.ContentLoadState.NOTLOADING;
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

                //if (nestedTreeLoaded || CameraChanged())
                //{
                //    nestedTreeLoaded = false;

                    lastCameraAngle = (currentCamera.orthographic ? currentCamera.orthographicSize : currentCamera.fieldOfView);
                    currentCamera.transform.GetPositionAndRotation(out lastCameraPosition, out lastCameraRotation);

                    SetSSEComponent(currentCamera);
                    DisposeTilesOutsideView(currentCamera);

                    //root.IsInViewFrustrum(currentCamera);
                foreach (var child in root.children)
                {
                    LoadInViewRecursively(child, currentCamera);
                }
                    
                //}

                yield return null;
            }
        }

        /// <summary>
        /// Check for tiles in our visibile tiles list that moved out of the view / max distance.
        /// Request dispose for tiles that moved out of view
        /// </summary>
        /// <param name="currentCamera">Camera to use for visibility check</param>
        private void DisposeTilesOutsideView(Camera currentCamera)
        {
            for (int i = visibleTiles.Count - 1; i >= 0; i--)
            {
                var tile = visibleTiles[i];
                var closestPointOnBounds = tile.ContentBounds.ClosestPoint(currentCamera.transform.position); //Returns original point when inside the bounds
                CalculateTileScreenSpaceError(tile, currentCamera, closestPointOnBounds);
            }

            //Clean up list op previously loaded tiles outside of view
            for (int i = visibleTiles.Count - 1; i >= 0; i--)
            {
                var tile = visibleTiles[i];
                var tileIsInView = tile.IsInViewFrustrum(currentCamera);
                if (!tileIsInView)
                {
                    tilePrioritiser.RequestDispose(tile,true);
                    visibleTiles.RemoveAt(i);
                    continue;
                }

                if (tile.parent.CountLoadedParents() + tile.parent.CountLoadingParents() > 1)
                {
                    tilePrioritiser.RequestDispose(tile, true);
                    visibleTiles.RemoveAt(i);
                    continue;
                }

                if (tile.screenSpaceError > maximumScreenSpaceError) //too little detail
                {
                    
                    if (tile.CountLoadingChildren() == 0)
                    {
                        
                        if (tile.CountLoadedChildren() > 0)
                        {
                            tilePrioritiser.RequestDispose(tile);
                            
                            
                            visibleTiles.RemoveAt(i);
                        }
                    }
                }
                if (tile.screenSpaceError < maximumScreenSpaceError) //too much detail
                {
                    if (tile.CountLoadedParents() > 0)
                    {
                        if (tile.getParentSSE()<maximumScreenSpaceError)
                        {
                            tilePrioritiser.RequestDispose(tile, true);
                            visibleTiles.RemoveAt(i);
                        }
                        
                    }
                }

                int childcount = tile.CountLoadedChildren();
                int layerIndex = 12;
                if (childcount==0)
                {
                    layerIndex = 11;
                }
                if (tile.content != null)
                {
                    if (tile.content.gameObject != null)
                    {
                        foreach (var item in tile.content.gameObject.GetComponentsInChildren<Transform>())
                        {
                            item.gameObject.layer = layerIndex;
                        }
                    }
                }

            }
        }

        private void CalculateTileScreenSpaceError(Tile child, Camera currentMainCamera, Vector3 closestPointOnBounds)
        {
            float sse;
            if (Vector3.Distance(currentMainCamera.transform.position, closestPointOnBounds) < 0.1)
            {
                sse = float.MaxValue;
            }
            else
            {
                sse = (sseComponent * (float)child.geometricError) / Vector3.Distance(currentMainCamera.transform.position, closestPointOnBounds);
            }   
            child.screenSpaceError = sse;
        }

        private void LoadInViewRecursively(Tile tile, Camera currentCamera)
        {
            var tileIsInView = tile.IsInViewFrustrum(currentCamera);
            if (!tileIsInView)
            {
                return;
            }

            if (tile.isLoading == false && tile.children.Count == 0 && tile.contentUri.Contains(".json"))
            {
                tile.isLoading = true;
                StartCoroutine(LoadNestedTileset(tile));
                return;
            }

            var closestPointOnBounds = tile.ContentBounds.ClosestPoint(currentCamera.transform.position); //Returns original point when inside the bounds
            CalculateTileScreenSpaceError(tile, currentCamera, closestPointOnBounds);
            var enoughDetail = tile.screenSpaceError < maximumScreenSpaceError;

            if (enoughDetail)
            {
                var Has3DContent = tile.contentUri.Length > 0 && !tile.contentUri.Contains(".json");
                
                if (Has3DContent)
                {
                    int loadingParentsCount = tile.CountLoadingParents();
                    int loadedParentsCount = tile.CountLoadedParents();
                    if (loadedParentsCount+ loadingParentsCount<2)
                    {
                        if (!visibleTiles.Contains(tile))
                        {
                            RequestContentUpdate(tile);
                            visibleTiles.Add(tile);
                        }
                    }
                    
                }
                return;
            }


            foreach (var childTile in tile.children)
            {
                LoadInViewRecursively(childTile, currentCamera);
            }
        }

        private IEnumerator LoadNestedTileset(Tile tile)
        {
            if (tilingMethod == TilingMethod.explicitTiling)
            {
                if (tile.contentUri.Contains(".json") && !tile.nestedTilesLoaded)
                {
                    string nestedJsonPath = GetFullContentUri(tile);
                    UnityWebRequest www = UnityWebRequest.Get(nestedJsonPath);
                    yield return www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {            
                        Debug.Log(www.error + " at " + nestedJsonPath);
                    }
                    else
                    {
                        string jsonstring = www.downloadHandler.text;
                        tile.nestedTilesLoaded = true;

                        JSONNode node = JSON.Parse(jsonstring)["root"];
                        ParseTileset.ReadExplicitNode(node, tile);
                        nestedTreeLoaded = true;
                    }
                }
                tile.isLoading = false;
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
            NameValueCollection contentQueryParameters = ParseQueryString(uriBuilder.Query);
            foreach (string key in contentQueryParameters.Keys)
            {
                if (!queryParameters.AllKeys.Contains(key))
                {
                    queryParameters.Add(key, contentQueryParameters[key]);
                }
            }
          
            uriBuilder.Query = ToQueryString(queryParameters);
            var url = uriBuilder.ToString();
            return url;
        }

        private string ToQueryString(NameValueCollection queryParameters)
        {
            if (queryParameters.Count == 0) return "";

            StringBuilder queryString = new StringBuilder();
            for (int i = 0; i < queryParameters.Count; i++)
            {
                string key = queryParameters.GetKey(i);
                string[] values = queryParameters.GetValues(i);

                if (!string.IsNullOrEmpty(key) && values != null)
                {
                    for (int j = 0; j < values.Length; j++)
                    {
                        string value = values[j];

                        if (queryString.Length > 0)
                            queryString.Append("&");

                        queryString.AppendFormat("{0}={1}", Uri.EscapeDataString(key), Uri.EscapeDataString(value));
                    }
                }
            }
            
            return "?" + queryString.ToString();
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
