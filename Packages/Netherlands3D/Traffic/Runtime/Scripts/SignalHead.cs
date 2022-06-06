using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Traffic.VISSIM
{
    [AddComponentMenu("Netherlands3D/Traffic/VISSIM/Signal Head")]
    public class SignalHead : MonoBehaviour
    {
        public SignalHeadData data;

        [Header("Components")]
        public Transform modelChild;
        public MeshRenderer meshRenderer;
        public Material materialRed; // Color index 0
        public Material materialAmber; // Color index 1
        public Material materialGreen; // Color index 2

        // Start is called before the first frame update
        void Start()
        {
        
        }

        public void Initialize(SignalHeadData data)
        {
            this.data = data;
            transform.position = new Vector3(data.wktLocation.x - 122000, 0, data.wktLocation.y - 450000); //TODO remove these hardcoded test values!
            modelChild.localScale = new Vector3(data.laneWidth, 1, 1);
            transform.rotation = Quaternion.Euler(0, data.rotationAngle, 0);
            name = "Signal Head " + data.groupID;
        }
    }
}
