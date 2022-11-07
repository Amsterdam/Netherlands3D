using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class WMSInterface : MonoBehaviour
{

    [SerializeField] private Transform layerContentParent;
    [SerializeField] private Transform styleContentParent;
    [SerializeField] private Button layerButtonPrefab;

    public void ResetInterface()
    {
        for(int i = layerContentParent.childCount - 1; i >= 0; i--)
        {
            GameObject child = layerContentParent.GetChild(i).gameObject;
            Button b = child.GetComponent<Button>();
            b.onClick.RemoveAllListeners();
            Destroy(child);
        }
        ClearStyles();
    }

    public void BuildInterface(object layerList)
    {
        foreach(WMSLayer layer in (List<WMSLayer>)layerList)
        {
            Button newLayerButton = Instantiate(layerButtonPrefab, layerContentParent);
            newLayerButton.GetComponentInChildren<Text>().text = layer.Title;
            newLayerButton.onClick.AddListener(() => DisplayStyles(layer));
        }
    }
    public void DisplayStyles(WMSLayer styledLayer)
    {
        ClearStyles();
        foreach(KeyValuePair<string, WMSStyle> stylePair in styledLayer.styles)
        {
            Button newLayerButton = Instantiate(layerButtonPrefab, styleContentParent);
            newLayerButton.GetComponentInChildren<Text>().text = stylePair.Value.Title;
            // Implementation for style selection needs to be done here!

            //newLayerButton.onClick.AddListener(() => DisplayStyles(layer));
        }


    }

    private void ClearStyles()
    {
        for (int i = styleContentParent.childCount - 1; i >= 0; i--)
        {
            GameObject child = styleContentParent.GetChild(i).gameObject;
            Button b = child.GetComponent<Button>();
            b.onClick.RemoveAllListeners();
            Destroy(child);
        }
    }

}
