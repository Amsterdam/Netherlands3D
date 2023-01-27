using Netherlands3D.Core;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Netherlands3D.Core
{
    /// <summary>
    /// This MonoBehaviour simply applies the two static CoordConvert values that
    /// determine the RD coordinates of the Unity scene center point (0,0,0).
    /// </summary>
    public class SetGlobalRDOrigin : MonoBehaviour
    {
        [SerializeField]
        private float zeroGroundLevelY = 0;
        [SerializeField]
        private Vector2RD relativeCenterRD = new Vector2RD(121000, 487000);

        [Tooltip("Forces standard culture for parsing/deserializing numbers"),SerializeField] private bool setInvariantCultureInfo = true;
        void Awake()
        {
            if(setInvariantCultureInfo)
            {
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            }

            CoordConvert.zeroGroundLevelY = zeroGroundLevelY;
            CoordConvert.relativeCenterRD = relativeCenterRD;
            
            CoordConvert.ecefIsSet = false;

            Vector3WGS origin_wgs = CoordConvert.UnitytoWGS84(Vector3.zero);
            

        }
    }
}