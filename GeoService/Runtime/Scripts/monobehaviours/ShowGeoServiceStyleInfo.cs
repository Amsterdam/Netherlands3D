using Netherlands3D.Geoservice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Netherlands3D.Geoservice
{
    public class ShowGeoServiceStyleInfo : MonoBehaviour
    {
        int layerIndex;
        int styleIndex;
        [HideInInspector]
        public ServerData serverData;
        [HideInInspector]
        public ShowGeoServiceResults mainScript;
        [Tooltip("textelement waarin de titel van de Style aangegeven wordt (optioneel)")]
        public Text StyleTitle;
        [Tooltip("textelement waarin de beschrijving van de Style aangegeven wordt (optioneel)")]
        public Text StyleAbstract;
        [Tooltip("Button waarmee de laag geselecterd wordt")]
        public Button StyleButton;

        public void displayStyleInfo(int LayerIndex, int StyleIndex)
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

        private void createLayer()
        {
            mainScript.SelectLayer(layerIndex, styleIndex);
        }


    }
}
