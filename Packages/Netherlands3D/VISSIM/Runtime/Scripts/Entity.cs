using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.TileSystem;

namespace Netherlands3D.VISSIM
{
    /// <summary>
    /// Base class for all VISSIM entities (example car, truck, bike etc)
    /// </summary>
    public class Entity : MonoBehaviour
    {
        /// <summary>
        /// The data for the entity
        /// </summary>
        public Data data;

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

        /// <summary>
        /// Move the entity based on its data
        /// </summary>
        public void UpdateNavigation()
        {
            // Calculate the right y axis coordinate for coordinatesFront
            if(Physics.Raycast(transform.position + Vector3.up * 50, Vector3.down, out Visualizer.Hit))
            {
                data.coordinatesFront.y = Visualizer.Hit.point.y;
            }


        }
    }
}
