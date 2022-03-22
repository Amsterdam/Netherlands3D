using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D.Timeline
{
    public class TimeBarInteraction : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Components")]
        [Tooltip("The timeline UI this interaction is part of")]
        public TimelineUI timelineUI;

        /// <summary>
        /// Is the user dragging its mouse on the time bar?
        /// </summary>
        private bool isDragging;
        /// <summary>
        /// The position of the mouse when left input was pressed
        /// </summary>
        private Vector3 mouseDownPosition;

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            OnPointerStay();
        }

        /// <summary>
        /// User presses mouse down on time bar
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerDown(PointerEventData eventData)
        {
            print("down");
            isDragging = true;
            mouseDownPosition = Input.mousePosition;
        }

        /// <summary>
        /// User presses mouse up on time bar
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerUp(PointerEventData eventData)
        {
            print("up");
            isDragging = false;
        }

        private void OnPointerStay()
        {
            if(!isDragging) return;

            timelineUI.ScrollTimeBar(Vector3.Distance(mouseDownPosition, Input.mousePosition) * Time.deltaTime);
        }
    }
}
