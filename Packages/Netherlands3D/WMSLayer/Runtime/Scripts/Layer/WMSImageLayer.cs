using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.TileSystem;
using System;
using Netherlands3D.Core;
using UnityEngine.Networking;

namespace Netherlands3D.Geoservice
{
    public class WMSImageLayer : Layer
    {
        
        private int BuildingStencilID=0;
        private int TerrainStencilID = 0;
        [SerializeField]
        private int activeStencilID = 0;
        private int activeStencilMask = 255;

        public GameObject TilePrefab;
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
                tiles[tileKey].LOD++;
                tiles[tileKey].runningCoroutine = StartCoroutine(DownloadTexture(tileChange, callback));
            }
            if (tileChange.action == TileAction.Downgrade)
            {
                tiles[tileKey].LOD--;
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
                DestroyImmediate(tex,true);

                //destroy the gameobject
                Destroy(tiles[tileKey].gameObject);
            }
        }

        private Tile CreateNewTile(Vector2Int tileKey)
        {
            Tile tile = new Tile();
            tile.LOD = 0;
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
            string url = Datasets[tiles[tileKey].LOD].path;
            url = url.Replace("{Xmin}", tileChange.X.ToString());
            url = url.Replace("{Ymin}", tileChange.Y.ToString()); ;
            url = url.Replace("{Xmax}", (tileChange.X+tileSize).ToString());
            url = url.Replace("{Ymax}", (tileChange.Y + tileSize).ToString());
            
            var webRequest = UnityWebRequestTexture.GetTexture(url);
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
                //remove old Texture if it exists
                Texture OldTexture = tiles[tileKey].gameObject.GetComponent<MeshRenderer>().material.GetTexture("_MainTex");
                if (OldTexture!=null)
                {
                    DestroyImmediate(OldTexture,true);
                }

                Texture myTexture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture; ;
                Tile tile = tiles[tileKey];

                Material material = tile.gameObject.GetComponent<MeshRenderer>().material;
                material.SetTexture("_MainTex", myTexture);
                material.SetFloat("_StencilRef", activeStencilID);
                material.SetFloat("_ReadMask", activeStencilMask);
                tile.gameObject.SetActive(true);
                callback(tileChange);
            }

                yield return null;
        }

        public void ProjectOnBuildings(int buildingStencilID)
        {
            if (BuildingStencilID==0)
            {
                BuildingStencilID = buildingStencilID;
                
            }
            activeStencilMask = 255;
            activeStencilID = BuildingStencilID;
            setProjectorStencilSettings();

        }
        public void ProjectOnTerrain(int terrainStencilID)
        {
            if (TerrainStencilID == 0)
            {
                TerrainStencilID = terrainStencilID;

            }
            activeStencilMask = 255;
            activeStencilID = TerrainStencilID;
            setProjectorStencilSettings();

        }
        public void ProjectOnBoth(int buildingStencilID, int terrainStencilID)
        {
            if (BuildingStencilID == 0)
            {
                BuildingStencilID = buildingStencilID;
            }
            if (TerrainStencilID == 0)
            {
                TerrainStencilID = terrainStencilID;
            }


            int difference = BuildingStencilID ^ TerrainStencilID;
            //int difference = Mathf.Abs(BuildingStencilID - TerrainStencilID);
            //difference = (int)Mathf.Ceil(Mathf.Log(difference, 2));
            //difference = (int)Mathf.Pow(2, difference);
            activeStencilMask = 255 - difference;

            activeStencilID = BuildingStencilID;
            setProjectorStencilSettings();
        }

        private void setProjectorStencilSettings()
        {

            foreach (var tile in tiles)
            {
                tile.Value.gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_StencilRef", activeStencilID);
                tile.Value.gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_ReadMask", activeStencilMask);
            }
        }

    }
}
