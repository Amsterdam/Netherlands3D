using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Events;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D.T3DPipeline
{
    /// <summary>
    /// Generic class to process a pointer click and fires the event with the 3D click position. Requires a PhysicsRaycaster to work
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ObjectClickHandler : MonoBehaviour, IPointerClickHandler
    {
        [Tooltip("Event that is called when the object is clicked")]
        [SerializeField]
        protected Vector3Event objectClicked;

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            var pos = eventData.pointerCurrentRaycast.worldPosition;
            objectClicked.InvokeStarted(pos);
        }
    }
}
