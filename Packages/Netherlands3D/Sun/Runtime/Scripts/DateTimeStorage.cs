using System;
using Netherlands3D.Events;
using UnityEngine;

public class DateTimeStorage : MonoBehaviour
{
    [SerializeField] private DateTimeContainer dateTimeContainer;
    [SerializeField] private DateTimeEvent onDateTimeStored;
    [SerializeField] private DateTimeEvent onDateTimeLoaded;
    private DateTime receivedDateTime;

    private void Awake()
    {
        if (dateTimeContainer == null)
        {
            // The DateTimeContainer can be predefined, but if it doesn't exist, one will be created on Awake.
            dateTimeContainer = ScriptableObject.CreateInstance<DateTimeContainer>();
        }
    }

    public void ReceiveDateTime(DateTime dateTime)
    {
        // This function can receive the dateTime every frame and it saves it, but doesn't keep it in the storage.
        receivedDateTime = dateTime;
    }

    public void StoreCurrentDateTime()
    {
        dateTimeContainer.SetValue(receivedDateTime);
        if (onDateTimeStored != null)
        {
            onDateTimeStored.InvokeStarted(dateTimeContainer.Value);
        }
    }

    public void LoadStoredDateTime()
    {
        if (onDateTimeLoaded != null)
        {
            onDateTimeLoaded.InvokeStarted(dateTimeContainer.Value);
        }
    }
}
