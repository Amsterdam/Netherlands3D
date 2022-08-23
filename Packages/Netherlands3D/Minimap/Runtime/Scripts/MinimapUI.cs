using Netherlands3D.JavascriptConnection;
using Netherlands3D.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using SLIDDES.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Netherlands3D.Minimap
{
    /// <summary>
    /// Handles all the UI interaction of the minimap
    /// </summary>
    [AddComponentMenu("Netherlands3D/Minimap/MinimapUI")]
    public class MinimapUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Values")]
        [Tooltip("The top, bottom, left, right values that increase the rect on hover")]
        [SerializeField] private Rect onHoverResize;

        [Header("Components")]
        [Tooltip("The wmts script that handles the minimap data")]
        [SerializeField] private WMTS wmts;
        [Tooltip("Gameobject that holds UI for navigating the map")]
        [SerializeField] private GameObject navigation;

        /// <summary>
        /// Is the user dragging the UI?
        /// </summary>
        private bool isDragging;
        /// <summary>
        /// Min amount of zoom index
        /// </summary>
        private float zoomIndexMin = 6;
        /// <summary>
        /// Max amount of zoom index
        /// </summary>
        private float zoomIndexMax = 11;
        /// <summary>
        /// The default size of the rect transform
        /// </summary>
        private Rect rectSizeDefault;
        /// <summary>
        /// The rect default offset max
        /// </summary>
        private Vector2 rectOffsetMax;
        /// <summary>
        /// The rect default offset min
        /// </summary>
        private Vector2 rectOffsetMin;
        /// <summary>
        /// The starting position where the cursor started grabbing
        /// </summary>
        private Vector3 onDragStartingPosition;
        /// <summary>
        /// The rect transform of the wmts component containing the tiles
        /// </summary>
        private RectTransform wmtsRectTransform;
        /// <summary>
        /// The rect transform attached to this gameobject
        /// </summary>
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            wmtsRectTransform = wmts.GetComponent<RectTransform>();
        }

        // Start is called before the first frame update
        void Start()
        {
            navigation.SetActive(false);
            rectSizeDefault = rectTransform.rect;
            rectOffsetMax = rectTransform.offsetMax;
            rectOffsetMin = rectTransform.offsetMin;
        }

        /// <summary>
        /// Resize the minimap rect transform
        /// </summary>
        /// <param name="expand">Should it expand or resize back to default</param>
        private void ResizeRect(bool expand)
        {
            // The top and right are connected so need a different calculation (and take in account the width/height)
            rectTransform.SetTop(expand ? -rectSizeDefault.height + rectOffsetMax.y + onHoverResize.x * -1 : rectOffsetMax.y * -1); // using a *-1 to keep interface for user simple (OnHoverResize)            
            rectTransform.SetBottom(expand ? rectOffsetMin.y + onHoverResize.y * -1 : rectOffsetMin.y);
            rectTransform.SetLeft(expand ? rectOffsetMin.x + onHoverResize.width * -1 : rectOffsetMin.x);
            rectTransform.SetRight(expand ? -rectSizeDefault.width + rectOffsetMax.x + onHoverResize.height * -1 : rectOffsetMax.x * -1);
        }

        /// <summary>
        /// Start the user interaction with the map
        /// </summary>
        private void StartMapInteraction()
        {
            navigation.SetActive(true);
            ChangePointerStyleHandler.ChangeCursor(ChangePointerStyleHandler.Style.POINTER);
            ResizeRect(true);
        }

        /// <summary>
        /// Stop the user interaction with the map
        /// </summary>
        private void StopMapInteraction()
        {
            navigation.SetActive(false);
            ChangePointerStyleHandler.ChangeCursor(ChangePointerStyleHandler.Style.AUTO);
            ResizeRect(false);
        }

        #region Interfaces

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
            ChangePointerStyleHandler.ChangeCursor(ChangePointerStyleHandler.Style.GRABBING);
            wmts.CenterPointerInView = true;
            onDragStartingPosition = wmtsRectTransform.position - (Vector3)eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            wmtsRectTransform.transform.position = (Vector3)eventData.position + onDragStartingPosition;
            wmts.UpdateTiles();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
            ChangePointerStyleHandler.ChangeCursor(ChangePointerStyleHandler.Style.POINTER);
        }

        public void OnScroll(PointerEventData eventData)
        {            
            if(eventData.scrollDelta.y > 0)
            {
                // zoom in
                if(wmts.LayerIndex + 1 <= zoomIndexMax)
                {
                    wmts.Zoom(1);                    
                }
            }
            else
            {
                // zoom out
                if(wmts.LayerIndex - 1 >= zoomIndexMin)
                {
                    wmts.Zoom(-1);
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // When the mouse enters the ui
            StartMapInteraction();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // When the pointer is off the map UI & the user is not dragging stop the map interaction
            if(!isDragging) StopMapInteraction();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(!isDragging) wmts.ClickedMap(eventData);
        }

        #endregion
    }
}
