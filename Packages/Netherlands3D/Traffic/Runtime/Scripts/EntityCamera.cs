using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Traffic
{
    /// <summary>
    /// Camera for an entity
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class EntityCamera : MonoBehaviour
    {
        public Vector3 offset = new Vector3(0, 15, -30);

        [Tooltip("The target of the camera to follow")]
        public Transform target;

        private void FixedUpdate()
        {
            Follow();
        }

        public void SetTarget(Transform t)
        {
            target = t;
            transform.SetParent(target);
            transform.localPosition = offset;
            GetComponent<Camera>().depth = Camera.main.depth + 1; //TODO improve this with a better switch
        }

        private void Follow()
        {
            if(target == null) return;
            transform.LookAt(target);
        }

        private void OnValidate()
        {
            if(target != null && transform.parent != target)
            {
                SetTarget(target);
            }
        }
    }
}
