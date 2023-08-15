using Netherlands3D.Events;
using UnityEngine;

public class StringNumberParser : MonoBehaviour
{
    [SerializeField] private IntEvent intEvent;
    [SerializeField] private FloatEvent floatEvent;

    public void ParseToInteger(string integerString)
    {
        if (int.TryParse(integerString, out int parsedInt))
        {
            intEvent.InvokeStarted(parsedInt);
        }
    }

    public void ParseToFloat(string floatString)
    {
        if (float.TryParse(floatString, out float parsedFloat))
        {
            floatEvent.InvokeStarted(parsedFloat);
        }
    }
}
