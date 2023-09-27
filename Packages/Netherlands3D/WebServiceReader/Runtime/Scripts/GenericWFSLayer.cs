using System.Collections;
using System.Collections.Generic;
using Netherlands3D.WFSHandlers;
using UnityEngine;

namespace Netherlands3D.TileSystem
{
    public class GenericWFSLayer : Layer
    {
        [SerializeField] protected int maxSpawnsPerFrame = 100;

        protected WFS2 activeWFS;
        protected bool wfsGetCapabilitiesProcessed;

        [Header("WFS settings")]

        [SerializeField] protected string typeName;
        [SerializeField] protected List<string> selectedProperties;

        //Dictionary<Vector2Int, UnityAction<List<WFSFeatureData>>> listenerDictionary = new();

#if UNITY_EDITOR
        //private void OnValidate()
        //{
        //    if (Datasets.Count > 1)
        //    {
        //        Debug.LogError("SpotInfoWFSLayer can only have 1 dataset, removing others", gameObject);
        //        Datasets.RemoveRange(1, Datasets.Count - 1);
        //    }
        //}
#endif

        public override void Start()
        {
            base.Start();
            //StartCoroutine(RequestWFSCapabilitiesWhenAPIKeyReceived());
        }

        //private IEnumerator RequestWFSCapabilitiesWhenAPIKeyReceived()
        //{
        //    wfsGetCapabilitiesProcessed = false;

        //    //var parameters = new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>("dataset", SPOTInfoAPIKeyRequest.Datasets[0]) };
        //    var datasetURL = Datasets[0].path.ReplacePlaceholders(parameters);
        //    //var urlWithParameters = datasetURL + "?service=WFS&version=2.0.0&request=GetCapabilities";

        //    activeWFS = CreateNewWFS(datasetURL);
        //    OnWFSCreated(activeWFS);
        //}

        protected WFS2 CreateNewWFS(string datasetURL)
        {
            var activeWFS = new WFS2(datasetURL);
            //activeWFS.RequestHeaders = new Dictionary<string, string>() { { "Authorization", "Bearer " + SPOTInfoAPIKeyRequest.AccessToken } };

            return activeWFS;
        }

        protected virtual void OnWFSCreated(WFS2 createdWFS)
        {
            createdWFS.getCapabilitiesReceived.AddListener(OnWFSGetCapabilitiesReceived); //will send a list event with the feature names
            createdWFS.featureDescriptorsReceived.AddListener(OnWFSFeatureDescriptorsReceived);
            createdWFS.featureDataReceived.AddListener(ONWFSFeatureDataReceived);
        }

        public override void HandleTile(TileChange tileChange, System.Action<TileChange> callback = null)
        {
            TileAction action = tileChange.action;
            var tileKey = new Vector2Int(tileChange.X, tileChange.Y);
            switch (action)
            {
                case TileAction.Create:
                    Tile newTile = CreateNewTile(tileKey);
                    tiles.Add(tileKey, newTile);
                    newTile.runningCoroutine = StartCoroutine(GetFeature(tileChange, newTile, callback));
                    break;
                case TileAction.Upgrade:
                    tiles[tileKey].unityLOD++;
                    callback(tileChange);
                    break;
                case TileAction.Downgrade:
                    tiles[tileKey].unityLOD--;
                    callback(tileChange);
                    break;
                case TileAction.Remove:
                    InteruptRunningProcesses(tileKey);
                    RemoveGameObjectFromTile(tileKey);
                    tiles.Remove(tileKey);
                    OnTileDeleted(tileKey);
                    callback(tileChange);
                    return;
                default:
                    break;
            }
        }

        private Tile CreateNewTile(Vector2Int tileKey)
        {
            Tile tile = new Tile();
            tile.unityLOD = 0;
            tile.tileKey = tileKey;
            tile.layer = transform.gameObject.GetComponent<Layer>();
            //tile.gameObject = new GameObject();
            //tile.gameObject.transform.parent = transform.gameObject.transform;
            //tile.gameObject.layer = tile.gameObject.transform.parent.gameObject.layer;
            //tile.gameObject.transform.position = CoordConvert.RDtoUnity(tileKey);
            //tile.gameObject.name = tileKey.ToString();

            return tile;
        }

        protected void RefreshLayer()
        {
            var tileHandler = GetComponentInParent<TileHandler>();
            tileHandler.RemoveLayer(this);
            tileHandler.AddLayer(this);
        }

        private void RemoveGameObjectFromTile(Vector2Int tileKey)
        {
            if (tiles.ContainsKey(tileKey))
            {
                Tile tile = tiles[tileKey];
                if (tile == null)
                {
                    return;
                }
                if (tile.gameObject == null)
                {
                    return;
                }
                MeshFilter mf = tile.gameObject.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    Destroy(tile.gameObject.GetComponent<MeshFilter>().sharedMesh);
                }
                Destroy(tiles[tileKey].gameObject);
            }
        }

        protected virtual void OnTileDeleted(Vector2Int tileKey)
        {

        }

        private IEnumerator GetFeature(TileChange tileChange, Tile tile, System.Action<TileChange> callback = null)
        {
            yield return new WaitUntil(() => activeWFS != null);

            activeWFS.BBox = GetBoundingBox(tile);
            activeWFS.GetFeatureData(tile.tileKey, typeName, selectedProperties);
            callback(tileChange);
        }

        protected virtual void OnWFSGetCapabilitiesReceived(object source, List<WFSFeature> features)
        {
        }

        protected virtual void OnWFSFeatureDescriptorsReceived(object source, List<WFSFeatureDescriptor> featureDescriptors)
        {
        }

        private void ONWFSFeatureDataReceived(object tileKey, List<WFSFeatureData> data)
        {
            ProcessWFSFeatureData((Vector2Int)tileKey, data);
        }

        protected virtual void ProcessWFSFeatureData(Vector2Int tileKey, List<WFSFeatureData> data)
        {
            print("Received " + data.Count + " features for tile " + tileKey);
        }

        private static BoundingBox GetBoundingBox(Tile tile)
        {
            var minX = tile.tileKey.x;
            var minY = tile.tileKey.y;
            var maxX = tile.tileKey.x + 1000;
            var maxY = tile.tileKey.y + 1000;

            return new BoundingBox(minX, minY, maxX, maxY);
        }
    }
}
