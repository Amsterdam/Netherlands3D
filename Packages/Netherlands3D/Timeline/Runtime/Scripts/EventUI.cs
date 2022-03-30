using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Timeline
{
    public class EventUI : MonoBehaviour
    {
        /// <summary>
        /// The event data for this eventUI
        /// </summary>
        public Event dEvent;

        public RectTransform rectTransform;

        public void Initialize(Event dEvent)
        {
            this.dEvent = dEvent;
        }

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
