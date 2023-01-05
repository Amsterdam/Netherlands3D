using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Netherlands3D.Timeline
{
    public class TimeBarDate : MonoBehaviour
    {
        /// <summary>
        /// The text where the date is displayed
        /// </summary>
        public TextMeshProUGUI field;

        private void Start()
        {
            var rt = GetComponent<RectTransform>();
            transform.localPosition = new Vector3(transform.localPosition.x, -rt.sizeDelta.y / 2, transform.localPosition.z);
        }
    }
}
