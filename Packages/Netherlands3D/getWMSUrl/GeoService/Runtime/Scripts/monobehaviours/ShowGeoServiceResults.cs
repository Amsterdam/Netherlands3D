using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;


namespace Netherlands3D.Geoservice
{
    public class ShowGeoServiceResults : MonoBehaviour
    {
        [Header("dataSource")]
        [Tooltip("verwijzing naar het script waarmee de service uitgelezen wordt")]
        public GeoServiceConnector dataOwner;

        [Header("ListenToEvents")]
        [Tooltip("het TriggerEvent waarmee aangegeven wordt dat de serverdata getoond kan worden")]
        public TriggerEvent displayServerdataEvent;

        [Header("Send Events")]
        [Tooltip("string-Event wanneer de url van de webservice bepaald is")]
        public StringEvent onLayerSelected ;
        [Tooltip("string-Event wanneer de url van de legenda bepaald is")]
        public StringEvent onLegendSelected;

        [Header("dataContainers")]
        [Tooltip("verwijzing naar het gameobject waarin de service-informatie getoond wordt")]
        [ContextMenuItem("create in Scene", "CreateServiceInfoTemplate")]
        public ShowGeoServiceInfo serviceInfoContainer;
        void CreateServiceInfoTemplate()
        {
            GameObject serviceInfoGameObject = new GameObject("GeoserviceInfoTemplate");
            serviceInfoGameObject.transform.parent = transform;
            serviceInfoContainer=serviceInfoGameObject.AddComponent<ShowGeoServiceInfo>();
        }

        [Tooltip("verwijzing naar het gameobject waarin de layerInformatie geplaatst moet worden")]
        public GameObject layerInfoContainer;

        [Header("prefabs")]
        [Tooltip("prefab voor de layerInformatie. bevat een component genaamd ShowGeoServiceLayerInfo")]
        [ContextMenuItem("create in Scene", "CreateLayerTemplate")]
        public GameObject LayerPrefab;
        void CreateLayerTemplate()
        {
            LayerPrefab = new GameObject("LayerPrefabTemplate");
            LayerPrefab.transform.parent = transform;
            LayerPrefab.AddComponent<ShowGeoServiceLayerInfo>();
            
        }
        [Tooltip("prefab voor de styleInformatie. bevat een component genaamd ShowGeoServiceStyleInfo")]
        [ContextMenuItem("create in Scene", "CreateStyleTemplate")]
        public GameObject StylePrefab;
        void CreateStyleTemplate()
        {
            StylePrefab = new GameObject("StylePrefabTemplate");
            StylePrefab.transform.parent = transform;
            StylePrefab.AddComponent<ShowGeoServiceStyleInfo>();

        }

        private ServerData serverData;

        // Start is called before the first frame update
        void Start()
        {
            //add listener
            if (displayServerdataEvent != null) displayServerdataEvent.AddListenerStarted(showData);

        }
        private void showData()
        {
            //get the serviceData
            serverData = dataOwner.serverData;
            //show the serviceInfo
            if (serviceInfoContainer != null)
            {
                serviceInfoContainer.serverData = serverData;
                serviceInfoContainer.Show();
            }
            //clear the old layers
            ClearlayerInfoContainer();

            //show the layerinfo
            if (layerInfoContainer != null)
            {
                ShowLayers();
            }

        }
        public void ClearlayerInfoContainer()
        {
            if (layerInfoContainer == null)
            {
                return;
            }
            foreach (Transform child in layerInfoContainer.transform)
            {
                if (child != layerInfoContainer.transform)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void ShowLayers()
        {
            for (int i = 0; i < serverData.layer.Count; i++)
            {

                GameObject layerObject = Instantiate(LayerPrefab, layerInfoContainer.transform);

                ShowGeoServiceLayerInfo layerinfo = layerObject.GetComponent<ShowGeoServiceLayerInfo>();
                layerinfo.mainScript = this;
                layerinfo.serverData = serverData;
                layerinfo.StylePrefab = StylePrefab;
                layerinfo.displayLayerInfo(i);
                
                if (layerinfo.StylesContainer == null)
                {
                    int startindex = layerinfo.startStyleIndex;

                    layerinfo.CreateStyleObjects(i, startindex, StylePrefab, layerInfoContainer.transform);
                }
            }
        }

        public void SelectLayer(int layerIndex, int styleIndex)
        {
            if (onLayerSelected != null)
            {
                onLayerSelected.InvokeStarted(serverData.layer[layerIndex].styles[styleIndex].imageURL);

            }
            if (onLegendSelected != null)
            {
                onLegendSelected.InvokeStarted(serverData.layer[layerIndex].styles[styleIndex].LegendURL);

            }
        }
    }
}