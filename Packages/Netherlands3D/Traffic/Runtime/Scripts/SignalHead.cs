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
        public SSO sso;

        [Header("Components")]
        public Transform modelChild;
        public MeshRenderer meshRenderer;
        public Material materialRed; // Color index 0
        public Material materialAmber; // Color index 1
        public Material materialGreen; // Color index 2
        
        private void OnEnable()
        {
            // If turned off and then turned back on update its values
            if(data != null)
            {
                OnSimulationTimeChanged(sso.simulationTime.Value);
            }
        }

        private void OnDisable()
        {
            if(data != null)
            {
                sso.eventSimulationTimeChanged.started.RemoveListener(OnSimulationTimeChanged);
            }
        }

        public void Initialize(SignalHeadData data)
        {
            this.data = data;
            transform.position = new Vector3(data.wktLocation.x, 0, data.wktLocation.y);
            modelChild.localScale = new Vector3(data.laneWidth, 1, 1);
            transform.rotation = Quaternion.Euler(0, data.rotationAngle, 0);
            name = "Signal Head " + data.groupID;

            // Add listeners
            sso.eventSimulationTimeChanged.started.AddListener(OnSimulationTimeChanged);
        }

        // Start is called before the first frame update
        void Start()
        {
        
        }

        private void OnSimulationTimeChanged(float value)
        {
            if(data == null || data.schedule == null || data.schedule.Count < 1) return;
            // Fetch the closest color index based on sim time
            var k = ArrayExtention.MinBy(data.schedule, x => Math.Abs(x.Key - value));

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
    }
}
