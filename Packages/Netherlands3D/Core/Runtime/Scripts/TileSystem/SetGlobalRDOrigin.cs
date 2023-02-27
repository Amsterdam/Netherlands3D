using Netherlands3D.Core;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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

        [Tooltip("Forces standard culture for parsing/deserializing numbers"),SerializeField] private bool setInvariantCultureInfo = true;
        void Awake()
        {
            if(setInvariantCultureInfo)
            {
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            }

            CoordConvert.zeroGroundLevelY = zeroGroundLevelY;
            CoordConvert.relativeCenterRD = relativeCenterRD;
        }

        private void OnValidate()
        {
            CoordConvert.zeroGroundLevelY = zeroGroundLevelY;
            CoordConvert.relativeCenterRD = relativeCenterRD;
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