using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
using Netherlands3D.TileSystem;
namespace Netherlands3D.Geoservice
{
    public class CreateWMSLayer : MonoBehaviour
    {
        public WMSImageLayer layer;
        public GameObject TilePrefab;
        private TileSystem.TileHandler tileHandler;
        public StringEvent OnWMSUrlDefined_String;
        // Start is called before the first frame update
        void Start()
        {
            if (OnWMSUrlDefined_String != null)
            {
                OnWMSUrlDefined_String.started.AddListener(CreateWMS_Layer);
            }
            tileHandler = FindObjectOfType(typeof(TileSystem.TileHandler)) as TileSystem.TileHandler;
        }

        // Update is called once per frame
        void CreateWMS_Layer(string baseURL)
        {
            GameObject layercontainer=null;


            if (layer != null)
            {
                tileHandler.RemoveLayer(layer);
                layercontainer = layer.gameObject;
                layer = null;
            }
            
            if (layercontainer == null)
            {
                layercontainer = new GameObject("WMSLayer");
                layercontainer.transform.parent = transform;
                
            }
            layer = layercontainer.AddComponent<WMSImageLayer>();
            DataSet dataSet = new DataSet();
            dataSet.path = baseURL;
            dataSet.maximumDistance = 6000;
            layer.TilePrefab = TilePrefab;
            layer.Datasets.Add(dataSet);

            tileHandler.AddLayer(layer);
        }
    }
}
