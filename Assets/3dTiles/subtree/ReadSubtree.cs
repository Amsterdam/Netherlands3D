using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using subtree;
using System.IO;
using Netherlands3D.Core;

public class ReadSubtree : MonoBehaviour
{
    public subtree.Subtree subtree;
     Tile rootTile;
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
            rootTile = new Tile();
            rootTile.X = 0;
            rootTile.Y = 0;
            rootTile.Z = 0;
            rootTile.geometricError = settings.geometricError;
            rootTile.hascontent = subtree.ContentAvailability[0];

            rootTile.boundingVolume = new BoundingVolume();
            rootTile.boundingVolume.boundingVolumeType = BoundingVolumeType.Region;
            rootTile.boundingVolume.values = settings.boundingRegion;

            AddChildren(rootTile, 0,0);

            sendResult( rootTile);

        }
    }

    public void AddChildren(Tile tile,int parentNortonIndex, int LevelStartIndex)
    {

        int localIndex = parentNortonIndex * 4;
        int levelstart = LevelStartIndex+ (int)Mathf.Pow(4, tile.X);
        int childOne = levelstart+localIndex;
        //if (childOne>subtree.TileAvailability.Length)
        //{
        //    return;
        //}

        AddChild(tile, localIndex, levelstart, 0);
        AddChild(tile, localIndex, levelstart, 1);
        AddChild(tile, localIndex, levelstart, 2);
        AddChild(tile, localIndex, levelstart, 3);

    }


    private void AddChild(Tile parentTile, int localIndex, int LevelStartIndex, int childNumber)
    {
        if (subtree.TileAvailability[localIndex+LevelStartIndex+childNumber])
        {
            Tile childTile = new Tile();
            childTile.X = parentTile.X + 1;
            childTile.Y = parentTile.Y * 2 + childNumber%2;
            childTile.Z = parentTile.Z * 2 ;
            if (childNumber>1)
            {
                childTile.Z += 1;
            }
            childTile.geometricError = parentTile.geometricError / 2f;
            childTile.hascontent = subtree.ContentAvailability[localIndex+LevelStartIndex + childNumber];

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

            childTile.unityBounds = CalculateUnityBounds(childTile);
            if (childTile.X < settings.subtreeLevels - 1)
            {
                AddChildren(childTile, localIndex + childNumber, LevelStartIndex);
            }

            parentTile.children.Add(childTile);
        }
    }

    Bounds CalculateUnityBounds(Tile tile)
    {
        Bounds result = new Bounds();
        Vector3WGS bottomleft = new Vector3WGS();
        Vector3WGS topright = new Vector3WGS();

        bottomleft.lon = tile.boundingVolume.values[0] * 180 / System.Math.PI;
        bottomleft.lat = tile.boundingVolume.values[1] * 180 / System.Math.PI;
        bottomleft.h = tile.boundingVolume.values[4];

        topright.lon = tile.boundingVolume.values[2] * 180 / System.Math.PI;
        topright.lat = tile.boundingVolume.values[3] * 180 / System.Math.PI;
        topright.h = tile.boundingVolume.values[4];

        Vector3 bottomleftUnity = CoordConvert.WGS84toUnity(bottomleft);
        Vector3 toprightUnity = CoordConvert.WGS84toUnity(topright);

        float deltaX = (toprightUnity.x - bottomleftUnity.x)/2;
        float deltaY = (toprightUnity.y - bottomleftUnity.y) / 2;
        result.size = new Vector3(deltaX, deltaY,0 );


        return result;
    }
}
