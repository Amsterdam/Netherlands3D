using System;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Events
{

	[CreateAssetMenu(fileName = "DateTimeEvent", menuName = "EventContainers/DateTimeEvent", order = 0)]
	[System.Serializable]
	public class DateTimeEvent : EventContainer<DateTime>
    {
		public override void InvokeStarted(DateTime dateTimeContent)
		{
            started.Invoke(dateTimeContent);
		}
	}
}