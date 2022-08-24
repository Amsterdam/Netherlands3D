using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Traffic.Simulation
{
    /// <summary>
    /// A scriptable object containing road data
    /// </summary>
    [CreateAssetMenu(fileName = "Traffic Roads Data", menuName = "ScriptableObjects/Traffic/Roads Data", order = 1)]
    public class RoadsData : ScriptableObject
    {
        [Tooltip("Do you want the so values to be reset on unity runtime")]
        [SerializeField] private bool resetOnRuntime;

        [field: SerializeField]
        public List<Road> Roads { get; private set; }

        public DelegateAdd OnAdd;
        public DelegateAdd OnRemove;
        public delegate void DelegateAdd(List<Road> roads);
        public delegate void DelegateRemove(List<Road> roads);

        private void OnDisable()
        {
            if(resetOnRuntime) Reset();
        }

        public void Add(Road road)
        {
            Roads.Add(road);
            OnAdd?.Invoke(new List<Road> { road });
        }

        public void Add(List<Road> roads)
        {
            foreach(var item in roads)
            {
                Roads.Add(item);
            }
            OnAdd?.Invoke(roads);
        }

        public void Remove(Road road)
        {
            Roads.Remove(road);
            OnRemove?.Invoke(new List<Road> { road });
        }

        public void Remove(List<Road> roads)
        {
            foreach(var item in roads)
            {
                Roads.Remove(item);
            }
            OnRemove?.Invoke(roads);
        }

        private void Reset()
        {
            Roads = default;
        }
    }
}
