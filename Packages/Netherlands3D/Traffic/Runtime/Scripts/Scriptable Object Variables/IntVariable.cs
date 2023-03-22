using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;

namespace Netherlands3D.Traffic
{
    /// <summary>
    /// A scriptable object variable of type int
    /// </summary>
    [CreateAssetMenu(fileName = "Int Variable", menuName = "ScriptableObjects/Traffic/Int Variable", order = 1)]
    public class IntVariable : ScriptableObject
    {
        /// <summary>
        /// The value of the float
        /// </summary>
        public int Value
        {
            get { return value; }
            set
            {
                this.value = value;
                if(Application.isPlaying && onValueChanged != null) onValueChanged.InvokeStarted(value);
            }
        }

        /// <summary>
        /// Actual float value. You can set this value also when you dont want to invoke the onValueChanged
        /// </summary>
        public int value;

        [TextArea(1, 10)]
        [Tooltip("The description of what this variable is for")]
        [SerializeField] private string description;

        /// <summary>
        /// Float event that gets invoked when Value changes
        /// </summary>
        public IntEvent onValueChanged;

        private void OnValidate()
        {
            Value = value;
        }
    }
}
