using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Netherlands3D.TileSystem;
using Netherlands3D.Core;

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
        /// Static raycasthit used by entities
        /// </summary>
        public static RaycastHit Hit;

        /// <summary>
        /// The data for the entity
        /// </summary>
        public Data Data { get { return data; } }

        [Header("Components")]
        [Tooltip("The default cube model in root > Model > Cube. Used to display the entity bounds")]
        [SerializeField] protected Transform defaultCubeModel;

        [Header("Scriptable Objects")]
        [SerializeField] protected EntityScriptableObjects so;

        /// <summary>
        /// Should the entity update itself in realtime?
        /// </summary>
        protected bool updateRealtime;
        /// <summary>
        /// The entity animation name
        /// </summary>
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
        /// The binary mesh layer to check raycast hits with
        /// </summary>
        /// <remarks>
        /// If left null it will not be used
        /// </remarks>
        protected BinaryMeshLayer binaryMeshLayer;
        /// <summary>
        /// The data of the entity
        /// </summary>
        protected Data data;
        /// <summary>
        /// The layermask to detect collisions with
        /// </summary>
        protected LayerMask layerMask;

        /// <summary>
        /// Initialize the entity
        /// </summary>
        /// <param name="data">Data of the entity</param>
        /// <param name="so">EntityScriptableObjects</param>
        public void Initialize(Data data, EntityScriptableObjects so, LayerMask layerMask, bool updateRealtime = false, BinaryMeshLayer binaryMeshLayer = null)
        {
            this.data = data;

            // So
            this.so = so;
            this.so.eventUpdateRealtime.started.AddListener(OnUpdateRealtimeChanged);
            this.so.eventSimulationTimeChanged.started.AddListener(OnSimulationTimeChanged);
            this.so.eventSimulationSpeedChanged.started.AddListener(OnSimulationSpeedChanged);
            this.so.eventSimulationStateChanged.started.AddListener(OnSimulationStateChanged);

            // Layer masks
            this.layerMask = layerMask;
            this.binaryMeshLayer = binaryMeshLayer;

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

            // Set default cube size
            if(defaultCubeModel != null)
            {
                defaultCubeModel.localScale = new Vector3(data.size.x, data.size.y, data.size.z);
                defaultCubeModel.localPosition = new Vector3(0, data.size.y * 0.5f, 0);
            }

            // Realtime
            this.updateRealtime = updateRealtime;

            UpdateNavigation();
        }

        protected virtual void OnEnable()
        {
            // If turned off and then turned back on update its values
            if(data != null)
            {
                OnSimulationTimeChanged(so.simulationTime.Value);
                OnSimulationSpeedChanged(so.simulationSpeed.Value);
                OnSimulationStateChanged(so.simulationState.Value);
            }
        }

        protected virtual void OnDisable()
        {
            if(data != null)
            {
                so.eventUpdateRealtime.started.RemoveListener(OnUpdateRealtimeChanged);
                so.eventSimulationTimeChanged.started.RemoveListener(OnSimulationTimeChanged);
                so.eventSimulationSpeedChanged.started.RemoveListener(OnSimulationSpeedChanged);
                so.eventSimulationStateChanged.started.RemoveListener(OnSimulationStateChanged);
            }
        }

        protected virtual void Awake()
        {
            animation = GetComponent<Animation>();
        }

        protected void LateUpdate()
        {
            UpdateNavigationRealtime();
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
            // Loop through coordinates backwards and calculate its correct y position
            // We need to loop backwards through the coordiantes because we need to get the correct height and
            // we use the height for rotation direction calculation
            float[] coordinatesKeys = data.coordinates.Keys.ToArray();
            for(int i = coordinatesKeys.Length - 1; i >= 0; i--)
            {
                float key = coordinatesKeys[i];
                float nextKey = i == coordinatesKeys.Length - 1 ? key : coordinatesKeys[i + 1];
                Data.Coordinates item = data.coordinates[key];

                // Check for a raycast with ground
                if(Physics.Raycast(item.center + new Vector3(0, 50, 0), Vector3.down, out Hit, Mathf.Infinity, layerMask))
                {
                    item.center.y = Hit.point.y;
                }
                else
                {
                    // Tell the binary mesh layer (if assigned) to add mesh colliders to the binary mesh layer to enable raycast collision
                    if(binaryMeshLayer != null)
                    {
                        binaryMeshLayer.AddMeshColliders(Hit.point);
                        // Cast the raycast again for a y axis point
                        if(Physics.Raycast(item.center + new Vector3(0, 50, 0), Vector3.down, out Hit, Mathf.Infinity, layerMask))
                        {
                            item.center.y = Hit.point.y;
                        }
                    }
                }

                // Add animation keyframe to clip
                // Position animation
                Vector3 position = new Vector3(item.center.x, item.center.y, item.center.z);
                animationCurvePositionX.AddKey(key, position.x);
                if(!updateRealtime) animationCurvePositionY.AddKey(key, position.y);
                animationCurvePositionZ.AddKey(key, position.z);

                // Rotation animation
                // Check if distance between next point is smaller than 1 meter (in case of center points not being correct/too close which causes visual rotation bugs)
                if(i == coordinatesKeys.Length - 1 || i == 0 || Vector3.Distance(data.coordinates[key].center, data.coordinates[nextKey].center) < 1) continue;

                item.direction = (data.coordinates[nextKey].center - data.coordinates[key].center).normalized;
                Quaternion q = Quaternion.LookRotation(item.direction, Vector3.up); //IMPROVE the rotation is not purpendicular to that of the raycast normal
                animationCurveRotationX.AddKey(key, q.x);
                animationCurveRotationY.AddKey(key, q.y);
                animationCurveRotationZ.AddKey(key, q.z);
                animationCurveRotationW.AddKey(key, q.w);
                
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
            if(!updateRealtime) animationClip.SetCurve("", typeof(Transform), "localPosition.y", animationCurvePositionY);
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
        /// Updates the navigation in realtime for height/rotation
        /// </summary>
        public virtual void UpdateNavigationRealtime()
        {
            if(!updateRealtime) return;

            // Y position
            // Check for a raycast with ground
            if(Physics.Raycast(transform.position + new Vector3(0, 50, 0), Vector3.down, out Hit, Mathf.Infinity, layerMask))
            {
                transform.position = new Vector3(transform.position.x, Hit.point.y + defaultCubeModel.localScale.y, transform.position.z);
            }
            else
            {
                // Tell the binary mesh layer (if assigned) to add mesh colliders to the binary mesh layer to enable raycast collision
                if(binaryMeshLayer != null)
                {
                    binaryMeshLayer.AddMeshColliders(Hit.point);
                    // Cast the raycast again for a y axis point
                    if(Physics.Raycast(transform.position + new Vector3(0, 50, 0), Vector3.down, out Hit, Mathf.Infinity, layerMask))
                    {
                        transform.position = new Vector3(transform.position.x, Hit.point.y + defaultCubeModel.localScale.y, transform.position.z);
                    }
                }
            }

            // Normal direction
            transform.rotation = Quaternion.FromToRotation(transform.up, Hit.normal) * transform.rotation;
        }

        protected virtual void OnUpdateRealtimeChanged(bool value)
        {
            updateRealtime = value;
            UpdateNavigation();
            OnSimulationTimeChanged(so.simulationTime.Value);
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
            switch(so.simulationState.Value)
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
                case 1: // Play
                    animation[animationName].speed = so.simulationSpeed.Value;
                    animation[animationName].time = so.simulationTime.Value;
                    animation.Play();
                    break;
                case 0: // Paused
                    animation[animationName].speed = 0;
                    break;
                case -1: // Reverse Play
                    animation[animationName].speed = -so.simulationSpeed.Value;
                    animation[animationName].time = so.simulationTime.Value;
                    animation.Play();
                    break;
                case -2: // Reset
                    animation[animationName].speed = 1;
                    animation[animationName].time = 0;
                    animation[animationName].speed = 0;
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
                    // Draw cube of position
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawCube(item.Value.center, Vector3.one);
                    // Draw line of direction
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(item.Value.center, item.Value.center + item.Value.direction * 2);
                }

                // Draw bounding box
                Matrix4x4 m = Gizmos.matrix;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireCube(new Vector3(0, data.size.y * 0.5f, 0), data.size);
                Gizmos.matrix = m;
            }
        }
#endif
    }
}
