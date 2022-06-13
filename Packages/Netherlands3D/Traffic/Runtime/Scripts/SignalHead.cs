using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
using System;

namespace Netherlands3D.Traffic.VISSIM
{
    [AddComponentMenu("Netherlands3D/Traffic/VISSIM/Signal Head")]
    public class SignalHead : MonoBehaviour
    {
        public SignalHeadData data;

        [Header("Events")]
        public SSO sso;// <--------------------------------------------------------------------------------------

        [Header("Components")]
        public Transform modelChild;
        public MeshRenderer meshRenderer;
        public Material materialRed; // Color index 0
        public Material materialAmber; // Color index 1
        public Material materialGreen; // Color index 2

        // Animation
        private readonly string animationName = "Behavior";
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword (Unity deprecated)
        /// <summary>
        /// The animation component
        /// </summary>
        private Animation animation;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
        /// <summary>
        /// The animation clip of the entity where its movement is in stored to animatie
        /// </summary>
        private AnimationClip animationClip;
        private AnimationCurve animationCurve;

        private void OnEnable()
        {
            // If turned off and then turned back on update its values
            if(data != null)
            {
                OnSimulationTimeChanged(sso.simulationTime.Value);
                OnSimulationSpeedChanged(sso.simulationSpeed.Value);
                OnSimulationStateChanged(sso.simulationState.Value);
            }
        }

        private void OnDisable()
        {
            if(data != null)
            {
                sso.eventUpdateRealtime.started.RemoveListener(OnUpdateRealtimeChanged);
                sso.eventSimulationTimeChanged.started.RemoveListener(OnSimulationTimeChanged);
                sso.eventSimulationSpeedChanged.started.RemoveListener(OnSimulationSpeedChanged);
                sso.eventSimulationStateChanged.started.RemoveListener(OnSimulationStateChanged);
            }
        }

        private void Awake()
        {
            animation = GetComponent<Animation>();
        }

        public void Initialize(SignalHeadData data)
        {
            this.data = data;
            transform.position = new Vector3(data.wktLocation.x - 122000, 0, data.wktLocation.y - 450000); //TODO remove these hardcoded test values!
            modelChild.localScale = new Vector3(data.laneWidth, 1, 1);
            transform.rotation = Quaternion.Euler(0, data.rotationAngle, 0);
            name = "Signal Head " + data.groupID;

            // Add listeners
            sso.eventUpdateRealtime.started.AddListener(OnUpdateRealtimeChanged);
            sso.eventSimulationTimeChanged.started.AddListener(OnSimulationTimeChanged);
            sso.eventSimulationSpeedChanged.started.AddListener(OnSimulationSpeedChanged);
            sso.eventSimulationStateChanged.started.AddListener(OnSimulationStateChanged);

            // Animation
            animationClip = new AnimationClip();
            animationClip.name = animationName;
            animationClip.legacy = true;
            animation.wrapMode = WrapMode.Clamp;


        }

        // Start is called before the first frame update
        void Start()
        {
        
        }

        public void UpdateFromData()
        {
            // Loop through signal head data schedule & add animation

        }


        private void OnUpdateRealtimeChanged(bool value)
        {

        }

        private void OnSimulationTimeChanged(float value)
        {
            if(data == null || data.schedule == null) return;
            // Fetch the closest color index based on sim time
            var k = ArrayExtention.MinBy(data.schedule, x => Math.Abs(x.Key - value));

            Debug.Log("sh: " + k.Value);

            switch(k.Value)
            {
                case 0:
                    meshRenderer.material = materialRed;
                    break;
                case 1:
                    meshRenderer.material = materialAmber;
                    break;
                case 2:
                    meshRenderer.material = materialGreen;
                    break;
                default:
                    meshRenderer.material = materialAmber;
                    break;
            }
        }

        private void OnSimulationSpeedChanged(float value)
        {
            
        }

        private void OnSimulationStateChanged(int value)
        {

        }
    }
}
