using Netherlands3D.Events;
using UnityEngine;

public class WMSMapHandler : MonoBehaviour
{

    [SerializeField] private float updateInterval = 1f;
    [SerializeField] private Camera cam;
    [SerializeField] private WMSHandler handler;

    [Header("Invoked Events")]
    [SerializeField] private TriggerEvent requestWMSData;
    [SerializeField] private ObjectEvent mapImageEvent;
    [Header("Listen Events")]
    [SerializeField] private ObjectEvent wmsDataEvent;

    private bool cameraPositionMapUpdate = true;
    private float intervalTimer;

    private void Awake()
    {
        wmsDataEvent.AddListenerStarted(HandleMapPreview);
        if(cam == null)
            cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (cameraPositionMapUpdate && WMS.ActiveInstance != null)
        {

            intervalTimer += Time.deltaTime;
            if(intervalTimer >= updateInterval)
            {
                intervalTimer = 0;
                requestWMSData.InvokeStarted();
            }
        }
    }

    public void AutoUpdateMap(bool value)
    {
        cameraPositionMapUpdate = value;
    }

    private void HandleMapPreview(object wms)
    {
        WMS current = (WMS)wms;
        if (current.ActivatedLayers.Count == 0)
            return;
        if (cam != Camera.main)
        {
            cam.transform.position = Camera.main.transform.position;
        }
        Extent e = cam.GetRDExtent();
        current.BBox = new BoundingBox(e.MinX, e.MinY, e.MaxX, e.MaxY);
        current.IsPreview(true);
        handler.StartCoroutine(handler.DownloadImage(current.GetMapRequest(), mapImageEvent));
    }

}
