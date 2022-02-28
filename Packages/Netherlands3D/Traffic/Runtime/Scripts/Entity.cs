using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Netherlands3D.TileSystem;
using Netherlands3D.Events;

namespace Netherlands3D.Traffic
{
    /// <summary>
    /// Base class for all Traffic entities (example car, truck, bike etc)
    /// </summary>
    [AddComponentMenu("Traffic/Traffic Entity")] // Used to change the script inspector name
    [RequireComponent(typeof(Animation))]
    public class Entity : MonoBehaviour
    {
        /// <summary>
        /// The data for the entity
        /// </summary>
        public Data Data { get { return data; } }

        [Header("Components")]
        [Tooltip("The default cube model in root > Model > Cube. Used to display the entity bounds")]
        [SerializeField] protected Transform defaultCubeModel;

        [Header("Scriptable Objects")]
        [Tooltip("Event that gets triggerd if the simulation time changes")]
        [SerializeField] protected FloatEvent eventSimulationTimeChanged;
        [Tooltip("Event that gets triggerd if the simulation speed changes")]
        [SerializeField] protected FloatEvent eventSimulationSpeedChanged;
        [Tooltip("Event that gets triggerd if the simulation state changes")]
        [SerializeField] protected IntEvent eventSimulationStateChanged;
        [Tooltip("The current VISSIM simulation time starting from 0 - infinity")]
        [SerializeField] protected FloatVariable simulationTime;
        [Tooltip("The speed multiplier to how fast the simulation is running. 1 = normal")]
        [SerializeField] protected FloatVariable simulationSpeed;
        [Tooltip("The state of the simulation time and how it gets updated. 0 = paused, 1 = play, -1 = reversed, -2 = reset")]
        [SerializeField] protected IntVariable simulationState;

        protected readonly string animationName = "Movement";
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
        /// The animationCurve for the entitys rotation w
        /// </summary>
        protected AnimationCurve animationCurveRotationW;
        /// <summary>
        /// The animationCurve for the entitys model
        /// </summary>
        protected AnimationCurve animationCurveModel;
        /// <summary>
        /// The data of the entity
        /// </summary>
        protected Data data;

        public void Initialize(Data data, EntityScriptableObjects so)
        {
            this.data = data;

            // So
            eventSimulationTimeChanged = so.eventSimulationTimeChanged;
            eventSimulationSpeedChanged = so.eventSimulationSpeedChanged;
            eventSimulationStateChanged = so.eventSimulationStateChanged;
            simulationSpeed = so.simulationSpeed;
            simulationTime = so.simulationTime;
            simulationState = so.simulationState;

            eventSimulationTimeChanged.started.AddListener(OnSimulationTimeChanged);
            eventSimulationSpeedChanged.started.AddListener(OnSimulationSpeedChanged);
            eventSimulationStateChanged.started.AddListener(OnSimulationStateChanged);

            // Animation
            animationClip = new AnimationClip();
            animationClip.name = animationName;
            animationClip.legacy = true;
            animation.wrapMode = WrapMode.Clamp;
            animationCurvePositionX = new AnimationCurve();
            animationCurvePositionY = new AnimationCurve();
            animationCurvePositionZ = new AnimationCurve();
            animationCurveRotationX = new AnimationCurve();
            animationCurveRotationY = new AnimationCurve();
            animationCurveRotationZ = new AnimationCurve();
            animationCurveRotationW = new AnimationCurve();
            animationCurveModel = new AnimationCurve();

            if(defaultCubeModel != null)
            {
                defaultCubeModel.localScale = new Vector3(data.width, data.height, data.length);
                defaultCubeModel.localPosition = new Vector3(0, data.height * 0.5f, 0);
            }

            UpdateNavigation();
        }

        protected virtual void OnEnable()
        {
            // If turned off and then turned back on update its values
            if(data != null)
            {
                OnSimulationTimeChanged(simulationTime.Value);
                OnSimulationSpeedChanged(simulationSpeed.Value);
                OnSimulationStateChanged(simulationState.Value);
            }
        }

        protected virtual void OnDisable()
        {
            if(data != null)
            {
                eventSimulationTimeChanged.started.RemoveListener(OnSimulationTimeChanged);
                eventSimulationSpeedChanged.started.RemoveListener(OnSimulationSpeedChanged);
                eventSimulationStateChanged.started.RemoveListener(OnSimulationStateChanged);
            }
        }

        protected virtual void Awake()
        {
            animation = GetComponent<Animation>();
        }

        /// <summary>
        /// Update the entity data
        /// </summary>
        /// <param name="data">The new data</param>
        /// <param name="updateNavigation">When updating the data also call UpdateNavigation()</param>
        public virtual void UpdateData(Data data, bool updateNavigation = true)
        {
            this.data = data;
            if(updateNavigation) UpdateNavigation();
        }

        /// <summary>
        /// Move the entity based on its data
        /// </summary>
        public virtual void UpdateNavigation()
        {
            // Loop through coordinates and calculate its correct y position
            foreach(var item in data.coordinates)
            {
                // Check for a raycast with ground
                if(Physics.Raycast(item.Value.center + new Vector3(0, 50, 0), Vector3.down, out Visualizer.Hit)) //TODO add layermask?
                {
                    item.Value.center.y = Visualizer.Hit.point.y;
                }//TODO Add binary mesh layer collider

                // Add animation keyframe to clip
                // Position animation
                Vector3 position = new Vector3(item.Value.center.x, item.Value.center.y, item.Value.center.z);
                animationCurvePositionX.AddKey(item.Key, position.x);
                animationCurvePositionY.AddKey(item.Key, position.y);
                animationCurvePositionZ.AddKey(item.Key, position.z);

                // Rotation animation
                Quaternion q = Quaternion.LookRotation(item.Value.direction, Vector3.up);
                animationCurveRotationX.AddKey(item.Key, q.x);
                animationCurveRotationY.AddKey(item.Key, q.y);
                animationCurveRotationZ.AddKey(item.Key, q.z);
                animationCurveRotationW.AddKey(item.Key, q.w);
            }

            // Model
            animationCurveModel.AddKey(new Keyframe(0, 0)); // Turn off model at start of animation
            animationCurveModel.AddKey(new Keyframe(data.coordinates.First().Key - 0.01f, 0)); // Tell model to stay inactive just before the frame (since we cant use TangentMode.Constant)
            animationCurveModel.AddKey(new Keyframe(data.coordinates.First().Key, 1)); // Turn on model
            animationCurveModel.AddKey(new Keyframe(data.coordinates.Last().Key - 0.01f, 1)); // Keep model turned on just before end frame
            animationCurveModel.AddKey(new Keyframe(data.coordinates.Last().Key, 0)); // Turn off model at end of animation
            // Set the animation curve to constant so the model only appears on its first coordiante keyframe
            // https://docs.unity3d.com/ScriptReference/AnimationUtility.SetKeyLeftTangentMode.htmls
            //UnityEditor.AnimationUtility.SetKeyLeftTangentMode(animationCurveModel, keyFrameIndex, AnimationUtility.TangentMode.Constant); // Cant use this since UnityEditor gets removed in build

            // Set animation clip curve positions
            animationClip.SetCurve("", typeof(Transform), "localPosition.x", animationCurvePositionX);
            animationClip.SetCurve("", typeof(Transform), "localPosition.y", animationCurvePositionY);
            animationClip.SetCurve("", typeof(Transform), "localPosition.z", animationCurvePositionZ);

            animationClip.SetCurve("", typeof(Transform), "localRotation.x", animationCurveRotationX);
            animationClip.SetCurve("", typeof(Transform), "localRotation.y", animationCurveRotationY);
            animationClip.SetCurve("", typeof(Transform), "localRotation.z", animationCurveRotationZ);
            animationClip.SetCurve("", typeof(Transform), "localRotation.w", animationCurveRotationW);
            animationClip.EnsureQuaternionContinuity();

            animationClip.SetCurve("Model", typeof(GameObject), "m_IsActive", animationCurveModel);            

            animation.clip = animationClip;
            animation.AddClip(animationClip, animationClip.name);
            animation.Play();
        }

        /// <summary>
        /// Callback when the VISSIM.SimulationTime gets changed
        /// </summary>
        /// <param name="newTime"></param>
        protected virtual void OnSimulationTimeChanged(float newTime)
        {
            if(animationClip == null) return;
            animation[animationName].time = newTime;
        }

        /// <summary>
        /// Callback when the VISSIM.SimulationSpeed gets changed
        /// </summary>
        /// <param name="newSpeed"></param>
        protected virtual void OnSimulationSpeedChanged(float newSpeed)
        {
            if(animationClip == null) return;
            switch(simulationState.Value)
            {
                case 1:
                    animation[animationName].speed = newSpeed;
                    break;
                case 0:
                    break;
                case -1:
                    animation[animationName].speed = -newSpeed;
                    break;
                case -2:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Callback when the VISSIM.SimulationState gets changed
        /// </summary>
        /// <param name="newState"></param>
        protected virtual void OnSimulationStateChanged(int newState)
        {
            if(animationClip == null) return;
            switch(newState)
            {
                case 1:
                    //animation.clip = animationClip;
                    animation[animationName].speed = simulationSpeed.Value;
                    animation[animationName].time = simulationTime.Value;
                    animation.Play();
                    break;
                case 0:
                    animation[animationName].speed = 0;
                    break;
                case -1:
                    animation[animationName].speed = -simulationSpeed.Value;
                    animation[animationName].time = simulationTime.Value;
                    animation.Play();
                    //animation.Rewind();
                    break;
                case -2:
                    animation[animationName].time = 0;
                    animation.Stop();
                    break;
                default:
                    break;
            }
        }
        
#if UNITY_EDITOR
        public virtual void OnDrawGizmosSelected()
        {
            if(!Application.isPlaying || UnityEditor.Selection.activeGameObject != gameObject) return;

            if(data != null)
            {
                // Draw each data coordinate
                foreach(var item in data.coordinates)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawCube(item.Value.center, Vector3.one);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(item.Value.center, item.Value.center + item.Value.direction * 2);
                }

                // Draw bounding box
                Matrix4x4 m = Gizmos.matrix;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireCube(new Vector3(0, 0.5f, 0), new Vector3(data.width, data.height, data.length));
                Gizmos.matrix = m;
            }
        }
#endif
    }
}
