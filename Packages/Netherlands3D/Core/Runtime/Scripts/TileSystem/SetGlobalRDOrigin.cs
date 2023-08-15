using Netherlands3D.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Netherlands3D.Coordinates;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Netherlands3D.Core
{
    /// <summary>
    /// This MonoBehaviour simply applies the two static CoordConvert values that
    /// determine the RD coordinates of the Unity scene center point (0,0,0).
    /// </summary>
    public class SetGlobalRDOrigin : MonoBehaviour
    {
        [SerializeField] private float zeroGroundLevelY = 0;
        [SerializeField] private Vector2RD relativeCenterRD = new Vector2RD(121000, 487000);

        [Tooltip("Forces standard culture for parsing/deserializing numbers"), SerializeField] private bool setInvariantCultureInfo = true;

        [Header("Move origin options")]
        [SerializeField]
        private bool movingOrigin = false;
        [SerializeField]
        private float maxCameraDistanceFromOrigin = 5000;
        public bool MovingOrigin {
            get => movingOrigin;
            set
            {
                if(runningCameraDistanceCheck != null)
                {
                    StopCoroutine(runningCameraDistanceCheck);
                    runningCameraDistanceCheck = null;
                }

                if(value == true)
                    runningCameraDistanceCheck = StartCoroutine(MaxCameraDistance());

                movingOrigin = value;
            }
        }

        private Coroutine runningCameraDistanceCheck;

        void Awake()
        {
            if(setInvariantCultureInfo)
            {
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            }

            CoordinateConverter.zeroGroundLevelY = zeroGroundLevelY;
            CoordinateConverter.relativeCenterRD = relativeCenterRD;
        }

        private void OnValidate()
        {
            CoordinateConverter.zeroGroundLevelY = zeroGroundLevelY;
            CoordinateConverter.relativeCenterRD = relativeCenterRD;

            Vector3WGS origin_wgs = CoordinateConverter.UnitytoWGS84(Vector3.zero);

            MovingOrigin = movingOrigin;
        }

        private IEnumerator MaxCameraDistance()
        {
            while (MovingOrigin)
            {
                var offset = Camera.main.transform.position;
                offset.y = 0;
                if (offset.magnitude > maxCameraDistanceFromOrigin)
                {
                    Coordinates.MovingOrigin.MoveAndRotateWorld(offset);
                }
                yield return new WaitForEndOfFrame();
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Draw the RD origin in our viewport center
        /// </summary>
        void OnDrawGizmosSelected()
        {
            Handles.color = Color.yellow;
            Handles.Label(Vector3.zero, $"    RD: {CoordinateConverter.relativeCenterRD.x},{CoordinateConverter.relativeCenterRD.y}");
        }
#endif
    }
}
