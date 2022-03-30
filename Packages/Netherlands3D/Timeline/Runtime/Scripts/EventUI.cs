using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SLIDDES.UI;

namespace Netherlands3D.Timeline
{
    public class EventUI : MonoBehaviour
    {
        /// <summary>
        /// The event data for this eventUI
        /// </summary>
        public Event dEvent;

        public RectTransform rectTransform;

        public void Initialize(Event dEvent, float posXLeft, float posXRight)
        {
            this.dEvent = dEvent;
            rectTransform.SetRect(0, 0, posXLeft, posXRight);
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
