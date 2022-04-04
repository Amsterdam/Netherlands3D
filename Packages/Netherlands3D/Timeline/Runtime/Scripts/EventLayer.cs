using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Timeline
{
    /// <summary>
    /// Layer of events
    /// </summary>
    public class EventLayer : MonoBehaviour
    {
        public List<EventUI> events = new List<EventUI>();

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        /// <summary>
        /// Add (and create) a event to the event layer
        /// </summary>
        /// <param name="dEvent"></param>
        /// <param name="prefabEventUI"></param>
        public EventUI AddEvent(Event dEvent, GameObject prefabEventUI)
        {
            EventUI a = Instantiate(prefabEventUI, transform).GetComponent<EventUI>();
            a.Initialize(dEvent, this);
            events.Add(a);
            return a;
        }
    }
}
