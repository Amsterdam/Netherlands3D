using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Netherlands3D.Events;

namespace Netherlands3D.wmsServer
{
    public class wmsresults : MonoBehaviour
    {
        [Header ("dataSource")]
        public GetCapabiltiesDownload dataOwner;

        [Header("ListenToEvents")]
        public TriggerEvent displayServerdataEvent;


        [Header ("data")]
        public ServerData serverData;

        [Header("serviceInformation")]
        public Text ServiceTitle;
        public Text ServiceAbstract;
        
        public GameObject LayersContainer;

        
        [Header("LayerInformation")]
        public GameObject LayerPrefab;
        public Text LayerTitle;
        public Text LayerAbstract;
        public Button LayerButton;
        public GameObject StylesContainer;
        

        [Header ("StyleInformation")]
        public GameObject StylePrefab;
        public Text StyleTitle;
        public Text StyleAbstract;
        public Button StyleButton;


        [Header("results")]
        [SerializeField]
        private int layerIndex;
        [SerializeField]
        private int styleIndex;
        public StringEvent onLayerSelected;
        public StringEvent onLegendSelected;


        void Awake()
        {
           
            if (displayServerdataEvent!=null)
            {
                displayServerdataEvent.started.AddListener(DisplayServiceInfo);
            }
            

        }

       
        public void DisplayServiceInfo(bool arg0)
        {
            if (arg0 == false)
            {
                return;
            }
            DisplayServiceInfo();
        }

        public void DisplayServiceInfo()
        {
            serverData = dataOwner.serverData;
            if (ServiceTitle != null) ServiceTitle.text = serverData.ServiceTitle;
            if (ServiceAbstract != null) ServiceAbstract.text = serverData.ServiceAbstract;

            //clear the layers
            // clear the list
            foreach ( Transform child in LayersContainer.transform)
            {
                if (child != StylesContainer.transform)
                {
                    Destroy(child.gameObject);
                }
            }

            // loop trough the layers
            for (int i = 0; i < serverData.layer.Count; i++)
            {
                GameObject layerObject = Instantiate(LayerPrefab,LayersContainer.transform);
                
                wmsresults layerinfo = layerObject.GetComponent<wmsresults>();
                layerinfo.serverData = serverData;
                layerinfo.dataOwner = dataOwner;
                layerinfo.displayLayerInfo(i);
                if (StylesContainer != null)
                {
                    int startindex = 0;
                    if (layerinfo.LayerButton!=null)
                    {
                        startindex = 1;
                    }
                    CreateStyleObjects(i,startindex);
                }
            }

        }

        private void CreateStyleObjects(int LayerIndex, int startIndex)
        {
            if (StylesContainer != null)
            {
                // clear the list
                //foreach (GameObject child in StylesContainer.transform)
                //{
                //    if (child !=StylesContainer)
                //    {
                //        Destroy(child);
                //    }
                //}

                for (int i = startIndex; i < serverData.layer[LayerIndex].styles.Count; i++)
                {
                    GameObject styleObject = Instantiate(StylePrefab, StylesContainer.transform);
                    wmsresults styleobject = styleObject.GetComponent<wmsresults>();
                    styleobject.serverData = serverData;
                    styleobject.dataOwner = dataOwner;
                    styleobject.displayStyleInfo(LayerIndex, i);
                }
            }
        }

        public void displayLayerInfo(int LayerIndex)
        {
            layerIndex = LayerIndex;
            if (LayerTitle != null) LayerTitle.text = serverData.layer[layerIndex].Title;
            if (LayerAbstract != null) LayerAbstract.text = serverData.layer[layerIndex].Abstract;
            if (LayerButton != null)
            {
                if (serverData.layer[layerIndex].styles.Count == 1)
                {
                    styleIndex = 0;
                    LayerButton.gameObject.SetActive(true);
                    LayerButton.onClick.AddListener(createLayer);
                }
                else
                {
                    LayerButton.gameObject.SetActive(false);
                    CreateStyleObjects(layerIndex,0);
                }
            }

        }

        public void displayStyleInfo(int LayerIndex,int StyleIndex)
        {
            layerIndex = LayerIndex;
            styleIndex = StyleIndex;
            if (StyleTitle != null) StyleTitle.text = serverData.layer[layerIndex].styles[styleIndex].Title;
            if (StyleAbstract != null) StyleAbstract.text = serverData.layer[layerIndex].styles[styleIndex].Abstract;
            if (StyleButton != null)
            {

                StyleButton.gameObject.SetActive(true);
                StyleButton.onClick.AddListener(createLayer);
            }


        }
        public void createLayer()
        {
            if (onLayerSelected!=null)
            {
                onLayerSelected.started.Invoke(serverData.layer[layerIndex].styles[styleIndex].imageURL);

            }
            if (onLegendSelected != null)
            {
                onLegendSelected.started.Invoke(serverData.layer[layerIndex].styles[styleIndex].LegendURL);

            }
        }
    }
}
