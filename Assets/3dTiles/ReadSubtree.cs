using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using subtree;
using System.IO;

public class ReadSubtree : MonoBehaviour
{
    public subtree.Subtree subtree;
     Tile tile;
    ImplicitTilingSettings settings;
    public string subtreeUrl = "https://storage.googleapis.com/ahp-research/maquette/kadaster/3dbasisvoorziening/test/landuse_1_1/subtrees/0_0_0.subtree";
    System.Action<Tile> sendResult;

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(downloadSubtree());
    }

    public void DownloadSubtree(string url, ImplicitTilingSettings tilingSettings, System.Action<Tile> callback)
    {
        settings = tilingSettings;
        sendResult = callback;
        subtreeUrl = url;
        StartCoroutine(downloadSubtree());
    }

    IEnumerator downloadSubtree()
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

            // setup tootTile
            tile = new Tile();
            tile.X = 0;
            tile.Y = 0;
            tile.Z = 0;
            tile.geometricError = settings.geometricError;
            tile.hascontent = subtree.ContentAvailability[0];

            tile.boundingVolume = new BoundingVolume();
            tile.boundingVolume.boundingVolumeType = BoundingVolumeType.Region;
            tile.boundingVolume.values = settings.boundingRegion;

            AddChildren(tile, 0,0);

            sendResult( tile);

        }
    }

    public void AddChildren(Tile tile,int parentNortonIndex, int LevelStartIndex)
    {

        int localIndex = parentNortonIndex * 4;
        int levelstart = LevelStartIndex+ (int)Mathf.Pow(4, tile.X);
        int childOne = levelstart+localIndex;
        if (childOne>subtree.TileAvailability.Length)
        {
            return;
        }

        AddChild(tile, childOne, levelstart, 0);
        AddChild(tile, childOne, levelstart, 1);
        AddChild(tile, childOne, levelstart, 2);
        AddChild(tile, childOne, levelstart, 3);

    }


    private void AddChild(Tile parentTile, int baseNortonIndex, int LevelStartIndex, int childNumber)
    {
        if (subtree.TileAvailability[baseNortonIndex+childNumber])
        {
            Tile childTile = new Tile();
            childTile.X = parentTile.X + 1;
            childTile.Y = parentTile.Y * 2 + childNumber%2;
            childTile.Z = parentTile.Z * 2 ;
            if (childNumber>1)
            {
                childTile.Z += 1;
            }
            childTile.geometricError = tile.geometricError / 2f;
            childTile.hascontent = subtree.ContentAvailability[baseNortonIndex + childNumber];

            childTile.boundingVolume = new BoundingVolume();
            childTile.boundingVolume.boundingVolumeType = parentTile.boundingVolume.boundingVolumeType;
            childTile.boundingVolume.values = new double[parentTile.boundingVolume.values.Length];
            double lonMin = parentTile.boundingVolume.values[0];
            double lonMax = parentTile.boundingVolume.values[2];
            double lonMid = (lonMin+lonMax)/2f;

            double latMin = parentTile.boundingVolume.values[1];
            double latMax = parentTile.boundingVolume.values[3];
            double latMid = (latMin + latMax) / 2f;

            if (childNumber%2==0)
            {
                childTile.boundingVolume.values[0] = lonMin;
                childTile.boundingVolume.values[2] = lonMid;
            }
            else
            {
                childTile.boundingVolume.values[0] = lonMid;
                childTile.boundingVolume.values[2] = lonMax;
            }
            if (childNumber<2)
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
                AddChildren(childTile, baseNortonIndex + childNumber, LevelStartIndex);
            }
            parentTile.children.Add(childTile);
        }
    }
}
