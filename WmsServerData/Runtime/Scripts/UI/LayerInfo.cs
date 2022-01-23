using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Netherlands3D.Events;

namespace Netherlands3D.wmsServer
{
    public class LayerInfo : MonoBehaviour
    {
        [HideInInspector]
        public ImageGeoserviceLayer layer;

        public Text layernameTextelement;
        public Text layerTitleTextelement;
        public Text layertypeTextelement;
        
        public Text layerDescriptionTextelement;
        public Button loadLayerButton;

        public void createLayerInfo()
        {
            if (layernameTextelement != null) layernameTextelement.text = layer.Name;
            if (layerTitleTextelement != null) layerTitleTextelement.text = layer.Name;
            if (layerDescriptionTextelement != null) layerDescriptionTextelement.text = layer.Abstract;
        }
        
    }
    public class styleInfo:MonoBehaviour
    {
        [HideInInspector]
        public ImageGeoserviceStyle style;

        public StringEvent loadImageGeoserviceLayer;
        public StringEvent loadImageGeoserviceLegend;
        public Text stylenameTextelement;
        public Text styleDescriptionTextelement;
        
        public void createStyleInfo()
        {
            if (stylenameTextelement != null) stylenameTextelement.text = style.Title;

        }
        public void createLayer()
        {
            if (loadImageGeoserviceLayer != null)
            {
                loadImageGeoserviceLayer.started.Invoke(style.imageURL);

            }
            if (loadImageGeoserviceLegend != null)
            {
                loadImageGeoserviceLegend.started.Invoke(style.LegendURL);

            }
        }
    }
}