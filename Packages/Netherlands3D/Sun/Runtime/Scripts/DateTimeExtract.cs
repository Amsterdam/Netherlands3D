using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class DateTimeExtract : MonoBehaviour
{
    public enum ExtractType
    {
        SECONDS,
        MINUTES,
        HOURS,
        DAYS,
        MONTHS,
        YEARS
    }

    [SerializeField] private ExtractType extractType;

    private InputField field;

    private void Start()
    {
        field = GetComponent<InputField>();
    }

    public void ExtractFromDateTime(DateTime dateTime)
    {
        int extractValue = -1;
        switch (extractType)
        {
            case ExtractType.SECONDS:
                extractValue = dateTime.Second;
                break;
            case ExtractType.MINUTES:
                extractValue = dateTime.Minute;
                break;
            case ExtractType.HOURS:
                extractValue = dateTime.Hour;
                break;
            case ExtractType.DAYS:
                extractValue = dateTime.Day;
                break;
            case ExtractType.MONTHS:
                extractValue = dateTime.Month;
                break;
            case ExtractType.YEARS:
                extractValue = dateTime.Year;
                break;
            default:
                throw new Exception("Impossible case found, this shouldn't happen!");
        }

        if (!field.isFocused)
        {
            field.text = extractValue.ToString();
        }
    }
}
