using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Traffic
{
    [CreateAssetMenu(fileName = "Dictionary Variable", menuName = "ScriptableObjects/Traffic/Dictionary Variable", order = 1)]
    public class DictionaryVariable<T0, T1> : ScriptableObject
    {
        public Dictionary<T0, T1> Value;
    }
}
