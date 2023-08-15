using Netherlands3D.Core;
using System.Collections;
using System.Collections.Generic;
using Netherlands3D.Coordinates;
using UnityEngine;

namespace Netherlands3D.Minimap
{
    /// <summary>
    /// Contains data used for the minimap
    /// </summary>
    [CreateAssetMenu(fileName = "Tile Matrix Set", menuName = "ScriptableObjects/Minimap/TileMatrixSet", order = 0)]
    [System.Serializable]
    public class TileMatrixSet : ScriptableObject
    {
        [Tooltip("The size of 1 tile")]
        public int TileSize = 256;
        [Tooltip("The pixels in a meter")]
        public double PixelInMeters = 0.00028;
        [Tooltip("Minimap scale denominator")]
        public double ScaleDenominator = 12288000;
        [Tooltip("The origin of the minimap")]
        public Vector2RD Origin = new Vector2RD(-285401.92, 903401.92);
        [Tooltip("The alignment of the minimap")]
        public OriginAlignment minimapOriginAlignment = OriginAlignment.TopLeft;

        public enum OriginAlignment
        {
            TopLeft,
            BottomLeft
        }
    }
}