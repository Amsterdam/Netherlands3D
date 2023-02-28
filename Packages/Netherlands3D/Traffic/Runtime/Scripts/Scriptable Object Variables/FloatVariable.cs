using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;

namespace Netherlands3D.Traffic
{
    /// <summary>
    /// A scriptable object variable of type float
    /// </summary>
    [CreateAssetMenu(fileName = "Float Variable", menuName = "ScriptableObjects/Traffic/Float Variable", order = 1)]
    public class FloatVariable : ScriptableObject
    {
        /// <summary>
        /// The value of the float
        /// </summary>
        public float Value
        {
            get { return value; }
            set
            {
                this.value = value;
                if(onValueChanged != null && Application.isPlaying) onValueChanged.InvokeStarted(value);
            }
        }

        /// <summary>
        /// Actual float value. You can set this value also when you dont want to invoke the onValueChanged
        /// </summary>
        public float value;

        [TextArea(1, 10)]
        [Tooltip("The description of what this variable is for")]
        [SerializeField] private string description;

        /// <summary>
        /// Float event that gets invoked when Value changes
        /// </summary>
        public FloatEvent onValueChanged;

        private void OnValidate()
        {
            Value = value;
        }
    }
}