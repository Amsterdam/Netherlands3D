using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Core.Colors
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/GradientsGroup", order = 1)]
    public class GradientsGroup : ScriptableObject
    {
        public string description = "";
        public GradientContainer[] gradientContainers;
    }
}