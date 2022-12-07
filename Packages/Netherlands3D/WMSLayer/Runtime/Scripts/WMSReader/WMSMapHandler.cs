using Netherlands3D.Core;
using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WMSMapHandler : MonoBehaviour
{

    [SerializeField] private float updateInterval = 1f;

    [SerializeField] private Camera cam;

    private bool cameraPositionMapUpdate = true;
    private float intervalTimer;

    [Header("Invoked Events")]
    [SerializeField] private ObjectEvent extentEvent;

    private void Awake()
    {
        if(cam == null)
            cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (cameraPositionMapUpdate)
        {
            intervalTimer += Time.deltaTime;
            if(cam != Camera.main)
            {
                cam.transform.position = Camera.main.transform.position;
            }
            if(intervalTimer >= updateInterval)
            {
                intervalTimer = 0;
                Extent e = cam.GetRDExtent();
                Debug.Log(e.MinX);
                Debug.Log(e.MaxX); Debug.Log(e.MinY); Debug.Log(e.MaxY);
                //extentEvent.started.Invoke(e);
                WMS.ActiveInstance.BBox = new BoundingBox((float)e.MinX, (float)e.MinY, (float)e.MaxX, (float)e.MaxY);
                WebServiceNetworker.Instance.SendRequest(true);
            }
        }
    }

    public void AutoUpdateMap(bool value)
    {
        cameraPositionMapUpdate = value;
    }
}
