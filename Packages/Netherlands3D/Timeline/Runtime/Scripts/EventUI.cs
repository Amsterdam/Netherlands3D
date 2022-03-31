using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SLIDDES.UI;
using TMPro;

namespace Netherlands3D.Timeline
{
    public class EventUI : MonoBehaviour
    {
        /// <summary>
        /// The event data for this eventUI
        /// </summary>
        public Event dEvent;

        public RectTransform rectTransform;

        public TextMeshProUGUI nameField;

        public void Initialize(Event dEvent, float posXLeft, float posXRight)
        {
            this.dEvent = dEvent;
            rectTransform.SetRect(0, 0, posXLeft, posXRight);
            nameField.text = dEvent.name;
        }
    }
}
