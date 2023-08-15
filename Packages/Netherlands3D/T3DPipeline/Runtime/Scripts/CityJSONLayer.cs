using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Core;
using UnityEngine.Networking;
using System;
using System.Linq;
using Netherlands3D.Coordinates;
using UnityEngine.Rendering;
using Netherlands3D.T3DPipeline;
using Netherlands3D.Events;

namespace Netherlands3D.TileSystem
{
    public class CityJSONLayer : Layer //todo: put in NL3D Package
    {
        public ShadowCastingMode tileShadowCastingMode = ShadowCastingMode.On;

        [SerializeField]
        private GameObject containerPrefab;
        private GameObject container;

        [SerializeField]
        private bool checkIfFileExistsBeforeDownloading;

        public override void HandleTile(TileChange tileChange, System.Action<TileChange> callback = null)
        {
            TileAction action = tileChange.action;
            var tileKey = new Vector2Int(tileChange.X, tileChange.Y);
            switch (action)
            {
                case TileAction.Create:
                    Tile newTile = CreateNewTile(tileKey);
                    tiles.Add(tileKey, newTile);
                    break;
                case TileAction.Upgrade:
                    tiles[tileKey].unityLOD++;
                    break;
                case TileAction.Downgrade:
                    tiles[tileKey].unityLOD--;
                    break;
                case TileAction.Remove:
                    InteruptRunningProcesses(tileKey);
                    RemoveGameObjectFromTile(tileKey);
                    tiles.Remove(tileKey);
                    callback(tileChange);
                    return;
                default:
                    break;
            }
            if (checkIfFileExistsBeforeDownloading)
                tiles[tileKey].runningCoroutine = StartCoroutine(CheckAndDownloadCityJSON(tileChange, callback));
            else
                tiles[tileKey].runningCoroutine = StartCoroutine(DownloadCityJSON(tileChange, callback));
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

            return tile;
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

        private IEnumerator CheckAndDownloadCityJSON(TileChange tileChange, System.Action<TileChange> callback = null)
        {
            var tileKey = new Vector2Int(tileChange.X, tileChange.Y);
            int index = tiles[tileKey].unityLOD;
            string url = Datasets[index].path;
            if (Datasets[index].path.StartsWith("https://") || Datasets[index].path.StartsWith("file://"))
            {
                url = Datasets[index].url;
            }

            url = url.ReplaceXY(tileChange.X, tileChange.Y);

            var webRequest = UnityWebRequest.Head(url);
            tiles[tileKey].runningWebRequest = webRequest;
            yield return webRequest.SendWebRequest();

            if (!tiles.ContainsKey(tileKey)) yield break;
            tiles[tileKey].runningWebRequest = null;

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                //file does not exist
                yield break;
            }
            else
            {
                //file exists, proceed with normal download
                yield return DownloadCityJSON(tileChange, callback);
            }
        }

        private IEnumerator DownloadCityJSON(TileChange tileChange, System.Action<TileChange> callback = null)
        {
            var tileKey = new Vector2Int(tileChange.X, tileChange.Y);
            int index = tiles[tileKey].unityLOD;
            string url = Datasets[index].path;
            if (Datasets[index].path.StartsWith("https://") || Datasets[index].path.StartsWith("file://"))
            {
                url = Datasets[index].url;
            }

            url = url.ReplaceXY(tileChange.X, tileChange.Y);

            var webRequest = UnityWebRequest.Get(url);

            tiles[tileKey].runningWebRequest = webRequest;
            yield return webRequest.SendWebRequest();

            if (!tiles.ContainsKey(tileKey)) yield break;

            tiles[tileKey].runningWebRequest = null;

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                RemoveGameObjectFromTile(tileKey);
                callback(tileChange);
            }
            else
            {
                string jsonResult = webRequest.downloadHandler.text;

                yield return new WaitUntil(() => pauseLoading == false);
                GameObject newGameobject = CreateNewGameObject(url, jsonResult, tileChange);
                if (newGameobject != null)
                {
                    if (TileHasSubObjectAltered(tileChange))
                    {
                        yield return SyncSubObjects(tileChange, newGameobject, callback);
                    }
                    else
                    {
                        RemoveGameObjectFromTile(tileKey);
                        tiles[tileKey].gameObject = newGameobject;
                        callback(tileChange);
                    }
                }
                else
                {
                    callback(tileChange);
                }
            }
        }

        public void EnableShadows(bool enabled)
        {
            tileShadowCastingMode = (enabled) ? ShadowCastingMode.On : ShadowCastingMode.Off;

            MeshRenderer[] existingTiles = GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in existingTiles)
            {
                renderer.shadowCastingMode = tileShadowCastingMode;
            }
        }

        private bool TileHasSubObjectAltered(TileChange tileChange)
        {
            Tile tile = tiles[new Vector2Int(tileChange.X, tileChange.Y)];
            if (tile.gameObject == null)
            {
                return false;
            }

            var subObjects = tile.gameObject.GetComponent<SubObjects>();
            if (subObjects == null)
            {
                return false;
            }
            if (subObjects.Altered == false)
            {
                return false;
            }

            return true;
        }

        private IEnumerator SyncSubObjects(TileChange tileChange, GameObject newGameobject, System.Action<TileChange> callback = null)
        {
            Tile tile = tiles[new Vector2Int(tileChange.X, tileChange.Y)];
            SubObjects oldObjectMapping = tile.gameObject.GetComponent<SubObjects>();
            SubObjects newObjectMapping = newGameobject.AddComponent<SubObjects>();

            yield return newObjectMapping.LoadMetaDataAndApply(oldObjectMapping.SubObjectsData);

            yield return new WaitUntil(() => pauseLoading == false);
            RemoveGameObjectFromTile(tile.tileKey);
            tiles[tile.tileKey].gameObject = newGameobject;

            yield return null;
            callback(tileChange);
        }

        private GameObject CreateNewGameObject(string source, string cityJSON, TileChange tileChange)
        {
            container = Instantiate(containerPrefab, transform);
            container.name = tileChange.X.ToString() + "-" + tileChange.Y.ToString();
            container.layer = container.transform.parent.gameObject.layer;
            container.transform.position = CoordinateConverter.RDtoUnity(new Vector2(tileChange.X + (tileSize / 2), tileChange.Y + (tileSize / 2)));

            container.SetActive(isEnabled);
            container.GetComponent<CityJSON>().ParseCityJSON(cityJSON);

            return container;
        }
    }
}
