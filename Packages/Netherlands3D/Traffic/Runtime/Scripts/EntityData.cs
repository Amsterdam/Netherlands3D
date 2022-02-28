using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.Traffic
{
    /// <summary>
    /// An scriptable object containing data for its VISSIM entity (Example a human, car, truck, bus etc.)
    /// </summary>
    [CreateAssetMenu(fileName = "Traffic Entity Data", menuName = "ScriptableObjects/Traffic/Entity Data", order = 1)]
    public class EntityData : ScriptableObject
    {
        [Tooltip("The corresponding key value of this entity. 100 = Car; 200 = Truck; 300 = Bus; 400 = Tram; 500 = Pedestrian; 600 = Cycle; 700 = Van;")]
        /// <summary>
        /// The corresponding key value of this entity
        /// </summary>
        /// <remarks>
        /// 100 = Car; 200 = Truck; 300 = Bus; 400 = Tram; 500 = Pedestrian; 600 = Cycle; 700 = Van;
        /// </remarks>
        public int id;
        [Tooltip("The average hight in meters of the entity to be used if it has no prefabEntity")]
        public float averageHeight = 1;
        [Tooltip("The name to be displayed to user")]
        public string displayName;
        [Tooltip("The sprite to be displayed to user")]
        public Sprite sprite;
        [Tooltip("The prefab of the entity (with entity component)")]
        public GameObject prefabEntity;
    }
}
