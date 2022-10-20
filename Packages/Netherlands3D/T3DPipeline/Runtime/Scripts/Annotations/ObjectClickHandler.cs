using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Events;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class ObjectClickHandler : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("Event that is called when the object is clicked")]
    [SerializeField]
    protected Vector3Event objectClicked;

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        var pos = eventData.pointerCurrentRaycast.worldPosition;
        objectClicked.Invoke(pos);
    }
}
