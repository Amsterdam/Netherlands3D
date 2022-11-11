using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Netherlands3D.Events;

public class WMSInterface : MonoBehaviour
{

    [SerializeField] private Transform layerContentParent;
    [SerializeField] private Transform styleContentParent;
    [SerializeField] private Transform activeLayerParent;

    [SerializeField] private DualTextComponent dtcPrefab;
    [SerializeField] private Button layerButtonPrefab;

    [SerializeField] private RawImage previewRawImage;

    [Header("Invoke Events")]
    [SerializeField] private ObjectEvent styleApplication;
    [SerializeField] private ObjectEvent layerDeactivation;

    private Dictionary<System.Tuple<string, string>, DualTextComponent> dtcs = new();
    
    
    public void DisplayPreviewImage(object textureFromRequest)
    {
        previewRawImage.texture = (Texture)textureFromRequest;
    }
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

            newLayerButton.onClick.AddListener(() => ApplyStyle(styledLayer, stylePair.Value));
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

    private void ApplyStyle(WMSLayer layerToStyle, WMSStyle styleToApply)
    {
        System.Tuple<string, string> layerStyleKey = new System.Tuple<string, string>(layerToStyle.Name, styleToApply.Name);
        if (dtcs.ContainsKey(layerStyleKey))
        {
            Debug.Log("This layer has already been added with this particular style!");
            return;
        }
        layerToStyle.SelectStyle(styleToApply);
        ActivateLayer(layerToStyle);

        DualTextComponent dualText = Instantiate(dtcPrefab, activeLayerParent);
        Button btn = dualText.GetComponent<Button>();

        btn.onClick.AddListener(() => DeactivateLayer(layerToStyle));
        btn.onClick.AddListener(() => dtcs.Remove(new System.Tuple<string, string>(layerToStyle.Name, styleToApply.Name)));
        btn.onClick.AddListener(() => Destroy(btn.gameObject));

        dualText.SetMainText(layerToStyle.Title);
        dualText.SetSubText(styleToApply.Title);
        dtcs.Add(layerStyleKey, dualText);
    }

    private void ActivateLayer(WMSLayer layerToActivate)
    {
        if(styleApplication != null)
        {
            styleApplication.Invoke(layerToActivate);
        }
        //wmsSettings.GetWMSRequest();
    }

    private void DeactivateLayer(WMSLayer layerToDeactivate)
    {
        if (layerDeactivation != null)
        {
            layerDeactivation.Invoke(layerToDeactivate);
        }
    }



}
