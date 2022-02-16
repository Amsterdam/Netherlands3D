using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.TileSystem;

namespace Netherlands3D.VISSIM
{
    /// <summary>
    /// Base class for all VISSIM entities (example car, truck, bike etc)
    /// </summary>
    [AddComponentMenu("VISSIM/VISSIM Entity")] // Used to change the script inspector name
    [RequireComponent(typeof(Animation))]
    public class Entity : MonoBehaviour
    {
        /// <summary>
        /// The data for the entity
        /// </summary>
        public Data Data { get { return data; } }

#pragma warning disable CS0108 // Member hides inherited member; missing new keyword (Unity deprecated)
        /// <summary>
        /// The animation component
        /// </summary>
        protected Animation animation;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
        /// <summary>
        /// The animation clip of the entity where its movement is in stored to animatie
        /// </summary>
        protected AnimationClip animationClip;

        protected Data data;

        public void Initialize(Data data)
        {
            this.data = data;
        }

        protected virtual void Awake()
        {
            animation = GetComponent<Animation>();
        }

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void UpdateData(Data data, bool updateNavigation = true)
        {
            this.data = data;
            if(updateNavigation) UpdateNavigation();
        }

        /// <summary>
        /// Move the entity based on its data
        /// </summary>
        public void UpdateNavigation()
        {
            // Loop through coordinates and calculate its correct y position
            foreach(var item in data.coordinates)
            {
                if(Physics.Raycast(item.Value.center + new Vector3(0, 50, 0), Vector3.down, out Visualizer.Hit))
                {
                    item.Value.center.y = Visualizer.Hit.point.y;
                }
            }


        }
    }
}
