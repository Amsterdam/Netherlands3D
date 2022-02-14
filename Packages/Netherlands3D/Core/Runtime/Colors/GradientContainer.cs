using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Core.Colors
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/GradientContainer", order = 1)]
    public class GradientContainer : ScriptableObject
    {
        public Gradient gradient;
        private Texture2D gradientTexture;

        // Update is called once per frame
        public Texture2D GetGradientTexture(int width)
        {
            if (gradientTexture == null)
            {
                gradientTexture = new Texture2D(width, 1);
                Color[] colors = new Color[gradientTexture.width * gradientTexture.height];
                for (int i = 0; i < colors.Length; i++)
                {
                    float gradientStep = (float)i / (float)colors.Length;
                    var colorInGradient = gradient.Evaluate(gradientStep);
                    colors[i] = colorInGradient;
                }
                gradientTexture.SetPixels(colors);
                gradientTexture.Apply();
            }
            return gradientTexture;
        }

        public void ClearTexture()
        {
            Destroy(gradientTexture);
        }
    }
}