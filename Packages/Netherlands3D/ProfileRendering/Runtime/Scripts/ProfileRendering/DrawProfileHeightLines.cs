using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Netherlands3D.Core;

namespace Netherlands3D.ProfileRendering { 
    public class DrawProfileHeightLines : MonoBehaviour
    {
        [SerializeField] private FloatEvent sizeInWorld;
        [SerializeField] private float startHeight = 100;
        void Awake()
        {
            sizeInWorld.AddListenerStarted(SetNumbersByNewHeight);
            SetNumbersByNewHeight(startHeight);
        }

        //Set height lines numbers ( child objects with TMP_Text on it )
        private void SetNumbersByNewHeight(float newHeight)
        {
            int childLines = transform.childCount;
            float lineHeight = newHeight / 2;
            float stepSize = newHeight / (childLines-1);
            for (int i = 0; i < childLines; i++)
            {
                var childText = transform.GetChild(i).GetComponentInChildren<TextMeshProUGUI>();
                var heightText = (lineHeight + CoordConvert.zeroGroundLevelY).ToString("F0");
                childText.text = (heightText=="0") ? "NAP": heightText+"m";
                
                lineHeight -= stepSize;
            }
        }
    }
}