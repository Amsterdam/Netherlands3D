using Netherlands3D.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Minimap
{
    /// <summary>
    /// A minimap configuration
    /// </summary>
    [CreateAssetMenu(fileName = "Minimap Configuration", menuName = "Netherlands3D/Minimap/Configuration")]
    public class Configuration : ScriptableObject
    {
        [Tooltip("The url where to get the data from")]
        public string serviceUrl;
        [Tooltip("The size of a tile")]
        public float tileSize = 256;
        [Tooltip("The amount of pixels in a meter")]
        public double pixelsInMeter;
        [Tooltip("The scale of the minimap?")]
        public double scaleDenominator;
        [Tooltip("The top left x/y positions of the minimap")]
        public Vector2RD minimapTopLeft;
        [Tooltip("The map top right RD position")]
        public Vector2RD topRight;
        [Tooltip("The map bottom left RD position")]
        public Vector2RD bottomLeft;
        [Tooltip("The origin alligment of the map")]
        public OriginAlignment alignment;
        
        public enum OriginAlignment
        {
            topLeft,
            bottomLeft
        }
    }
}
