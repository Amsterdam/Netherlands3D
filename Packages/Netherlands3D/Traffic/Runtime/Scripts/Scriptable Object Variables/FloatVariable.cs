using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Traffic
{
    [CreateAssetMenu(fileName = "Float Variable", menuName = "ScriptableObjects/Traffic/Float Variable", order = 1)]
    public class FloatVariable : ScriptableObject
    {
        public float Value;
    }
}