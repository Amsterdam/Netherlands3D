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

        [Header("Events")]
        public StringEvent OnWMSUrlDefined_String;
        public BoolEvent ShowLayer_Bool;
        public TriggerEvent UnloadWMSService;

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
            if (OnWMSUrlDefined_String)
            {
                OnWMSUrlDefined_String.AddListenerStarted(CreateLayer);
            }  
            if (UnloadWMSService)
            {
                UnloadWMSService.AddListenerStarted(UnloadLayer);
            }
            if (ShowLayer_Bool)
            {
                ShowLayer_Bool.AddListenerStarted(ShowLayer);
            }
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

        private void UnloadLayer()
        {
            Debug.Log("Removing WMS layer");
            tileHandler.RemoveLayer(layer);
            Destroy(layer.gameObject);
            layer = null;
        }

        private void CreateLayer(string baseURL)
        {
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
