using Netherlands3D.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Coordinates;
using UnityEngine;
using TMPro;
using Netherlands3D.Core;

namespace Netherlands3D.ProfileRendering {
    public class DrawProfileHeightLines : MonoBehaviour
    {
        [SerializeField] private FloatEvent sizeInWorld;
        [SerializeField] private TextMeshProUGUI widthText;
        [SerializeField] private float startHeight = 100;
        void Awake()
        {
            if(sizeInWorld)
                sizeInWorld.AddListenerStarted(SetNumbersByNewHeight);
            SetNumbersByNewHeight(startHeight);
        }

        //Sets height lines numbers ( child objects with TMP_Text on it )
        public void SetNumbersByNewHeight(float newHeight)
        {
            widthText.text = newHeight.ToString("F0") + "m";

            int childLines = transform.childCount;
            float lineHeight = newHeight / 2;
            float stepSize = newHeight / (childLines-1);
            for (int i = 0; i < childLines; i++)
            {
                var childText = transform.GetChild(i).GetComponentInChildren<TextMeshProUGUI>();
                var heightText = (lineHeight + CoordinateConverter.zeroGroundLevelY).ToString("F0");
                childText.text = (heightText=="0") ? "NAP": heightText+"m";

                lineHeight -= stepSize;
            }
        }
    }
}
