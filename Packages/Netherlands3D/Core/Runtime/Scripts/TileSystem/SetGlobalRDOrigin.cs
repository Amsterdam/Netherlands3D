using Netherlands3D.Core;
using System.Collections;
using System.Collections.Generic;
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
        void Awake()
        {
            CoordConvert.zeroGroundLevelY = zeroGroundLevelY;
            CoordConvert.relativeCenterRD = relativeCenterRD;
        }
    }
}