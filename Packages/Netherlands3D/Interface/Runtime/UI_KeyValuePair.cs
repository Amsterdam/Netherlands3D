using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_KeyValuePair : MonoBehaviour
{
    [SerializeField]
    private Text keyText;

    [SerializeField]
    private Text valueText;

    public void SetValues(string key, string value)
    {
        keyText.text = key;
        valueText.text = value;
    }
}
