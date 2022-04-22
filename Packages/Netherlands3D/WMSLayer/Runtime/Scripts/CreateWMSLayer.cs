using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
using Netherlands3D.TileSystem;

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

        private bool OnlyOnTerrain_Memory = false;
        private bool DisplayState = true;
        //public List<LayerMask> layermasks;
        
        // Start is called before the first frame update
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

        void UnloadLayer()
        {
            tileHandler.RemoveLayer(layer);
            Destroy(layer.gameObject);
            layer = null;
        }

        void ShowOnlyOnTerrain(bool toggleValue)
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

        void showWMSOnBuildings()
        {
            if (layer)
            {
               layer.ProjectOnBuildings(buildingStencilID);
                            }
        }
        void showWMSOnTerrain()
        {
            if (layer)
            {
                layer.ProjectOnTerrain(terrainStencilID);
            }
        }
        void showWMSOnBuildingsAndTerrain()
        {
            if (layer)
            {
                layer.ProjectOnBoth(buildingStencilID, terrainStencilID);
            }
        }

        // Update is called once per frame
        void CreateWMS_Layer(string baseURL)
        {
            Debug.Log("createWMSlayer");
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
                layercontainer.layer = this.gameObject.layer;
                layercontainer.transform.parent = transform;
                
                
            }
            layer = layercontainer.AddComponent<WMSImageLayer>();
            layer.tileSize = 1500;
            if (OnlyOnTerrain_Memory)
            {
                showWMSOnTerrain();
            }
            else
            {
                showWMSOnBuildingsAndTerrain();
            }
            

            DataSet dataSet = new DataSet();
            string datasetURL = baseURL.Replace("{Width}", "16");
            datasetURL = datasetURL.Replace("{Height}", "16");
            dataSet.path = datasetURL;
            dataSet.lod = 0;
            dataSet.maximumDistance = 6000;
            layer.TilePrefab = TilePrefab;
            layer.Datasets.Add(dataSet);

            dataSet = new DataSet();
            datasetURL = baseURL.Replace("{Width}", "256");
            datasetURL = datasetURL.Replace("{Height}", "256");
            dataSet.path = datasetURL;
            dataSet.lod = 1;
            dataSet.maximumDistance = 3000;
            layer.TilePrefab = TilePrefab;
            layer.Datasets.Add(dataSet);

            dataSet = new DataSet();
            datasetURL = baseURL.Replace("{Width}", "2048");
            datasetURL = datasetURL.Replace("{Height}", "2048");
            dataSet.path = datasetURL;
            dataSet.lod = 2;
            dataSet.maximumDistance = 1000;
            layer.TilePrefab = TilePrefab;
            layer.Datasets.Add(dataSet);
            tileHandler.AddLayer(layer);


            ShowLayer(DisplayState);
        }

        
    }
}
