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
        private TileSystem.TileHandler tileHandler;
        [SerializeField]
        private int buildingStencilID;
        [SerializeField]
        private int terrainStencilID;

        [Header("Events")]
        public StringEvent OnWMSUrlDefined_String;
        public BoolEvent AleenOpMaaiveld_Bool;
        public BoolEvent ShowLayer_Bool;
        public TriggerEvent ShowWMSOnBuildings;
        public TriggerEvent ShowWMSOnTerrain;
        public TriggerEvent ShowWMSOnBuildingsAndTerrain;
        public TriggerEvent UnloadWMSService;

        [Header("URP RenderObjects")]
        
        public Object RenderObjectTerrainAsTarget;
        public Object RenderObjectTerrainNOTAsTarget;
        public Object RenderObjectBuildingsAsTarget;
        public Object RenderObjectBuildingsNOTAsTarget;

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

        [SerializeField]
        private int tileSize = 1500;
        [SerializeField] private bool compressLoadedTextures = true;
        private bool OnlyOnTerrain_Memory = false;
        private bool DisplayState = true;

        void Start()
        {
            tileHandler = FindObjectOfType(typeof(TileSystem.TileHandler)) as TileSystem.TileHandler;
            if (OnWMSUrlDefined_String)
            {
                OnWMSUrlDefined_String.started.AddListener(CreateWMS_Layer);
            }
            if (ShowWMSOnBuildings)
            {
                ShowWMSOnBuildings.started.AddListener(showWMSOnBuildings);
            }
            if (ShowWMSOnTerrain)
            {
                ShowWMSOnTerrain.started.AddListener(showWMSOnTerrain);
            }
            if (ShowWMSOnBuildingsAndTerrain)
            {
                ShowWMSOnBuildingsAndTerrain.started.AddListener(showWMSOnBuildingsAndTerrain);
            }
            if (UnloadWMSService)
            {
                UnloadWMSService.started.AddListener(UnloadLayer);
            }
            if (AleenOpMaaiveld_Bool)
            {
                AleenOpMaaiveld_Bool.started.AddListener(ShowOnlyOnTerrain);
            }
            if (ShowLayer_Bool)
            {
                ShowLayer_Bool.started.AddListener(ShowLayer);
            }
        }
        /// <summary>
        /// turn the layer on or off
        /// </summary>
        /// <param name="OnOff"></param>
        public void ShowLayer(bool OnOff)
        {
            DisplayState = OnOff;
            if (layer)
            {
                layer.isEnabled = OnOff;
                layer.gameObject.SetActive(OnOff);
            }
        }

        private void UnloadLayer()
        {
            tileHandler.RemoveLayer(layer);
            Destroy(layer.gameObject);
            layer = null;
        }

        private void ShowOnlyOnTerrain(bool toggleValue)
        {
            
            OnlyOnTerrain_Memory = toggleValue;
            if (toggleValue)
            {
                showWMSOnTerrain();
            }
            else
            {
                showWMSOnBuildingsAndTerrain();
            }
        }

        private void showWMSOnBuildings()
        {
            if (layer)
            {
               layer.ProjectOnBuildings(buildingStencilID);
                            }
        }
        private void showWMSOnTerrain()
        {

            if (RenderObjectTerrainAsTarget)
            {
                //RenderObjectTerrainAsTarget.hideFlags = 
            }
            if (layer)
            {
                layer.ProjectOnTerrain(terrainStencilID);
            }
        }
        private void showWMSOnBuildingsAndTerrain()
        {
            if (layer)
            {
                layer.ProjectOnBoth(buildingStencilID, terrainStencilID);
            }
        }

        private void CreateWMS_Layer(string baseURL)
        {
            Debug.Log("Create WMS layer");
            GameObject layercontainer = null;

            if (layer != null)
            {
                tileHandler.RemoveLayer(layer);
                layercontainer = layer.gameObject;
                layer = null;
            }

            if (layercontainer == null)
            {
                layercontainer = new GameObject("WMSLayer");
                layercontainer.layer = this.gameObject.layer;
                layercontainer.transform.parent = transform;
            }
            layer = layercontainer.AddComponent<WMSImageLayer>();
            layer.compressLoadedTextures = compressLoadedTextures;
            layer.tileSize = tileSize;
            if (OnlyOnTerrain_Memory)
            {
                showWMSOnTerrain();
            }
            else
            {
                showWMSOnBuildingsAndTerrain();
            }

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
                dataSet.lod = i;
                dataSet.maximumDistance = wmsLOD.maximumDistance;

                layer.TilePrefab = TilePrefab;
                layer.Datasets.Add(dataSet);
            }
        }

    }
}
