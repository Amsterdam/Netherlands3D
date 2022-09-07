using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Timeline
{
    [CreateAssetMenu(fileName = "Timeline Time Period", menuName = "ScriptableObjects/Timeline/Time Period")]
    public class TimePeriodSO : ScriptableObject
    {
        [Tooltip("The time period data")]
        public TimePeriod timePeriod;
    }
}
