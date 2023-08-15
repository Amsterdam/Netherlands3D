using System;
using UnityEngine;
using UnityEngine.UI;

public class DateTimeToString : MonoBehaviour
{
    [SerializeField] private Text dateTimeText;

    public void SetTextToDateTime(DateTime dateTime)
    {
        dateTimeText.text = dateTime.ToString();
    }
}
