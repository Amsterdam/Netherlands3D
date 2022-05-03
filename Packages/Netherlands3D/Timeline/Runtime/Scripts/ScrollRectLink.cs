using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

namespace Netherlands3D.Timeline
{
    /// <summary>
    /// Links scroll rects scrolling
    /// </summary>
    public class ScrollRectLink : MonoBehaviour
    {
        public ScrollRect[] scrollRects;

        private ScrollRect selectedScrollRect;

        [Tooltip("Fetch the Raycaster from the GameObject (the Canvas)")]
        [SerializeField] private GraphicRaycaster m_Raycaster;
        private PointerEventData m_PointerEventData;
        [Tooltip("The event system of the scene")]
        [SerializeField] private EventSystem m_EventSystem;

        // Update is called once per frame
        void Update()
        {
            MouseUpdate();
        }

        private void MouseUpdate()
        {
            //Check if the left Mouse button is clicked
            if(Input.GetKeyDown(KeyCode.Mouse0))
            {
                //Set up the new Pointer Event
                m_PointerEventData = new PointerEventData(m_EventSystem);
                //Set the Pointer Event Position to that of the mouse position
                m_PointerEventData.position = Input.mousePosition;

                //Create a list of Raycast Results
                List<RaycastResult> results = new List<RaycastResult>();

                //Raycast using the Graphics Raycaster and mouse click position
                m_Raycaster.Raycast(m_PointerEventData, results);
                results.Reverse(); // Revert list

                //For every result returned, check if it contains a scrollrect to set it as the selectedscrollrect
                foreach(RaycastResult result in results)
                {
                    // Check if scrollrect was hit
                    var scrollRect = result.gameObject.GetComponent<ScrollRect>();
                    if(scrollRect != null && scrollRects.Contains(scrollRect))
                    {
                        SetSelectedScrollRect(scrollRect);
                    }
                }
            }

            if(Input.GetKeyUp(KeyCode.Mouse0))
            {
                SetSelectedScrollRect(null);
            }
        }

        private void OnScrollRectChanged(Vector2 vector2)
        {
            // Tell other scroll rects to set its positions the same as selected scroll rect
            foreach(var item in scrollRects)
            {
                if(item == selectedScrollRect) continue;
                item.movementType = ScrollRect.MovementType.Unrestricted;
                item.content.localPosition = selectedScrollRect.content.localPosition;
            }
        }

        /// <summary>
        /// Set a new selected scroll rect
        /// </summary>
        /// <param name="scrollRect"></param>
        private void SetSelectedScrollRect(ScrollRect scrollRect)
        {
            // Reset previous selected scrollrect if set
            if(selectedScrollRect != null)
            {
                selectedScrollRect.onValueChanged.RemoveListener(OnScrollRectChanged);
                selectedScrollRect.movementType = ScrollRect.MovementType.Elastic;
                selectedScrollRect = null;
            }

            // Reset other scroll rects
            foreach(var item in scrollRects)
            {
                item.movementType = ScrollRect.MovementType.Elastic;
            }

            // Set new
            if(scrollRect == null) return;
            selectedScrollRect = scrollRect;
            selectedScrollRect.movementType = ScrollRect.MovementType.Unrestricted;
            selectedScrollRect.onValueChanged.AddListener(OnScrollRectChanged);
        }
    }
}
