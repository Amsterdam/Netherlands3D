using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Events;
using UnityEngine;

public class AnnotationMarker : MonoBehaviour
{
    [SerializeField]
    private TriggerEvent onAnnotationSubmitted;
    [SerializeField]
    private Material activeMaterial, inactiveMaterial;

    private void OnEnable()
    {
        onAnnotationSubmitted.AddListenerStarted(DeactivateMarker);
    }

    private void OnDisable()
    {
        onAnnotationSubmitted.RemoveListenerStarted(DeactivateMarker);
    }

    //public virtual void ActivateMarker(GameObject activatedAnnotationMarker)
    //{
    //    if (activatedAnnotationMarker == gameObject)
    //        GetComponent<MeshRenderer>().material = activeMaterial;
    //}

    public virtual void DeactivateMarker()
    {
        GetComponent<MeshRenderer>().material = inactiveMaterial;
    }
}
