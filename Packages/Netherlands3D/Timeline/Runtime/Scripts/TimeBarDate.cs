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
        [HideInInspector] public TextMeshProUGUI field;

        private void Awake()
        {
            field = GetComponent<TextMeshProUGUI>();
        }

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
