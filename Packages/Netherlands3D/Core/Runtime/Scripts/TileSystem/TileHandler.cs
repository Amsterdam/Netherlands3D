/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/3DAmsterdam/blob/master/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Core;
using System.Linq;
using Netherlands3D.Coordinates;
using UnityEngine.Networking;
using UnityEditor;
using Netherlands3D.Events;

namespace Netherlands3D.TileSystem
{
    public class TileHandler : MonoBehaviour
    {
        /// <summary>
        /// if true, prevents all layers from updating tiles
        /// downloading data continues if already started
        /// </summary>
        public bool pauseLoading
        {
            set
            {
                foreach (Layer layer in layers)
                {
                    layer.pauseLoading = value;
                }
            }
        }
        public int maximumConcurrentDownloads = 6;

        [SerializeField]
        private bool filterByCameraFrustum = true;

        [HideInInspector]
        public List<Layer> layers = new List<Layer>();


        private List<int> tileSizes = new List<int>();
        /// <summary>
        /// contains, for each tilesize in tileSizes, al list with tilecoordinates an distance to camera
        /// X,Y is bottom-left coordinate of tile in RD (for example 121000,480000)
        /// Z is distance-squared to camera in m
        /// </summary>
        private List<List<Vector3Int>> tileDistances = new List<List<Vector3Int>>();
        private List<Vector3Int> tileList = new List<Vector3Int>();
        /// <summary>
        /// list of tilechanges, ready to be processed
        /// </summary>
        [HideInInspector]
        public List<TileChange> pendingTileChanges = new List<TileChange>();

        /// <summary>
        /// dictionary with tilechanges that are curently being processed
        /// Key:
        ///		X,Y is bottom-left coordinate of tile in RD (for example 121000,480000)
        ///		Z is the Layerindex of the tile
        /// </summary>
        private Dictionary<Vector3Int, TileChange> activeTileChanges = new Dictionary<Vector3Int, TileChange>();

        /// <summary>
        /// area that is visible
        /// X, Y is bottom-left coordinate in RD (for example 121000,480000)
        /// Z width of area(RD-X-direction) in M
        /// W length of area(RD-Y-direction) in M
        /// </summary>
        private Vector4 viewRange = new Vector4();

        /// <summary>
        /// postion of camera in RDcoordinates rounded to nearest integer
        /// </summary>
        private Vector3Int cameraPosition;

        /// <summary>
        /// The method to use to determine what LOD should be showed.
        /// Auto is the default, using distance from camera and LOD distances
        /// </summary>
        private LODCalculationMethod lodCalculationMethod = LODCalculationMethod.Auto;
        private float maxDistanceMultiplier = 1.0f;

        private Vector2Int tileKey;
        private Bounds tileBounds;
        private Plane[] cameraFrustumPlanes;
        private int startX;
        private int startY;
        private int endX;
        private int endY;

        public static int runningTileDataRequests = 0;

        private bool useRadialDistanceCheck = false; //Nicer for FPS cameras

        private int maxTileSize = 0;

        private float groundLevelClipRange = 1000;

        [Header("Optional events")]
        [SerializeField]
        private Vector2IntEvent tileCreatedEvent;
        [SerializeField]
        private Vector2IntEvent tileUpgradeEvent;
        [SerializeField]
        private Vector2IntEvent tileDowngradeEvent;
        [SerializeField]
        private Vector2IntEvent tileDestroyedEvent;

        void Start()
        {
            layers = GetComponentsInChildren<Layer>(false).ToList();
            if (layers.Count == 0)
            {
                Debug.Log("No active layers found in TileHandler", this.gameObject);
            }

            pauseLoading = false;
            CacheCameraFrustum();

            if (!Camera.main)
            {
                Debug.LogWarning("The TileHandler requires a camera. Make sure your scene has a camera, and it is tagged as MainCamera.");
                this.enabled = false;
            }

            if (tileSizes.Count == 0)
            {
                GetTilesizes();
            }
        }

        public void AddLayer(Layer layer)
        {
            layers.Add(layer);
            GetTilesizes();
        }
        public void RemoveLayer(Layer layer)
        {

            int layerIndex = layers.IndexOf(layer);


            // add all existing tiles to pending destroy
            int tilesizeIndex = tileSizes.IndexOf(layer.tileSize);
            foreach (Vector3Int tileDistance in tileDistances[tilesizeIndex])
            {
                tileKey = new Vector2Int(tileDistance.x, tileDistance.y);

                if (layer.tiles.ContainsKey(tileKey))
                {
                    TileChange tileChange = new TileChange();
                    tileChange.action = TileAction.Remove;
                    tileChange.X = tileKey.x;
                    tileChange.Y = tileKey.y;
                    tileChange.layerIndex = layerIndex;
                    tileChange.priorityScore = CalculatePriorityScore(layer.layerPriority, 0, tileDistance.z, TileAction.Remove);
                    AddTileChange(tileChange, layerIndex);

                }
            }
            InstantlyStartRemoveChanges();
            layers.Remove(layer);


        }

        private void CacheCameraFrustum()
        {
            tileBounds = new Bounds();
            cameraFrustumPlanes = new Plane[6]
            {
                new Plane(), //Left
				new Plane(), //Right
				new Plane(), //Down
				new Plane(), //Up
				new Plane(), //Near
				new Plane(), //Far
			};
        }

        void Update()
        {
            if (layers.Count == 0)
            {
                return;
            }
            viewRange = GetViewRange();
            cameraPosition = GetRDCameraPosition();

            GetTileDistancesInView(tileSizes, viewRange, cameraPosition);

            pendingTileChanges.Clear();
            RemoveOutOfViewTiles();
            GetTileChanges();

            if (pendingTileChanges.Count == 0) return;

            //Start with all remove changes to clear resources. We to all remove actions, and stop any running tilechanges that share the same position and layerindex
            InstantlyStartRemoveChanges();

            if (activeTileChanges.Count < maximumConcurrentDownloads && pendingTileChanges.Count > 0)
            {
                TileChange highestPriorityTileChange = GetHighestPriorityTileChange();
                Vector3Int tilekey = new Vector3Int(highestPriorityTileChange.X, highestPriorityTileChange.Y, highestPriorityTileChange.layerIndex);
                if (activeTileChanges.ContainsKey(tilekey) == false)
                {
                    activeTileChanges.Add(tilekey, highestPriorityTileChange);
                    pendingTileChanges.Remove(highestPriorityTileChange);
                    layers[highestPriorityTileChange.layerIndex].HandleTile(highestPriorityTileChange, TileHandled);
                }
                else if (activeTileChanges.TryGetValue(tilekey, out TileChange existingTileChange))
                {
                    //Change running tile changes to more important ones
                    Debug.Log("Upgrading existing");
                    if (existingTileChange.priorityScore < highestPriorityTileChange.priorityScore)
                    {
                        activeTileChanges[tilekey] = highestPriorityTileChange;
                        pendingTileChanges.Remove(highestPriorityTileChange);
                    }
                }
            }
        }

        private void InstantlyStartRemoveChanges()
        {
            var removeChanges = pendingTileChanges.Where(change => change.action == TileAction.Remove).ToArray();
            for (int i = removeChanges.Length - 1; i >= 0; i--)
            {
                var removeChange = removeChanges[i];
                layers[removeChange.layerIndex].HandleTile(removeChange, TileRemoved);
                pendingTileChanges.Remove(removeChange);

                //Abort all tilechanges with the same key
                AbortSimilarTileChanges(removeChange);
                AbortPendingSimilarTileChanges(removeChange);
            }
        }

        private void AbortSimilarTileChanges(TileChange removeChange)
        {
            var changes = activeTileChanges.Where(change => ((change.Value.X == removeChange.X) && (change.Value.Y == removeChange.Y))).ToArray();
            for (int i = changes.Length - 1; i >= 0; i--)
            {
                var runningChange = changes[i];
                layers[removeChange.layerIndex].InteruptRunningProcesses(new Vector2Int(removeChange.X, removeChange.Y));
                layers[removeChange.layerIndex].HandleTile(removeChange, TileRemoved);
                activeTileChanges.Remove(runningChange.Key);
            }
        }

        private void AbortPendingSimilarTileChanges(TileChange removeChange)
        {
            var changes = pendingTileChanges.Where(change => ((change.X == removeChange.X) && (change.Y == removeChange.Y))).ToArray();
            for (int i = changes.Length - 1; i >= 0; i--)
            {
                var runningChange = changes[i];
                layers[removeChange.layerIndex].InteruptRunningProcesses(new Vector2Int(removeChange.X, removeChange.Y));
                layers[removeChange.layerIndex].HandleTile(removeChange, TileRemoved);
                pendingTileChanges.Remove(runningChange);
            }
        }

        public void TileHandled(TileChange handledTileChange)
        {
            InvokeTileChangeEvent(handledTileChange);

            activeTileChanges.Remove(new Vector3Int(handledTileChange.X, handledTileChange.Y, handledTileChange.layerIndex));
        }

        private void InvokeTileChangeEvent(TileChange handledTileChange)
        {
            switch (handledTileChange.action)
            {
                case TileAction.Create:
                    if (tileCreatedEvent)
                        tileCreatedEvent.InvokeStarted(new Vector2Int(handledTileChange.X, handledTileChange.Y));
                    break;
                case TileAction.Downgrade:
                    if (tileDowngradeEvent)
                        tileDowngradeEvent.InvokeStarted(new Vector2Int(handledTileChange.X, handledTileChange.Y));
                    break;
                case TileAction.Upgrade:
                    if (tileUpgradeEvent)
                        tileUpgradeEvent.InvokeStarted(new Vector2Int(handledTileChange.X, handledTileChange.Y));
                    break;
                case TileAction.Remove:
                    if (tileDestroyedEvent)
                        tileDestroyedEvent.InvokeStarted(new Vector2Int(handledTileChange.X, handledTileChange.Y));
                    break;
            }
        }

        public void TileRemoved(TileChange handledTileChange)
        {
            if (tileDestroyedEvent)
                tileDestroyedEvent.InvokeStarted(new Vector2Int(handledTileChange.X, handledTileChange.Y));
        }

        /// <summary>
        /// uses CameraExtent
        /// updates the variable viewrange
        /// updates the variable cameraPositionRD
        /// updates the variable cameraPosition
        /// </summary>
        private Vector4 GetViewRange()
        {
            Extent cameraExtent;
            if (Camera.main.transform.position.y > 20)
            {
                useRadialDistanceCheck = false;
                cameraExtent = Camera.main.GetRDExtent(Camera.main.farClipPlane + maxTileSize);
            }
            else
            {
                useRadialDistanceCheck = true;
                var cameraRD = CoordinateConverter.UnitytoRD(Camera.main.transform.position);
                cameraExtent = new Extent(
                    cameraRD.x - groundLevelClipRange,
                    cameraRD.y - groundLevelClipRange,
                    cameraRD.x + groundLevelClipRange,
                    cameraRD.y + groundLevelClipRange
                );
            }

            Vector4 viewRange = new Vector4();
            viewRange.x = (float)cameraExtent.MinX;
            viewRange.y = (float)cameraExtent.MinY;
            viewRange.z = (float)(cameraExtent.MaxX - cameraExtent.MinX);
            viewRange.w = (float)(cameraExtent.MaxY - cameraExtent.MinY);

            return viewRange;
        }

        private Vector3Int GetRDCameraPosition()
        {
            var cameraPositionRD = CoordinateConverter.UnitytoRD(Camera.main.transform.position);
            Vector3Int cameraPosition = new Vector3Int();
            cameraPosition.x = (int)cameraPositionRD.x;
            cameraPosition.y = (int)cameraPositionRD.y;
            cameraPosition.z = (int)cameraPositionRD.z;

            return cameraPosition;
        }

        /// <summary>
        /// create a list of unique tilesizes used by all the layers
        /// save the list in variable tileSizes
        /// </summary>
        private void GetTilesizes()
        {
            int tilesize;
            tileSizes = new List<int>();
            if (layers.Count == 0)
            {
                return;
            }
            foreach (Layer layer in layers)
            {
                if (layer.gameObject.activeInHierarchy == false)
                {
                    continue;
                }
                if (layer.isEnabled == true)
                {
                    tilesize = layer.tileSize;
                    if (tileSizes.Contains(tilesize) == false)
                    {
                        tileSizes.Add(tilesize);
                    }
                }
            }
            maxTileSize = tileSizes.Max();
        }

        private Vector3 GetPlaneIntersection(Plane plane, Camera camera, Vector2 screenCoordinate)
        {
            Ray ray = camera.ViewportPointToRay(screenCoordinate);
            Vector3 dirNorm = ray.direction / ray.direction.y;
            Vector3 IntersectionPos = ray.origin - dirNorm * ray.origin.y;
            return IntersectionPos;
        }

        private void GetTileDistancesInView(List<int> tileSizes, Vector4 viewRange, Vector3Int cameraPosition)
        {
            //Godview only frustum check
            if (filterByCameraFrustum && !useRadialDistanceCheck)
            {
                GeometryUtility.CalculateFrustumPlanes(Camera.main, cameraFrustumPlanes);
            }
            tileDistances.Clear();

            foreach (int tileSize in tileSizes)
            {
                startX = (int)Math.Floor(viewRange.x / tileSize) * tileSize;
                startY = (int)Math.Floor(viewRange.y / tileSize) * tileSize;
                endX = (int)Math.Ceiling((viewRange.x + viewRange.z) / tileSize) * tileSize;
                endY = (int)Math.Ceiling((viewRange.y + viewRange.w) / tileSize) * tileSize;
                tileList = new List<Vector3Int>();

                for (int x = startX; x <= endX; x += tileSize)
                {
                    for (int y = startY; y <= endY; y += tileSize)
                    {
                        Vector3Int tileID = new Vector3Int(x, y, tileSize);
                        if (filterByCameraFrustum && !useRadialDistanceCheck)
                        {
                            tileBounds.SetMinMax(CoordinateConverter.RDtoUnity(new Vector2(x, y)), CoordinateConverter.RDtoUnity(new Vector2(x + tileSize, y + tileSize)));
                            if (GeometryUtility.TestPlanesAABB(cameraFrustumPlanes, tileBounds))
                            {
                                tileList.Add(new Vector3Int(x, y, (int)GetTileDistanceSquared(tileID, cameraPosition)));
                            }
                        }
                        else
                        {
                            tileList.Add(new Vector3Int(x, y, (int)GetTileDistanceSquared(tileID, cameraPosition)));
                        }
                    }
                }

                tileDistances.Add(tileList);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            foreach (var tileList in tileDistances)
            {
                foreach (var tile in tileList)
                {
                    Gizmos.DrawWireCube(CoordinateConverter.RDtoUnity(new Vector3(tile.x + 500, tile.y + 500, 0)), new Vector3(1000, 100, 1000));
                }
            }
        }

        private float GetTileDistanceSquared(Vector3Int tileID, Vector3Int cameraPosition)
        {
            float distance = 0;
            int centerOffset = (int)tileID.z / 2;
            Vector3Int center = new Vector3Int(tileID.x + centerOffset, tileID.y + centerOffset, 0);
            float delta = center.x - cameraPosition.x;
            distance += (delta * delta);
            delta = center.y - cameraPosition.y;
            distance += (delta * delta);
            delta = cameraPosition.z * cameraPosition.z;
            distance += (delta);

            return distance;
        }


        private void GetTileChanges()
        {
            for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                Layer layer = layers[layerIndex];
                if (layer.isEnabled == false) { continue; }
                int tilesizeIndex = tileSizes.IndexOf(layer.tileSize);
                foreach (Vector3Int tileDistance in tileDistances[tilesizeIndex])
                {
                    tileKey = new Vector2Int(tileDistance.x, tileDistance.y);
                    int LOD = CalculateUnityLOD(tileDistance, layer);
                    if (layer.tiles.ContainsKey(tileKey))
                    {
                        int activeLOD = layer.tiles[tileKey].unityLOD;
                        if (LOD == -1)
                        {
                            TileChange tileChange = new TileChange();
                            tileChange.action = TileAction.Remove;
                            tileChange.X = tileKey.x;
                            tileChange.Y = tileKey.y;
                            tileChange.layerIndex = layerIndex;
                            tileChange.priorityScore = CalculatePriorityScore(layer.layerPriority, 0, tileDistance.z, TileAction.Remove);
                            AddTileChange(tileChange, layerIndex);
                        }
                        else if (activeLOD > LOD)
                        {
                            TileChange tileChange = new TileChange();
                            tileChange.action = TileAction.Downgrade;
                            tileChange.X = tileKey.x;
                            tileChange.Y = tileKey.y;
                            tileChange.layerIndex = layerIndex;
                            tileChange.priorityScore = CalculatePriorityScore(layer.layerPriority, activeLOD - 1, tileDistance.z, TileAction.Downgrade);
                            AddTileChange(tileChange, layerIndex);
                        }
                        else if (activeLOD < LOD)
                        {
                            TileChange tileChange = new TileChange();
                            tileChange.action = TileAction.Upgrade;
                            tileChange.X = tileKey.x;
                            tileChange.Y = tileKey.y;
                            tileChange.layerIndex = layerIndex;
                            tileChange.priorityScore = CalculatePriorityScore(layer.layerPriority, activeLOD + 1, tileDistance.z, TileAction.Upgrade);
                            AddTileChange(tileChange, layerIndex);
                        }
                    }
                    else
                    {
                        if (LOD != -1)
                        {
                            TileChange tileChange = new TileChange();
                            tileChange.action = TileAction.Create;
                            tileChange.X = tileKey.x;
                            tileChange.Y = tileKey.y;
                            tileChange.priorityScore = CalculatePriorityScore(layer.layerPriority, 0, tileDistance.z, TileAction.Create);
                            tileChange.layerIndex = layerIndex;
                            AddTileChange(tileChange, layerIndex);
                        }
                    }
                }
            }
        }

        private void AddTileChange(TileChange tileChange, int layerIndex)
        {

            //don't add a tilechange if the tile has an active tilechange already

            Vector3Int activekey = new Vector3Int(tileChange.X, tileChange.Y, tileChange.layerIndex);
            if (activeTileChanges.ContainsKey(activekey) && tileChange.action != TileAction.Remove)
            {
                return;
            }
            bool tileIspending = false;
            for (int i = pendingTileChanges.Count - 1; i >= 0; i--)
            {
                if (pendingTileChanges[i].X == tileChange.X && pendingTileChanges[i].Y == tileChange.Y && pendingTileChanges[i].layerIndex == tileChange.layerIndex)
                {
                    tileIspending = true;
                }
            }

            //Replace running tile changes with this one if priority is higher
            if (tileIspending == false)
            {
                pendingTileChanges.Add(tileChange);
            }
        }

        private int CalculateUnityLOD(Vector3Int tiledistance, Layer layer)
        {
            int unityLod = -1;

            foreach (DataSet dataSet in layer.Datasets)
            {
                //Are we within distance
                if (dataSet.enabled && dataSet.maximumDistanceSquared * maxDistanceMultiplier > (tiledistance.z))
                {
                    if (lodCalculationMethod == LODCalculationMethod.Lod1)
                    {
                        return (layer.Datasets.Count > 2) ? 1 : 0;
                    }
                    else if (lodCalculationMethod == LODCalculationMethod.Lod2)
                    {
                        //Just use the dataset length for now (we currently have 3 LOD steps)
                        return layer.Datasets.Count - 1;
                    }
                    else
                    {
                        unityLod = layer.Datasets.IndexOf(dataSet);
                    }
                }
            }
            return unityLod;
        }

        /// <summary>
        /// Switch the LOD calculaton mode
        /// </summary>
        /// <param name="method">0=Auto, 1=Lod1, 2=Lod2</param>
        public void SetLODMode(int method = 0)
        {
            lodCalculationMethod = (LODCalculationMethod)method;
        }

        /// <summary>
        /// Set the multiplier to use to limit tile distances
        /// </summary>
        /// <param name="multiplier">Multiplier value</param>
        public void SetMaxDistanceMultiplier(float multiplier)
        {
            maxDistanceMultiplier = multiplier;
        }

        private int CalculatePriorityScore(int layerPriority, int lod, int distanceSquared, TileAction action)
        {
            float distanceFactor = ((5000f * 5000f) / distanceSquared);
            int priority = 1;
            switch (action)
            {
                case TileAction.Create:
                    priority = (int)((1 + (10 * (lod + layerPriority))) * distanceFactor);
                    break;
                case TileAction.Upgrade:
                    priority = (int)((1 + (1 * (lod + layerPriority))) * distanceFactor);
                    break;
                case TileAction.Downgrade:
                    priority = (int)((1 + (0.5 * (lod + layerPriority))) * distanceFactor);
                    break;
                case TileAction.Remove:
                    priority = int.MaxValue;
                    break;
                default:
                    break;
            }
            return priority;
        }

        Layer layer;
        List<Vector3Int> neededTiles;
        List<Vector2Int> neededTileKeys = new List<Vector2Int>();
        TileChange tileChange;

        private void RemoveOutOfViewTiles()
        {
            for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                // create a list of tilekeys for the tiles that are within the viewrange
                layer = layers[layerIndex];
                if (layer == null)
                {
                    continue;
                }
                if (layer.gameObject.activeSelf == false) { continue; }
                if (layer.isEnabled == false)
                {
                    continue;
                }
                int tilesizeIndex = tileSizes.IndexOf(layer.tileSize);
                neededTiles = tileDistances[tilesizeIndex];
                neededTileKeys.Clear();
                neededTileKeys.Capacity = neededTiles.Count;
                foreach (var neededTile in neededTiles)
                {
                    //tileKey.x = neededTile.x;
                    //tileKey.y = neededTile.y;
                    neededTileKeys.Add(new Vector2Int(neededTile.x, neededTile.y));
                }
                //activeTiles = layer.tiles.Keys.ToArray();
                //activeTiles = new List<Vector2Int>(layer.tiles.Keys);
                // check for each active tile if the key is in the list of tilekeys within the viewrange
                foreach (var kvp in layer.tiles)
                {
                    if (neededTileKeys.Contains(kvp.Key) == false) // if the tile is not within the viewrange, set it up for removal
                    {
                        tileChange = new TileChange();
                        tileChange.action = TileAction.Remove;
                        tileChange.X = kvp.Key.x;
                        tileChange.Y = kvp.Key.y;
                        tileChange.layerIndex = layerIndex;
                        tileChange.priorityScore = int.MaxValue; // set the priorityscore to maximum
                        AddTileChange(tileChange, layerIndex);
                    }
                }

            }
        }
        private TileChange GetHighestPriorityTileChange()
        {
            TileChange highestPriorityTileChange = pendingTileChanges[0];
            float highestPriority = highestPriorityTileChange.priorityScore;

            for (int i = 1; i < pendingTileChanges.Count; i++)
            {
                if (pendingTileChanges[i].priorityScore > highestPriority)
                {
                    highestPriorityTileChange = pendingTileChanges[i];
                    highestPriority = highestPriorityTileChange.priorityScore;
                }
            }
            return highestPriorityTileChange;
        }
    }
    [Serializable]
    public class DataSet
    {
        public string Description;
        public string geoLOD;
        public string path;
        public string pathQuery;
        public float maximumDistance;
        [HideInInspector]
        public float maximumDistanceSquared;
        public bool enabled = true;


        public string url
        {
            get
            {
                return path + pathQuery;
            }
        }
    }

    public class Tile
    {
        public Layer layer;
        public int unityLOD;
        public GameObject gameObject;
        public AssetBundle assetBundle;
        public Vector2Int tileKey;
        public UnityWebRequest runningWebRequest;
        public Coroutine runningCoroutine;
    }
    [Serializable]
    public struct TileChange : IEquatable<TileChange>
    {

        public TileAction action;
        public int priorityScore;
        public int layerIndex;
        public int X;
        public int Y;

        public bool Equals(TileChange other)
        {
            return (X == other.X && Y == other.Y && layerIndex == other.layerIndex);
        }

    }
    public enum TileAction
    {
        Create,
        Upgrade,
        Downgrade,
        Remove
    }

    [Serializable]
    public enum LODCalculationMethod
    {
        Auto,
        Lod1,
        Lod2
    }
}
