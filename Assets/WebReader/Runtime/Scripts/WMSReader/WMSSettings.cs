using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class WMSSettings
{
    private List<WMSLayer> activatedLayers = new();

    public void ActivateLayer(WMSLayer layerToActivate)
    {
        activatedLayers.Add(layerToActivate);
    }

    public void DeactivateLayer(WMSLayer layerToDeactivate)
    {
        if (activatedLayers.Contains(layerToDeactivate))
        {
            activatedLayers.Remove(layerToDeactivate);
        }
    }

    public string BuildWMSRequest()
    {
        StringBuilder layerBuilder = new StringBuilder();
        layerBuilder.Append("LAYERS=");

        StringBuilder styleBuilder = new StringBuilder();
        styleBuilder.Append("STYLES=");

        for(int i = 0; i < activatedLayers.Count; i++)
        {
            WMSLayer current = activatedLayers[i];
            if(current.activeStyle == null)
            {
                throw new System.NullReferenceException($"Layer: {current.Title} has no active style selected and cannot have the request finished!");
            }
            layerBuilder.Append(current.Name);
            styleBuilder.Append(current.activeStyle.Name);
            if(i != activatedLayers.Count - 1)
            {
                layerBuilder.Append(",");
                styleBuilder.Append(",");
            }
        }
        string request = layerBuilder + "&" + styleBuilder;
        Debug.Log(request);
        return request;
    }

}
