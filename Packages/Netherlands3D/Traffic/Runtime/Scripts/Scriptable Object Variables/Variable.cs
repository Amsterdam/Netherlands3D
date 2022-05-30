using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Traffic
{
    public abstract class Variable<T> : ScriptableObject
    {
        [Tooltip("Do you want the so values to be reset on unity runtime")]
        [SerializeField] private bool resetOnRuntime = true;

        private void OnDisable()
        {
            if(resetOnRuntime) Reset();
        }

        public abstract void Reset();
    }
}
