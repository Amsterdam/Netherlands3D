using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class UI_FloatToTextValue : MonoBehaviour
{
    private Text text;

    [SerializeField]
    private bool interpolate = false;
    [SerializeField]
    private bool roundToInt = false;
    [SerializeField]
    private float minLerp = 0;
    [SerializeField]
    private float maxLerp = 100;

    void Awake()
    {
        text = GetComponent<Text>();
    }

    public void SetFloatText(float value)
    {
        if (!text) return;

        if (interpolate)
        {
            value = Mathf.Lerp(minLerp, maxLerp, value);
        }

        if(roundToInt)
        {
            value = Mathf.RoundToInt(value);
        }


        text.text = value.ToString();
    }
}
