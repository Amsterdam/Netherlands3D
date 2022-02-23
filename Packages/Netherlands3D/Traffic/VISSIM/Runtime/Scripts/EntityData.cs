using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Netherlands3D.VISSIM
{
    /// <summary>
    /// An scriptable object containing data for its VISSIM entity (Example a human, car, truck, bus etc.)
    /// </summary>
    [CreateAssetMenu(fileName = "VISSIM Entity", menuName = "ScriptableObjects/VISSIM Entity Data", order = 1)]
    public class EntityData : ScriptableObject
    {
        [Tooltip("The corresponding key value of this entity. 100 = Car; 200 = Truck; 300 = Bus; 400 = Tram; 500 = Pedestrian; 600 = Cycle; 700 = Van;")]
        /// <summary>
        /// The corresponding key value of this entity
        /// </summary>
        /// <remarks>
        /// 100 = Car; 200 = Truck; 300 = Bus; 400 = Tram; 500 = Pedestrian; 600 = Cycle; 700 = Van;
        /// </remarks>
        public int id; //TODO make this a dropdown menu for user
        [Tooltip("The name to be displayed to user")]
        public string displayName;
        [Tooltip("The sprite to be displayed to user")]
        public Sprite sprite;
        [Tooltip("The prefab of the entity (with entity component)")]
        public GameObject prefabEntity;
    }
}
