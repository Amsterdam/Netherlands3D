using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Events;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class ObjectClickHandler : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Vector3Event onObjectClicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        var pos = eventData.pointerCurrentRaycast.worldPosition;
        onObjectClicked.Invoke(pos);
    }
}
