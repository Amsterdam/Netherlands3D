using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
using Netherlands3D.TileSystem;


public class CreateWFSLayer : MonoBehaviour
{
    [HideInInspector] public List<WFSGeoLayer> geoLayers = new();

    private TileHandler tileHandler;

    [SerializeField] private int tileSize = 1500;

    [Header("Events")]
    [SerializeField] private StringEvent onWfsUrlDefined_String;
    [SerializeField] private ObjectEvent unloadWfs;
    [SerializeField] private GameObjectEvent wfsGeoParentEvent;


    private void Awake()
    {
        tileHandler = FindObjectOfType<TileHandler>();
        if(onWfsUrlDefined_String)
        {
            onWfsUrlDefined_String.AddListenerStarted(CreateWebFeatureLayer);
        }
    }

    private void CreateWebFeatureLayer(string baseUrl)
    {
        Debug.Log("Creating a WFS layer");
        Debug.LogWarning("Creating WFS Layer not yet implemented!");
        if (wfsGeoParentEvent) wfsGeoParentEvent.Invoke(new GameObject());

    }

}
