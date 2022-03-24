using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// The pixel position and the corresponding dateTime
        /// </summary>
        private Dictionary<float, DateTime> dateTimePositions = new Dictionary<float, DateTime>();

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
        /// Get the current selected dateTime from the time bar based on its local x position
        /// </summary>
        /// <returns></returns>
        public DateTime GetCurrentDateTime()
        {
            float posX = transform.localPosition.x * -1;
            // Get the closest dictionary value to posX
            var bestMatch = dateTimePositions.OrderBy(x => Math.Abs(x.Key - posX)).FirstOrDefault();
            return bestMatch.Value;
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
            dateTimePositions.Clear();

            // Calculate space
            float width = rectTransform.rect.width;
            int datesToPlace = (int)(width / pixelDistanceDates);
            float spaceBetween = width / datesToPlace;

            // Calc bar starting date
            switch(barIndex)
            {
                default: // 0 (left bar)
                    dateTimeLeaderIndex = dateTimeLeaderIndex.AddYears(-datesToPlace);
                    break;
                case 1: // 1 (middle bar)

                    break;
                case 2: // 2 (right bar)
                    dateTimeLeaderIndex = dateTimeLeaderIndex.AddYears(datesToPlace);
                    break;
            }

            // Space dates evenly
            for(int i = 0; i < datesToPlace; i++)
            {
                TimeBarDate a = Instantiate(prefabTimeBarDate, parentDates).GetComponent<TimeBarDate>();
                float posX = -(width / 2) + (spaceBetween * i) + spaceBetween * 0.5f;
                DateTime dateTime = dateTimeLeaderIndex.AddYears(i);
                a.transform.localPosition = new Vector3(posX, 0, 0);
                a.field.text = dateTime.ToString("yyyy");
                dateTimePositions.Add(posX, dateTime);
            }
            foreach(var item in dateTimePositions.ToArray())
            {
                Debug.Log(item);
            }
        }
    }
}
