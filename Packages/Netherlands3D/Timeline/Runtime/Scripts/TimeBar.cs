using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Timeline
{
    /// <summary>
    /// Class attached to a time bar to handle its displaying of time
    /// </summary>
    public class TimeBar : MonoBehaviour
    {
        private static float pixelDistanceDates = 100;

        [Header("Components")]
        [SerializeField] private Transform parentDates;
        [SerializeField] private GameObject prefabTimeBarDate;

        /// <summary>
        /// The rect transform of the time bar
        /// </summary>
        [HideInInspector] public RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        /// <summary>
        /// Update the timebars visuals that are displaying time
        /// </summary>
        public void UpdateVisuals(DateTime dateTimeLeaderIndex, int barIndex)
        {
            // Clear old
            foreach(Transform child in parentDates.transform)
            {
                Destroy(child.gameObject);
            }

            // Calculate space
            float width = rectTransform.rect.width;
            int datesToPlace = (int)(width / pixelDistanceDates);
            float spaceBetween = width / datesToPlace;

            // Calc bar starting date
            switch(barIndex)
            {
                default: // 0 (left bar)
                    dateTimeLeaderIndex.AddYears(-datesToPlace);
                    break;
                case 1: // 1 (middle bar)

                    break;
                case 2: // 2 (right bar)
                    dateTimeLeaderIndex.AddYears(datesToPlace);
                    break;
            }

            // Space dates evenly
            for(int i = 0; i < datesToPlace; i++)
            {
                TimeBarDate a = Instantiate(prefabTimeBarDate, parentDates).GetComponent<TimeBarDate>();
                a.transform.localPosition = new Vector3(-(width / 2) + (spaceBetween * i) + spaceBetween * 0.5f, 0, 0);
                a.field.text = dateTimeLeaderIndex.AddYears(i).ToString("yyyy");
            }
        }
    }
}
