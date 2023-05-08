using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.TileSystem;
using System;
using Netherlands3D.Core;
using UnityEngine.Networking;
using UnityEngine.Rendering.Universal;

namespace Netherlands3D.Geoservice
{
    public class WMSImageLayer : Layer
    {    
        public bool compressLoadedTextures = false;
        private bool prefabIsDecalProjector = false;
        private GameObject tilePrefab;
        public GameObject TilePrefab {
            get
            {
                return tilePrefab;
            }
            set
            {
                tilePrefab = value;
                prefabIsDecalProjector = tilePrefab.GetComponent<DecalProjector>();
            }
        }

        public override void HandleTile(TileChange tileChange, Action<TileChange> callback = null)
        {
            var tileKey = new Vector2Int(tileChange.X, tileChange.Y);

            if (tileChange.action== TileAction.Create)
            {
                Tile newTile = CreateNewTile(tileKey);
                tiles.Add(tileKey, newTile);
                newTile.gameObject.SetActive(false);
                //retrieve the image and put it on the tile
                tiles[tileKey].runningCoroutine = StartCoroutine(DownloadTexture(tileChange, callback));
            }
            if (tileChange.action == TileAction.Upgrade)
            {
                tiles[tileKey].unityLOD++;
                tiles[tileKey].runningCoroutine = StartCoroutine(DownloadTexture(tileChange, callback));
            }
            if (tileChange.action == TileAction.Downgrade)
            {
                tiles[tileKey].unityLOD--;
                tiles[tileKey].runningCoroutine = StartCoroutine(DownloadTexture(tileChange, callback));
            }

            if (tileChange.action == TileAction.Remove)
            {
                InteruptRunningProcesses(tileKey);
                RemoveGameObjectFromTile(tileKey);
                tiles.Remove(tileKey);
                callback(tileChange);
                return;
            }
            
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
                //destroy the image
                Texture tex= tile.gameObject.GetComponent<MeshRenderer>().material.GetTexture("_MainTex");
                tile.gameObject.GetComponent<MeshRenderer>().material.SetTexture("_MainTex",null);
                Destroy(tex);

                //destroy the gameobject
                Destroy(tiles[tileKey].gameObject);
            }
        }

        private Tile CreateNewTile(Vector2Int tileKey)
        {
            Tile tile = new Tile();
            tile.unityLOD = 0;
            tile.tileKey = tileKey;
            tile.layer = transform.gameObject.GetComponent<Layer>();
            tile.gameObject = Instantiate(TilePrefab);
            tile.gameObject.name = tileKey.x + "-" + tileKey.y;
            tile.gameObject.transform.parent = transform.gameObject.transform;
            tile.gameObject.layer = tile.gameObject.transform.parent.gameObject.layer;
            Vector2Int origin = new Vector2Int(tileKey.x+(tileSize/2), tileKey.y + (tileSize / 2));
            tile.gameObject.transform.position = CoordConvert.RDtoUnity(origin);
            
            return tile;
        }

        IEnumerator DownloadTexture(TileChange tileChange, Action<TileChange> callback = null)
        {
            var tileKey = new Vector2Int(tileChange.X, tileChange.Y);
            string url = $"{Datasets[tiles[tileKey].unityLOD].path}?Xmin={tileChange.X}&Ymin={tileChange.Y}&Xmax={tileChange.X + tileSize}&Ymax={tileChange.Y + tileSize}";

            var webRequest = UnityWebRequestTexture.GetTexture(url, compressLoadedTextures == false);
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
                Tile tile = tiles[tileKey];
                ClearPreviousTexture(tile);

                Texture2D myTexture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;
                if (compressLoadedTextures) myTexture.Compress(false);
                myTexture.wrapMode = TextureWrapMode.Clamp;

                ApplyTextureToMaterial(myTexture, tile);

                callback(tileChange);
            }
            yield return null;
        }

        /// <summary>
        /// Clear existing texture from tile projector
        /// </summary>
        private void ClearPreviousTexture(Tile tile)
        {
            //remove old Texture if it exists
            Texture oldTexture;
            if (prefabIsDecalProjector)
            {
                oldTexture = tile.gameObject.GetComponent<DecalProjector>().material.mainTexture;
            }
            else
            {
                oldTexture = tile.gameObject.GetComponent<MeshRenderer>().material.mainTexture;
            }
            if (oldTexture != null)
            {
                Destroy(oldTexture);
            }
        }

        private void ApplyTextureToMaterial(Texture2D myTexture, Tile tile)
        {
            Material material;
            if (prefabIsDecalProjector)
            {
                material = tile.gameObject.GetComponent<DecalProjector>().material;
            }
            else
            {
                material = tile.gameObject.GetComponent<MeshRenderer>().material;
            }

            material.mainTexture = myTexture;
            tile.gameObject.SetActive(true);
        }
    }
}
