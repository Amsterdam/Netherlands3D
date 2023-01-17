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
    string subtreeUrl = "https://storage.googleapis.com/ahp-research/maquette/kadaster/3dbasisvoorziening/test/landuse_1_1/subtrees/0_0_0.subtree";
    System.Action<Tile> sendResult;

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(downloadSubtree());
    }

    public void DownloadSubtree(string url, System.Action<Tile> callback)
    {
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
            File.WriteAllBytes(tempFilePath, subtreeData);
            using FileStream fileStream = File.Open(tempFilePath, FileMode.Open);
            using BinaryReader binaryReader = new(fileStream);

            subtree = SubtreeReader.ReadSubtree(binaryReader);

            
            tile = new Tile();
            tile.X = 0;
            tile.Y = 0;
            tile.Z = 0;

            AddChildren(tile, 0,0);

            sendResult( tile);

        }
    }

    public void AddChildren(Tile tile,int parentNortonIndex, int LevelStartIndex)
    {

        int localIndex = parentNortonIndex * 4;
        int levelstart = LevelStartIndex+ (int)Mathf.Pow(4, tile.X);
        int childOne = levelstart+localIndex;
        Debug.Log(childOne);
        if (subtree.TileAvailability[childOne])
        {
            Tile childTile = new Tile();
            childTile.X = tile.X + 1;
            childTile.Y = tile.Y * 2;
            childTile.Z = tile.Z * 2;
            
            if (childTile.X<6)
            {
                AddChildren(childTile, localIndex,levelstart);
            }
            tile.children.Add(childTile);
        }

        localIndex++;
        childOne++;
        if (subtree.TileAvailability[childOne])
        {
            Tile childTile = new Tile();
            childTile.X = tile.X + 1;
            childTile.Y = tile.Y * 2;
            childTile.Z = tile.Z * 2+1;

            if (childTile.X < 6)
            {
                AddChildren(childTile, localIndex, levelstart);
            }
            tile.children.Add(childTile);
        }

        localIndex++;
        childOne++;
        if (subtree.TileAvailability[childOne])
        {
            Tile childTile = new Tile();
            childTile.X = tile.X + 1;
            childTile.Y = tile.Y * 2+1;
            childTile.Z = tile.Z * 2;

            if (childTile.X < 6)
            {
                AddChildren(childTile, localIndex, levelstart);
            }
            tile.children.Add(childTile);
        }

        localIndex++;
        childOne++;
        if (subtree.TileAvailability[childOne])
        {
            Tile childTile = new Tile();
            childTile.X = tile.X + 1;
            childTile.Y = tile.Y * 2+1;
            childTile.Z = tile.Z * 2+1;

            if (childTile.X < 6)
            {
                AddChildren(childTile, localIndex, levelstart);
            }
            tile.children.Add(childTile);
        }

    }
}
