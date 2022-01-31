using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Geoservice
{
    public class ShowGeoServiceLayerInfo : MonoBehaviour
    {
        [HideInInspector]
        public ServerData serverData;
        [HideInInspector]
        public ShowGeoServiceResults mainScript;
        int layerIndex;
        int styleIndex;
        [HideInInspector]
        public int startStyleIndex;
        [Header("LayerInformation")]
        [Tooltip("textelement waarin de titel van de layer aangegeven wordt (optioneel)")]
        public Text LayerTitle;
        [Tooltip("textelement waarin de omschrijving van de layer aangegeven wordt (optioneel)")]
        public Text LayerAbstract;
        [Tooltip("Button waarmee de laag geselecterd wordt(Optioneel)\nWanneer een button is aangegeven wordt deze getoond als de layer maar 1 style heeft. er wordt dan geen style-object aangemaakt")]
        public Button LayerButton;
        [Tooltip("gameobject waarbinnen de style-objecten geplaatst worden. wanneer geen gameObject is aangegeven worden de style-objecten onder het layerObject geplaatst")]
        [Header("StyleInfomation")]

        public GameObject StylesContainer;
        // Start is called before the first frame update
        [HideInInspector]
        public GameObject StylePrefab;

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
                    startStyleIndex = 1;
                    LayerButton.gameObject.SetActive(true);
                    LayerButton.onClick.AddListener(createLayer);
                }
                else
                {
                    startStyleIndex = 0;
                    
                    LayerButton.gameObject.SetActive(false);
                }


            }
            if (StylesContainer != null)
            {
                for (int i = startStyleIndex; i < serverData.layer[layerIndex].styles.Count; i++)
                {
                    CreateStyleObjects(LayerIndex, i, StylePrefab, StylesContainer.transform);
                }
            }
        }
        public void CreateStyleObjects(int LayerIndex, int startIndex, GameObject stylePrefab, Transform parent = null)
        {
            if (parent == null)
            {
                return;
            }
            for (int i = startIndex; i < serverData.layer[LayerIndex].styles.Count; i++)
            {
                GameObject styleObject = Instantiate(stylePrefab, parent);
                ShowGeoServiceStyleInfo styleobject = styleObject.GetComponent<ShowGeoServiceStyleInfo>();
                styleobject.mainScript = mainScript;
                styleobject.serverData = serverData;
                styleobject.displayStyleInfo(LayerIndex, i);
            }

        }

        public void createLayer()
        {
            mainScript.SelectLayer(layerIndex, styleIndex);
        }

    }
}