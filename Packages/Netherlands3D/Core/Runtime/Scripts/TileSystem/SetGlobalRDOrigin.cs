using Netherlands3D.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

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
        private Vector3ECEF centerECEF;

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
                

               

                movingOrigin = value;
            }
        }

        public UnityEvent prepareForOriginShift = new UnityEvent();
        public  CenterChangedEvent relativeOriginChanged = new CenterChangedEvent();
        private Coroutine runningCameraDistanceCheck;

        void Awake()
        {
            

            if (setInvariantCultureInfo)
            {
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            }

            CoordConvert.zeroGroundLevelY = zeroGroundLevelY;
            CoordConvert.relativeCenterRD = relativeCenterRD;
        }

        private void OnDisable()
        {
            if (runningCameraDistanceCheck != null)
            {
                StopCoroutine(runningCameraDistanceCheck);
                runningCameraDistanceCheck = null;
            }
        }

        private void OnValidate()
        {
            CoordConvert.zeroGroundLevelY = zeroGroundLevelY;
            CoordConvert.relativeCenterRD = relativeCenterRD;
            

            Vector3WGS origin_wgs = CoordConvert.UnitytoWGS84(Vector3.zero);

            MovingOrigin = movingOrigin;
        }

        private void MaxCameraDistance()
        {
          
                var offset = Camera.main.transform.position;
                offset.y = 0;
                if (offset.magnitude > maxCameraDistanceFromOrigin)
                {
                    MoveAndRotateWorld(offset);
                    
                    //Camera.main.transform.position = Camera.main.transform.position - offset;
                }

                
               
            
        }

        public static bool ecefIsSet;
        
        public class CenterChangedEvent : UnityEvent<Vector3> { }
        

        public  void MoveAndRotateWorld(Vector3 cameraPosition)
        {

            prepareForOriginShift.Invoke();

            var flatCameraPosition = new Vector3(cameraPosition.x, 0, cameraPosition.z);
            Vector3ECEF newECEFOrigin = CoordConvert.UnityToECEF(flatCameraPosition);
            CoordConvert.relativeCenterECEF = newECEFOrigin;
            //var newWGS84 = CoordConvert.ECEFtoWGS84(newECEFOrigin);
            //var newRD = CoordConvert.WGS84toRD(newWGS84.lon,newWGS84.lat);
            //CoordConvert.relativeCenterRD = new Vector2RD(newRD.x, newRD.y);

            var offset = new Vector3(-cameraPosition.x, 0, -cameraPosition.z);

            relativeOriginChanged.Invoke(offset);
        }


        private void Update()
        {
           if(MovingOrigin )
            {
                MaxCameraDistance();
            }
        }
#if UNITY_EDITOR
        /// <summary>
        /// Draw the RD origin in our viewport center
        /// </summary>
        void OnDrawGizmosSelected()
        {
            Handles.color = Color.yellow;
            Handles.Label(Vector3.zero, $"    RD: {CoordConvert.relativeCenterRD.x},{CoordConvert.relativeCenterRD.y}");
        }
#endif
    }
}
