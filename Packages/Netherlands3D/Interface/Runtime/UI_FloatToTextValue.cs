using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class UI_FloatToTextValue : MonoBehaviour
{
    private Text text;
    void Awake()
    {
        text = GetComponent<Text>();
    }

    public void SetFloatText(float value)
    {
        if (text) text.text = value.ToString();
    }
}
