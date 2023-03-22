using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Netherlands3D.Events;
using TMPro;

public class WMSInterface : MonoBehaviour
{
    [SerializeField] private GameObject messagePanel;

    [Header("Options Parents")]
    [SerializeField] private Transform layerContentParent;
    [SerializeField] private Transform styleContentParent;
    [SerializeField] private GameObject styleWindow;
    [SerializeField] private Transform crsContentParent;
    [SerializeField] private Transform activeLayerParent;

    [Header("Prefabs")]
    [SerializeField] private DualTextComponent dtcPrefab;
    [SerializeField] private Button layerButtonPrefab;

    [Header("Preview Image")]
    [SerializeField] private RawImage previewRawImage;
    [SerializeField] private RawImage legendRawImage;

    [Header("Invoked Events")]
    [SerializeField] private StringEvent messageTitleEvent;
    [SerializeField] private StringEvent urlDisplayEvent;

    [Header("Listen Events")]
    [SerializeField] private TriggerEvent resetEvent;
    [SerializeField] private ObjectEvent buildInterfaceEvent;
    [SerializeField] private ObjectEvent imageEvent;
    [SerializeField] private ObjectEvent legendEvent;
    [SerializeField] private TriggerEvent logEvent;

    private int legendIndex = 0;
    private List<Texture> legends = new();
    //private System.Action layerAdded;

    //[Header("Invoke Events")]
    //[SerializeField] private ObjectEvent styleApplication;
    //[SerializeField] private ObjectEvent layerDeactivation;

    private Dictionary<System.Tuple<string, string>, DualTextComponent> dtcs = new();
    private List<string> activeCRSOptions = new();

    private void Awake()
    {
        legendRawImage.gameObject.SetActive(false);
        resetEvent.AddListenerStarted(ResetInterface);
        buildInterfaceEvent.AddListenerStarted(BuildInterface);
        imageEvent.AddListenerStarted(DisplayPreviewImage);
        legendEvent.AddListenerStarted(GetLegendTexture);

        logEvent.AddListenerStarted(() =>
            {
                messagePanel.SetActive(true);
                messageTitleEvent.Invoke("Url Logged");
                WMS.ActiveInstance.IsPreview(false);
                urlDisplayEvent.Invoke(WMS.ActiveInstance.GetMapRequest());
            }
        );
    }

    private void Start()
    {
        styleWindow.SetActive(false);
    }
    public void ResetInterface()
    {
        dtcs.Clear();
        ClearLayers();
        ClearActivatedLayers();
        ClearStyles();
        ClearLegends();
    }

    public void BuildInterface(object layerList)
    {
        foreach(WMSLayer layer in (List<WMSLayer>)layerList)
        {
            // Create a new button for each Layer, based on the prefab. Set the text in the button to the Layer's title.
            // Pressing the button will display the styles within that Layer in the style window.
            Button newLayerButton = Instantiate(layerButtonPrefab, layerContentParent);
            newLayerButton.GetComponentInChildren<TextMeshProUGUI>().text = layer.Title;
            newLayerButton.onClick.AddListener(() => DisplayStyles(layer));
        }
    }
    public void DisplayStyles(WMSLayer styledLayer)
    {
        // We clear the styles that are currently displayed and belong to the previous (if applicable)
        ClearStyles();
        if(styledLayer.styles.Count is 0)
        {
            // We can immediately apply the layer if it has no styles (such as aerial photos).
            ApplyStyle(styledLayer, null);
            return;
        }
        // If the selected layer has no styles, we don't need to show the style window at all.
        styleWindow.SetActive(true);
        foreach(KeyValuePair<string, WMSStyle> stylePair in styledLayer.styles)
        {
            // Create a new button for each Style, based on the prefab. Set the text in the button to the Style's title.
            // Pressing the button will apply the Style for the active layer.
            Button newLayerButton = Instantiate(layerButtonPrefab, styleContentParent);
            newLayerButton.GetComponentInChildren<TextMeshProUGUI>().text = stylePair.Value.Title;
            newLayerButton.onClick.AddListener(() => 
            {
                // Pressing a style button will apply it and also close the style window.
                ApplyStyle(styledLayer, stylePair.Value);
                styleWindow.SetActive(false); 
            });
        }


    }
    public void ToggleLegendForward()
    {
        if (legends.Count <= 1)
            return;
        legendIndex = (legendIndex + 1) % legends.Count;
        DisplayLegendImage(legends[legendIndex]);
    }
    public void ToggleLegendBackward()
    {
        if (legends.Count <= 1)
            return;
        legendIndex = legendIndex - 1 < 0 ? legends.Count - 1 : legendIndex - 1;
        DisplayLegendImage(legends[legendIndex]);
    }
    public void ClearLegends()
    {
        legends.Clear();
        legendRawImage.gameObject.SetActive(false);

    }
    private void ClearLayers()
    {
        for (int i = layerContentParent.childCount - 1; i >= 0; i--)
        {
            GameObject child = layerContentParent.GetChild(i).gameObject;
            Button b = child.GetComponent<Button>();
            b.onClick.RemoveAllListeners();
            Destroy(child);
        }
    }
    private void ClearActivatedLayers()
    {
        for (int i = activeLayerParent.childCount - 1; i >= 0; i--)
        {
            GameObject child = activeLayerParent.GetChild(i).gameObject;
            Button b = child.GetComponentInChildren<Button>();
            b.onClick.RemoveAllListeners();
            Destroy(child);
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
        System.Tuple<string, string> layerStyleKey;
        if (styleToApply == null)
        {
            layerStyleKey = new System.Tuple<string, string>(layerToStyle.Name, "null");
        }
        else
        {
            layerStyleKey = new System.Tuple<string, string>(layerToStyle.Name, styleToApply.Name);
        }
        if (dtcs.ContainsKey(layerStyleKey))
        {
            Debug.Log("This layer has already been added with this particular style!");
            return;
        }
        if (!ActivateLayer(layerToStyle))
        {
            return;
        }
        layerToStyle.SelectStyle(styleToApply);

        DualTextComponent dualText = Instantiate(dtcPrefab, activeLayerParent);
        Button btn = dualText.closeButton;

        //TwoWayUISorter sorter = dualText.GetComponent<TwoWayUISorter>();
        //layerAdded += sorter.EvaluateButtonStates;

        btn.onClick.AddListener(() =>
        {
            DeactivateLayer(layerToStyle);
            dtcs.Remove(layerStyleKey);
            //layerAdded -= sorter.EvaluateButtonStates;
            Destroy(dualText.gameObject);
        }
        );

        dualText.SetMainText(layerToStyle.Title);
        if(styleToApply != null)
        {
            dualText.SetSubText(styleToApply.Title);
        }
        else
        {
            dualText.SetSubText("");
        }
        dtcs.Add(layerStyleKey, dualText);
    }

    private bool ActivateLayer(WMSLayer layerToActivate)
    {
        if(WMS.ActiveInstance.ActivatedLayers.Count is 0)
        {
            activeCRSOptions = layerToActivate.CRS;
        }
        else
        {
            List<string> newCRSOptions = new List<string>();
            for(int i = 0; i < activeCRSOptions.Count; i++)
            {
                string currentCRS = activeCRSOptions[i];
                if (layerToActivate.CRS.Contains(currentCRS))
                {
                    newCRSOptions.Add(currentCRS);
                }
            }
            if(newCRSOptions.Count is 0)
            {
                Debug.Log("Adding this new layer means no matching CRS's are available anymore! Cannot add it to the list!");
                return false;
            }
            activeCRSOptions = newCRSOptions;
        }
        WMS.ActiveInstance.ActivateLayer(layerToActivate);
        //layerAdded?.Invoke();
        ShowReferenceSystemOptions();
        return true;
    }

    private void DeactivateLayer(WMSLayer layerToDeactivate)
    {
        if (WMS.ActiveInstance.ActivatedLayers.Contains(layerToDeactivate))
        {
            WMS.ActiveInstance.DeactivateLayer(layerToDeactivate);
        }
    }
    private void ShowReferenceSystemOptions()
    {
        for (int i = crsContentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(crsContentParent.GetChild(i).gameObject);
        }
        foreach(string referenceSystem in activeCRSOptions)
        {
            Button b = Instantiate(layerButtonPrefab, crsContentParent);
            b.GetComponentInChildren<TextMeshProUGUI>().text = referenceSystem;
            // This AddListener checks if the current WMS requires an SRS instead of CRS and sets the appropiate value with one of two lambda functions.
            // Scenarios in which you need both may(?) arise, in which case this part of building the interface would require some refactoring.
            b.onClick.AddListener(WMS.ActiveInstance.RequiresSRS ? 
                () => WMS.ActiveInstance.SetSRS(referenceSystem) : () => WMS.ActiveInstance.SetCRS(referenceSystem)
                );
        }
    }
    private void DisplayPreviewImage(object textureFromRequest)
    {
        previewRawImage.texture = (Texture)textureFromRequest;
    }
    private void GetLegendTexture(object textureFromRequest)
    {
        Texture txt = (Texture)textureFromRequest;
        legends.Add(txt);
        if(legends.Count == 1)
        {
            DisplayLegendImage(legends[0]);
        }
    }
    private void DisplayLegendImage(Texture txt)
    {
        if (!legendRawImage.gameObject.activeSelf)
        {
            legendRawImage.gameObject.SetActive(true);
        }
        legendRawImage.texture = txt;
        legendRawImage.SetNativeSize();
        Rect rect = legendRawImage.rectTransform.rect;
        legendRawImage.rectTransform.ScaleWithAspectRatio(100, rect.x <= rect.y ? SizeRef.WIDTH : SizeRef.HEIGHT);
        //legendRawImage.SetNativeSize();
        //legendRawImage.SizeToParent(5);
    }
}
