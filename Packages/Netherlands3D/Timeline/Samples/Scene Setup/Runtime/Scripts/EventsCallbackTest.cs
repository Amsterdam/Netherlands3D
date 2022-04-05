using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Timeline.Samples
{
    public class EventsCallbackTest : MonoBehaviour
    {
        public TimePeriod timePeriod;
        public GameObject cube;

        private void OnEnable()
        {
            timePeriod.unityEvent.AddListener(ToggleCube);
        }

        private void OnDisable()
        {
            timePeriod.unityEvent.RemoveListener(ToggleCube);
        }

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void ToggleCube()
        {
            cube.SetActive(!cube.activeSelf);
        }
    }
}
