using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
using Netherlands3D.TileSystem;
using UnityEngine.Rendering;


namespace Netherlands3D.Geoservice
{
    public class CreateWMSLayer : MonoBehaviour
    {
        [HideInInspector]
        public WMSImageLayer layer;

        public GameObject TilePrefab;
        private TileHandler tileHandler;

        [System.Serializable]
        public class WMSLOD
        {
            public int textureSize;
            public float maximumDistance;
        }
        [Header("WMS texture sizes")]
        [SerializeField]
        private WMSLOD[] wmsLods = new WMSLOD[3]
        {
            new WMSLOD() { textureSize = 16, maximumDistance = 6000 },
            new WMSLOD() { textureSize = 256, maximumDistance = 3000 },
            new WMSLOD() { textureSize = 2048, maximumDistance = 1000 }
        };

        [SerializeField] private int tileSize = 1500;
        [SerializeField] private bool compressLoadedTextures = true;
        private bool DisplayState = true;

        void Start()
        {
            tileHandler = FindObjectOfType(typeof(TileHandler)) as TileHandler;
            if (!tileHandler)
                Debug.LogWarning("No TileHandler found. This script depends on a TileHandler.", this.gameObject);
        }

        /// <summary>
        /// turn the layer on or off
        /// </summary>
        /// <param name="OnOff"></param>
        public void ShowLayer(bool OnOff)
        {
            DisplayState = OnOff;
            Debug.Log("Show layer " + OnOff);
            if (layer)
            {
                layer.isEnabled = OnOff;
                layer.gameObject.SetActive(OnOff);
            }
        }

        /// <summary>
        /// Clears layer from tilehandler
        /// </summary>
        public void UnloadLayer()
        {
            Debug.Log("Removing WMS layer");
            tileHandler.RemoveLayer(layer);
            Destroy(layer.gameObject);
            layer = null;
        }

        /// <summary>
        /// Create a new layer using a WMS base url.
        /// The following placeholders can be used:
        /// {Width} and {Height} to determine requested image size.
        /// {Xmin},{Ymin},{Xmax} and {Ymax} to set the boundingbox bottom left (xmin) and top right (ymax) coordinates.
        /// </summary>
        /// <param name="baseURL">The WMS base url. For example 'https://service.pdok.nl/hwh/luchtfotorgb/wms/v1_0?service=WMS&request=GETMAP&version=1.1.1&LAYERS=Actueel_orthoHR&Styles=Default&WIDTH={Width}&HEIGHT={Height}&format=image/jpeg&srs=EPSG:28992&bbox={Xmin},{Ymin},{Xmax},{Ymax}&transparent=true'</param>
        public void CreateLayer(string baseURL)
        {
            if (layer != null)
            {

                UnloadLayer();
            }

            Debug.Log("Creating WMS layer");
            GameObject layerContainer = null;

            if (layer != null)
            {
                tileHandler.RemoveLayer(layer);
                layerContainer = layer.gameObject;
                layer = null;
            }

            if (layerContainer == null)
            {
                layerContainer = new GameObject("WMSLayer");
                layerContainer.layer = this.gameObject.layer;
                layerContainer.transform.parent = transform;
            }
            layer = layerContainer.AddComponent<WMSImageLayer>();
            layer.compressLoadedTextures = compressLoadedTextures;
            layer.tileSize = tileSize;

            AddWMSLayerDataSets(baseURL);
            tileHandler.AddLayer(layer);

            ShowLayer(DisplayState);
        }

        private void AddWMSLayerDataSets(string baseURL)
        {
            for (int i = 0; i < wmsLods.Length; i++)
            {
                var wmsLOD = wmsLods[i];
                DataSet dataSet = new DataSet();
                string datasetURL = baseURL.Replace("{Width}", wmsLOD.textureSize.ToString());
                datasetURL = datasetURL.Replace("{Height}", wmsLOD.textureSize.ToString());
                dataSet.path = datasetURL;
                dataSet.maximumDistance = wmsLOD.maximumDistance;

                layer.TilePrefab = TilePrefab;
                layer.Datasets.Add(dataSet);
            }
        }
    }
}
