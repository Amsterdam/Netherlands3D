using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Core;
using Netherlands3D.TileSystem;
using Netherlands3D.Events;


public class ModelFormatCreation : MonoBehaviour
{
    [Header("during progress")]
    [SerializeField] protected StringEvent progressTitle;
    [SerializeField] protected StringEvent progressMessage;
    [SerializeField] protected FloatEvent progressPercentage;
    [SerializeField] protected BoolEvent ShowHideprogressWindow;
    [Header("when finished")]
    [SerializeField] protected StringEvent filename;
    [SerializeField] protected StringEvent onError;
    

    public void FreezeLayers(List<Layer> layerList, bool freeze)
    {
        foreach (var layer in layerList)
        {
            layer.pauseLoading = freeze;
        }
    }

    public List<GameObject> GetTilesInLayer(Layer layer, Vector3RD bottomLeftRD, Vector3RD topRightRD)
    {
        if (layer == null)
        {
            return new List<GameObject>();
        }
        List<GameObject> output = new List<GameObject>();
        double tilesize = layer.tileSize;
        Debug.Log(tilesize);
        int tileX;
        int tileY;
        foreach (var tile in layer.tiles)
        {
            tileX = tile.Key.x;
            tileY = tile.Key.y;

            if (tileX + tilesize < bottomLeftRD.x || tileX > topRightRD.x)
            {
                continue;
            }
            if (tileY + tilesize < bottomLeftRD.y || tileY > topRightRD.y)
            {
                continue;
            }
            MeshFilter[] meshFilters = tile.Value.gameObject.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in meshFilters)
            {
                output.Add(meshFilter.gameObject);
            }
        }
        return output;
    }

}
    
