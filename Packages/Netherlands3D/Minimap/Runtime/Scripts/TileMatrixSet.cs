using Netherlands3D.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Minimap
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/TileMatrixSet", order = 0)]
    [System.Serializable]
    public class TileMatrixSet : ScriptableObject
    {
        public enum OriginAlignment
        {
            TopLeft,
            BottomLeft
        }
        public int TileSize = 256;
        public OriginAlignment minimapOriginAlignment = OriginAlignment.TopLeft;
        public Vector2RD Origin = new Vector2RD(-285401.92, 903401.92);
        public double PixelInMeters = 0.00028;
        public double ScaleDenominator = 12288000;
    }
}