using System;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Events
{
	[System.Serializable]
	public class DateTimeUnityEvent : UnityEvent<DateTime> { }

	[CreateAssetMenu(fileName = "DateTimeEvent", menuName = "EventContainers/DateTimeEvent", order = 0)]
	[System.Serializable]
	public class DateTimeEvent : EventContainer<DateTimeUnityEvent>
	{
		public void Invoke(DateTime dateTimeContent)
		{
			started.Invoke(dateTimeContent);
		}
	}
}