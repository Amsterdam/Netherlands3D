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

        [HideInInspector] public new Camera camera;

        private void Awake()
        {
            camera = GetComponent<Camera>();
        }

        private void FixedUpdate()
        {
            Follow();
        }

        public void SetTarget(Transform t)
        {
            if(t != null)
            {
                target = t;
                transform.SetParent(target);
                transform.localPosition = offset;
                if(camera != Camera.main)
                    camera.depth = Camera.main.depth + 1; //TODO improve this with a better switch
            }
            else
            {
                camera.depth = -10;
            }
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
