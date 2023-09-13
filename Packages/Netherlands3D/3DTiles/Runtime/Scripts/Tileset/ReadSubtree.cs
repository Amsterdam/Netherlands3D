using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using subtree;
using System.IO;

namespace Netherlands3D.Tiles3D
{
    public class ReadSubtree : MonoBehaviour
    {
        public Subtree subtree;
        Tile tile;
        ImplicitTilingSettings settings;
        public string subtreeUrl;
        System.Action<Tile> sendResult;

        public void DownloadSubtree(string url, ImplicitTilingSettings tilingSettings, System.Action<Tile> callback)
        {
            settings = tilingSettings;
            sendResult = callback;
            subtreeUrl = url;
            StartCoroutine(DownloadSubtree());
        }

        IEnumerator DownloadSubtree()
        {
            UnityWebRequest www = UnityWebRequest.Get(subtreeUrl);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                byte[] subtreeData = www.downloadHandler.data;
                string tempFilePath = Path.Combine(Application.persistentDataPath, "st.subtree");
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
                File.WriteAllBytes(tempFilePath, subtreeData);
                using FileStream fileStream = File.Open(tempFilePath, FileMode.Open);
                using BinaryReader binaryReader = new(fileStream);

                subtree = SubtreeReader.ReadSubtree(binaryReader);

                // setup rootTile
                tile = new Tile();
                tile.X = 0;
                tile.Y = 0;
                tile.Z = 0;
                tile.geometricError = settings.geometricError;
                tile.hascontent = (subtree.ContentAvailabiltyConstant == 1) || (subtree.ContentAvailability != null && subtree.ContentAvailability[0]);

                tile.boundingVolume = new BoundingVolume
                {
                    boundingVolumeType = BoundingVolumeType.Region,
                    values = settings.boundingRegion
                };

                AddChildren(tile, 0, 0);

                sendResult(tile);
            }
        }

        public void AddChildren(Tile tile, int parentNortonIndex, int LevelStartIndex)
        {
            int localIndex = parentNortonIndex * 4;
            int levelstart = LevelStartIndex + (int)Mathf.Pow(4, tile.X);
            int childOne = levelstart + localIndex;

            AddChild(tile, localIndex, levelstart, 0);
            AddChild(tile, localIndex, levelstart, 1);
            AddChild(tile, localIndex, levelstart, 2);
            AddChild(tile, localIndex, levelstart, 3);

        }

        private void AddChild(Tile parentTile, int localIndex, int LevelStartIndex, int childNumber)
        {
            if (subtree.TileAvailabiltyConstant == 1 || subtree.TileAvailability[localIndex + LevelStartIndex + childNumber])
            {
                Tile childTile = new Tile();
                childTile.parent = parentTile;

                childTile.X = parentTile.X + 1;
                childTile.Y = parentTile.Y * 2 + childNumber % 2;
                childTile.Z = parentTile.Z * 2;
                if (childNumber > 1)
                {
                    childTile.Z += 1;
                }
                childTile.geometricError = parentTile.geometricError / 2f;

                childTile.hascontent = (subtree.ContentAvailabiltyConstant == 1) || (subtree.ContentAvailability != null && subtree.ContentAvailability[localIndex + LevelStartIndex + childNumber]);

                childTile.boundingVolume = new BoundingVolume();
                childTile.boundingVolume.boundingVolumeType = parentTile.boundingVolume.boundingVolumeType;
                childTile.boundingVolume.values = new double[parentTile.boundingVolume.values.Length];
                double lonMin = parentTile.boundingVolume.values[0];
                double lonMax = parentTile.boundingVolume.values[2];
                double lonMid = (lonMin + lonMax) / 2f;

                double latMin = parentTile.boundingVolume.values[1];
                double latMax = parentTile.boundingVolume.values[3];
                double latMid = (latMin + latMax) / 2f;

                if (childNumber % 2 == 0)
                {
                    childTile.boundingVolume.values[0] = lonMin;
                    childTile.boundingVolume.values[2] = lonMid;
                }
                else
                {
                    childTile.boundingVolume.values[0] = lonMid;
                    childTile.boundingVolume.values[2] = lonMax;
                }
                if (childNumber < 2)
                {
                    childTile.boundingVolume.values[1] = latMin;
                    childTile.boundingVolume.values[3] = latMid;
                }
                else
                {
                    childTile.boundingVolume.values[1] = latMid;
                    childTile.boundingVolume.values[3] = latMax;
                }
                childTile.boundingVolume.values[4] = parentTile.boundingVolume.values[4];
                childTile.boundingVolume.values[5] = parentTile.boundingVolume.values[5];

                if (childTile.X < settings.subtreeLevels - 1)
                {
                    AddChildren(childTile, localIndex + childNumber, LevelStartIndex);
                }
                parentTile.children.Add(childTile);
            }
        }
    }
}
