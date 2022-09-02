using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Netherlands3D.Timeline
{
    /// <summary>
    /// Link scroll rects together so that when 1 scrolls the other scrolls with
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollRectLinker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Values")]
        [Tooltip("Link the scrollRect horizontal movement")]
        [SerializeField] private bool linkHorizontal = true;
        [Tooltip("Link the scrollRect vertical movement")]
        [SerializeField] private bool linkVertical = true;
        [Header("Components")]
        [Tooltip("The other scrollrects to scroll when this scroll rect scrolls")]
        [SerializeField] private ScrollRect[] scrollRects;

        /// <summary>
        /// If the scroll rect linker is active
        /// </summary>
        private bool isLinkActive;
        /// <summary>
        /// The scroll rect to link
        /// </summary>
        private ScrollRect scrollRect;
                
        private void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();
            scrollRect.onValueChanged.AddListener(OnScrollRectChanged);
        }

        /// <summary>
        /// When a scroll rect position is changed
        /// </summary>
        /// <param name="vector2"></param>
        private void OnScrollRectChanged(Vector2 vector2)
        {
            if(!isLinkActive) return;
            // Tell other scroll rects to set its positions the same as selected scroll rect (based on horizontal/vertical values)
            foreach(var item in scrollRects)
            {
                item.movementType = ScrollRect.MovementType.Unrestricted;
                item.content.localPosition = new Vector3(
                    linkHorizontal ? scrollRect.content.localPosition.x : item.content.localPosition.x,
                    linkVertical ? scrollRect.content.localPosition.y : item.content.localPosition.y,
                    item.content.localPosition.z);
            }
        }

        #region Interfaces 

        public void OnPointerDown(PointerEventData eventData)
        {
            isLinkActive = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isLinkActive = false;
            // Reset other scroll rects
            foreach(var item in scrollRects)
            {
                item.movementType = ScrollRect.MovementType.Elastic;
            }
        }

        #endregion
    }
}
