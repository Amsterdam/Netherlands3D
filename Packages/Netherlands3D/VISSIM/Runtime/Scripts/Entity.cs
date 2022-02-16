using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        /// <summary>
        /// The animationCurve for the entitys position x
        /// </summary>
        protected AnimationCurve animationCurvePositionX;
        /// <summary>
        /// The animationCurve for the entitys position y
        /// </summary>
        protected AnimationCurve animationCurvePositionY;
        /// <summary>
        /// The animationCurve for the entitys position z
        /// </summary>
        protected AnimationCurve animationCurvePositionZ;
        /// <summary>
        /// The animationCurve for the entitys rotation x
        /// </summary>
        protected AnimationCurve animationCurveRotationX;
        /// <summary>
        /// The animationCurve for the entitys rotation y
        /// </summary>
        protected AnimationCurve animationCurveRotationY;
        /// <summary>
        /// The animationCurve for the entitys rotation z
        /// </summary>
        protected AnimationCurve animationCurveRotationZ;
        /// <summary>
        /// The data of the entity
        /// </summary>
        protected Data data;

        public void Initialize(Data data)
        {
            this.data = data;
            animationClip = new AnimationClip();
            animationClip.name = "Movement";
            animationClip.legacy = true;
            animation.wrapMode = WrapMode.Loop;

            animationCurvePositionX = new AnimationCurve();
            animationCurvePositionY = new AnimationCurve();
            animationCurvePositionZ = new AnimationCurve();
            animationCurveRotationX = new AnimationCurve();
            animationCurveRotationY = new AnimationCurve();
            animationCurveRotationZ = new AnimationCurve();

            UpdateNavigation();
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
            float[] keys = data.coordinates.Keys.ToArray(); // For getting the next dictionary key
            int keyIndex = 0;
            foreach(var item in data.coordinates)
            {
                // Check for a raycast with ground
                if(Physics.Raycast(item.Value.center + new Vector3(0, 50, 0), Vector3.down, out Visualizer.Hit)) //TODO add layermask?
                {
                    item.Value.center.y = Visualizer.Hit.point.y;
                }

                // Add animation keyframe to clip
                // Position animation
                Vector3 position = new Vector3(item.Value.center.x, item.Value.center.y, item.Value.center.z);
                animationCurvePositionX.AddKey(item.Key, position.x);
                animationCurvePositionY.AddKey(item.Key, position.y);
                animationCurvePositionZ.AddKey(item.Key, position.z);

                // Rotation animation
                // Rotate the entity towords its next data.coordinates point if the next point exists
                if(keyIndex < keys.Length - 1)
                {
                    Vector3 nextPosition = data.coordinates[keys[keyIndex + 1]].center;
                    nextPosition.y = position.y;
                    Vector3 targetDir = nextPosition - position;
                    Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, 999, 0.0f);
                    Quaternion q = Quaternion.LookRotation(newDir);
                    animationCurveRotationX.AddKey(item.Key, q.eulerAngles.x);
                    animationCurveRotationY.AddKey(item.Key, q.eulerAngles.y);
                    animationCurveRotationZ.AddKey(item.Key, q.eulerAngles.z);
                }

                keyIndex++;
            }

            // Set animation clip curve positions
            animationClip.SetCurve("", typeof(Transform), "localPosition.x", animationCurvePositionX);
            animationClip.SetCurve("", typeof(Transform), "localPosition.y", animationCurvePositionY);
            animationClip.SetCurve("", typeof(Transform), "localPosition.z", animationCurvePositionZ);
            animationClip.SetCurve("", typeof(Transform), "localRotation.x", animationCurveRotationX);
            animationClip.SetCurve("", typeof(Transform), "localRotation.y", animationCurveRotationY);
            animationClip.SetCurve("", typeof(Transform), "localRotation.z", animationCurveRotationZ);

            animation.clip = animationClip;
            animation.AddClip(animationClip, animationClip.name);
            animation.Play();
        }

        private void OnDrawGizmosSelected()
        {
            if(!VISSIMManager.VisualizeGizmosDataPoints) return;

            // Draw each data coordinate
            Gizmos.color = Color.blue;
            if(data != null)
            {
                foreach(var item in data.coordinates)
                {
                    Gizmos.DrawCube(item.Value.center, Vector3.one);
                }
            }
        }
    }
}
