using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D.Timeline
{
    public class TimeBarInteraction : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private float sensitivity = 1000;

        [Header("Components")]
        [Tooltip("The timeline UI this interaction is part of")]
        public TimelineUI timelineUI;

        /// <summary>
        /// Is the user dragging its mouse on the time bar?
        /// </summary>
        private bool isDragging;
        /// <summary>
        /// Is the mouse on the ui?
        /// </summary>
        private bool mouseIsOn;
        /// <summary>
        /// The position of the mouse when left input was pressed
        /// </summary>
        private Vector3 mouseDownPosition;

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
            isDragging = true;
            mouseDownPosition = Input.mousePosition;
            print("yo");
        }

        /// <summary>
        /// The mouse is on the ui
        /// </summary>
        public void OnPointerStay()
        {
            if(isDragging)
            {
                int dir = mouseDownPosition.x < Input.mousePosition.x ? 1 : -1;
                timelineUI.ScrollTimeBar(Vector3.Distance(mouseDownPosition, Input.mousePosition) * dir * sensitivity * UnityEngine.Time.deltaTime);
                timelineUI.PlayScroll(false);
            }

            if(mouseIsOn)
            {
                if(Input.mouseScrollDelta.y < 0)
                {
                    // Up
                    timelineUI.SetTimeUnit(-1);
                }
                else if(Input.mouseScrollDelta.y > 0)
                {
                    // Down
                    timelineUI.SetTimeUnit(1);
                }
            }
        }

        /// <summary>
        /// User presses mouse up on time bar
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerUp(PointerEventData eventData)
        {
            isDragging = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            mouseIsOn = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            mouseIsOn = false;
        }
    }
}
