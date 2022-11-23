using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Netherlands3D.Events;

public class WMSInterface : MonoBehaviour
{
    public static List<WMSLayer> ActivatedLayers { get; private set; } = new();

    public int Health { get; private set; }

    [Header("Options Parents")]
    [SerializeField] private Transform layerContentParent;
    [SerializeField] private Transform styleContentParent;
    [SerializeField] private Transform crsContentParent;
    [SerializeField] private Transform activeLayerParent;

    [Header("Prefabs")]
    [SerializeField] private DualTextComponent dtcPrefab;
    [SerializeField] private Button layerButtonPrefab;
    
    [Header("Preview Image")]
    [SerializeField] private RawImage previewRawImage;

    [Header("Listen Events")]
    [SerializeField] private TriggerEvent resetEvent;
    [SerializeField] private ObjectEvent buildInterfaceEvent;
    [SerializeField] private ObjectEvent imageEvent;

    //[Header("Invoke Events")]
    //[SerializeField] private ObjectEvent styleApplication;
    //[SerializeField] private ObjectEvent layerDeactivation;

    private Dictionary<System.Tuple<string, string>, DualTextComponent> dtcs = new();
    private List<string> activeCRSOptions = new();

    private void Awake()
    {
        resetEvent.started.AddListener(ResetInterface);
        buildInterfaceEvent.started.AddListener(BuildInterface);
        imageEvent.started.AddListener(DisplayPreviewImage);
    }

    public void DisplayPreviewImage(object textureFromRequest)
    {
        previewRawImage.texture = (Texture)textureFromRequest;
    }
    public void ResetInterface()
    {
        dtcs.Clear();
        ActivatedLayers.Clear();
        for(int i = layerContentParent.childCount - 1; i >= 0; i--)
        {
            GameObject child = layerContentParent.GetChild(i).gameObject;
            Button b = child.GetComponent<Button>();
            b.onClick.RemoveAllListeners();
            Destroy(child);
        }
        for (int i = activeLayerParent.childCount - 1; i >= 0; i--)
        {
            GameObject child = activeLayerParent.GetChild(i).gameObject;
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
        if(styledLayer.styles.Count is 0)
        {
            ApplyStyle(styledLayer, null);
            return;
        }
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
        System.Tuple<string, string> layerStyleKey;
        if(styleToApply == null)
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
        Button btn = dualText.GetComponent<Button>();

        btn.onClick.AddListener(() => DeactivateLayer(layerToStyle));
        btn.onClick.AddListener(() => dtcs.Remove(layerStyleKey));
        btn.onClick.AddListener(() => Destroy(btn.gameObject));

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
        if(ActivatedLayers.Count is 0)
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
        ActivatedLayers.Add(layerToActivate);
        ShowCRSOptions();
        return true;
    }

    private void DeactivateLayer(WMSLayer layerToDeactivate)
    {
        if (ActivatedLayers.Contains(layerToDeactivate))
        {
            ActivatedLayers.Remove(layerToDeactivate);
        }
    }
    private void ShowCRSOptions()
    {
        for (int i = crsContentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(crsContentParent.GetChild(i).gameObject);
        }
        foreach(string crs in activeCRSOptions)
        {
            Button b = Instantiate(layerButtonPrefab, crsContentParent);
            b.GetComponentInChildren<Text>().text = crs;
            b.onClick.AddListener(() => UrlReader.Instance.ActiveWMS.CRS = crs);
        }
    }
}
