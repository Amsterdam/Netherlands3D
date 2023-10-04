using System.Collections;
using Netherlands3D.Coordinates;
using Netherlands3D.JavascriptConnection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Netherlands3D.Minimap
{
    /// <summary>
    /// Handles the minimap UI interaction
    /// </summary>
    public class MinimapUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Values")]
        [Tooltip("The max zoomscale of the minimap")]
        [SerializeField] private float maxZoomScale = 6.0f;
        [Tooltip("Should the minimap resize upon hover?")]
        [SerializeField] private bool hoverResize = true;
        [Tooltip("The speed at which the minimap resizes on hover")]
        [SerializeField] private float hoverResizeSpeed = 10.0f;
        [Tooltip("The new rect delta size when hovered")]
        [SerializeField] private Vector2 hoverSize;

        [Header("Components")]
        [SerializeField] private RectTransform mapTiles;
        [SerializeField] private RectTransform navigation;

        /// <summary>
        /// The drag offset when the user starts dragging (position where the user clicked before dragging)
        /// </summary>
        private Vector3 dragOffset;
        /// <summary>
        /// The wmts map script that handles the minimap functionallity
        /// </summary>
        private WMTSMap wmtsMap;
        /// <summary>
        /// The rect transform of this gameobject
        /// </summary>
        private RectTransform rectTransform;
        /// <summary>
        /// The default size delta of this rect transform before hover resize
        /// </summary>
        private Vector2 defaultSizeDelta;
        /// <summary>
        /// Is the minimap being dragged?
        /// </summary>
        private bool dragging = false;
        /// <summary>
        /// The minimum zoom scale
        /// </summary>
        private float minZoomScale = 0.0f;
        /// <summary>
        /// The current zoom scale
        /// </summary>
        private float zoomScale = 0.0f;

        private void Awake()
        {
            zoomScale = minZoomScale;
            wmtsMap = mapTiles.GetComponent<WMTSMap>();
            rectTransform = this.GetComponent<RectTransform>();

            defaultSizeDelta = rectTransform.sizeDelta;

            var anchorOffset = rectTransform.pivot * defaultSizeDelta;
            rectTransform.pivot = new Vector2(0,0);
            rectTransform.anchoredPosition -= anchorOffset;

            navigation.gameObject.SetActive(false);
        }

        /// <summary>
        /// For resizing the UI when the mouse enters the minimap UI
        /// </summary>
        /// <param name="targetScale"></param>
        /// <returns></returns>
        IEnumerator HoverResize(Vector2 targetScale)
        {
            while(true)
            {
                if(Vector2.Distance(targetScale, rectTransform.sizeDelta) > 0.5f)
                {
                    //Eaze out to target scale
                    rectTransform.sizeDelta = Vector2.Lerp(rectTransform.sizeDelta, targetScale, hoverResizeSpeed * Time.deltaTime);
                }
                else
                {
                    //Finish animation
                    rectTransform.sizeDelta = targetScale;
                    yield break;
                }
                yield return null;
            }
        }

        /// <summary>
        /// When the user starts interacting with the map
        /// </summary>
        private void StartedMapInteraction()
        {
            navigation.gameObject.SetActive(true);
            ChangePointerStyleHandler.ChangeCursor(ChangePointerStyleHandler.Style.POINTER);

            StopAllCoroutines();
            if (hoverResize)
            {
                StartCoroutine(HoverResize(hoverSize));
            }
        }

        /// <summary>
        /// When the user stops interacting with the map
        /// </summary>
        private void StoppedMapInteraction()
        {
            navigation.gameObject.SetActive(false);
            wmtsMap.CenterPointerInView = true;
            ChangePointerStyleHandler.ChangeCursor(ChangePointerStyleHandler.Style.AUTO);

            StopAllCoroutines();
            if (hoverResize)
            {
                StartCoroutine(HoverResize(defaultSizeDelta));
            }
        }

        /// <summary>
        /// Scale the map over a set origin
        /// </summary>
        /// <param name="scaleOrigin"></param>
        /// <param name="newScale"></param>
        public void ScaleMapOverOrigin(Vector3 scaleOrigin, Vector3 newScale)
        {
            var targetPosition = mapTiles.position;
            var origin = scaleOrigin;
            var newOrigin = targetPosition - origin;
            var relativeScale = newScale.x / mapTiles.localScale.x;
            var finalPosition = origin + newOrigin * relativeScale;

            mapTiles.localScale = newScale;
            mapTiles.position = finalPosition;
        }

        /// <summary>
        /// Zoom in on the minimap
        /// </summary>
        /// <param name="useMousePosition"></param>
        public void ZoomIn(bool useMousePosition = true)
        {
            if (zoomScale >= maxZoomScale) return;

            zoomScale++;
            ZoomTowardsLocation(useMousePosition);
            wmtsMap.Zoomed((int)zoomScale);
        }

        /// <summary>
        /// Zoom out on the minimap
        /// </summary>
        /// <param name="useMousePosition"></param>
        public void ZoomOut(bool useMousePosition = true)
        {
            if (zoomScale <= minZoomScale) return;

            zoomScale--;
            ZoomTowardsLocation(useMousePosition);
            wmtsMap.Zoomed((int)zoomScale);
        }

        /// <summary>
        /// Zoom on a given location on the minimap
        /// </summary>
        /// <param name="useMouse"></param>
        private void ZoomTowardsLocation(bool useMouse = true)
        {
            var zoomTarget = Vector3.zero;
            if(useMouse)
            {
                zoomTarget = Mouse.current.position.ReadValue();
            }
            else
            {
                zoomTarget = rectTransform.position + new Vector3(rectTransform.sizeDelta.x * 0.5f, rectTransform.sizeDelta.y * 0.5f);
            }

            ScaleMapOverOrigin(zoomTarget, Vector3.one * Mathf.Pow(2.0f, zoomScale));
        }

        #region Interfaces

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragging = true;
            wmtsMap.CenterPointerInView = false;
            ChangePointerStyleHandler.ChangeCursor(ChangePointerStyleHandler.Style.GRABBING);
            dragOffset = mapTiles.position - (Vector3)eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            mapTiles.transform.position = (Vector3)eventData.position + dragOffset;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragging = false;
            ChangePointerStyleHandler.ChangeCursor(ChangePointerStyleHandler.Style.POINTER);
        }

        public void OnScroll(PointerEventData eventData)
        {
            if(eventData.scrollDelta.y > 0)
            {
                ZoomIn();
            }
            else if(eventData.scrollDelta.y < 0)
            {
                ZoomOut();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            StartedMapInteraction();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if(!dragging)
            {
                StoppedMapInteraction();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (dragging) return;

            wmtsMap.ClickedMap(eventData);
        }

        #endregion Interfaces

    }
}
