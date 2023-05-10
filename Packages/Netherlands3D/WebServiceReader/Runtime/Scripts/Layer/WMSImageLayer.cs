/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/Netherlands3D/blob/main/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/

using System.Collections;
using UnityEngine;
using Netherlands3D.TileSystem;
using System;
using Netherlands3D.Core;
using UnityEngine.Networking;

namespace Netherlands3D.Geoservice
{
    public class WMSImageLayer : Layer
    {    
        public bool compressLoadedTextures = false;

        private TextureProjectorBase projectorPrefab;
        public TextureProjectorBase ProjectorPrefab { get => projectorPrefab; set => projectorPrefab = value; }

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

                //destroy the gameobject
                Destroy(tile.gameObject);
            }
        }

        private Tile CreateNewTile(Vector2Int tileKey)
        {
            Tile tile = new();
            tile.unityLOD = 0;
            tile.tileKey = tileKey;
            tile.layer = transform.gameObject.GetComponent<Layer>();
            tile.gameObject = Instantiate(ProjectorPrefab.gameObject);
            tile.gameObject.name = tileKey.x + "-" + tileKey.y;
            tile.gameObject.transform.parent = transform.gameObject.transform;
            tile.gameObject.layer = tile.gameObject.transform.parent.gameObject.layer;
            Vector2Int origin = new Vector2Int(tileKey.x+(tileSize/2), tileKey.y + (tileSize / 2));
            tile.gameObject.transform.position = CoordConvert.RDtoUnity(origin);

            if (tile.gameObject.TryGetComponent<TextureProjectorBase>(out var projector))
            {
                projector.SetSize(tileSize, tileSize, tileSize);
            }

            return tile;
        }

        IEnumerator DownloadTexture(TileChange tileChange, Action<TileChange> callback = null)
        {
            var tileKey = new Vector2Int(tileChange.X, tileChange.Y);

            if (!tiles.ContainsKey(tileKey))
            {
                Debug.Log("Tile key does not exist", this.gameObject);
                yield break;
            }

            Tile tile = tiles[tileKey];

            string url = Datasets[tiles[tileKey].unityLOD].path;
            url = url.Replace("{Xmin}", tileChange.X.ToString());
            url = url.Replace("{Ymin}", tileChange.Y.ToString()); ;
            url = url.Replace("{Xmax}", (tileChange.X + tileSize).ToString());
            url = url.Replace("{Ymax}", (tileChange.Y + tileSize).ToString());

            var webRequest = UnityWebRequestTexture.GetTexture(url, compressLoadedTextures == false);
            tile.runningWebRequest = webRequest;
            yield return webRequest.SendWebRequest();

            tile.runningWebRequest = null;

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                RemoveGameObjectFromTile(tileKey);
                callback(tileChange);
            }
            else
            {
                ClearPreviousTexture(tile);

                Texture2D myTexture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;
                if (compressLoadedTextures) myTexture.Compress(false);

                myTexture.wrapMode = TextureWrapMode.Clamp;

                SetProjectorTexture(tile,myTexture);

                callback(tileChange);
            }
            yield return null;
        }

        /// <summary>
        /// Clear existing texture from tile projector
        /// </summary>
        private void ClearPreviousTexture(Tile tile)
        {
            if (tile.gameObject.TryGetComponent<TextureProjectorBase>(out var projector))
            {
                projector.ClearTexture();
            }
        }

        private void SetProjectorTexture(Tile tile, Texture2D myTexture)
        {
            if (tile.gameObject.TryGetComponent<TextureProjectorBase>(out var projector))
            {
                projector.SetTexture(myTexture);
            }
            tile.gameObject.SetActive(true);
        }
    }
}
