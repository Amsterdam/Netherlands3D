using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Netherlands3D.Timeline
{

    /// <summary>
    /// A category of a event
    /// </summary>
    public class Category : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private TextMeshProUGUI nameField;

        private new string name;

        public void Initialize(string name)
        {
            this.name = name;
            nameField.text = name;
        }

    }
}
