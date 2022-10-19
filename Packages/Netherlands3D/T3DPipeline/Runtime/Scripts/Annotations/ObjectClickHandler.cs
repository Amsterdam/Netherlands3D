using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Events;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class ObjectClickHandler : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    protected Vector3Event onObjectClicked;

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        var pos = eventData.pointerCurrentRaycast.worldPosition;
        onObjectClicked.Invoke(pos);
    }
}
