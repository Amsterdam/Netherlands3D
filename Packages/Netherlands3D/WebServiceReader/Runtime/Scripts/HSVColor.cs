using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HSVColor : MonoBehaviour
{
    [Header("Invoked Event")]
    [SerializeField] private ColorEvent colorEvent;

    public float Hue { get { return hue; } set { hue = value; OnValueChanged(); } }
    public float Saturation { get { return saturation; } set { saturation = value; OnValueChanged(); } }
    public float Brightness { get { return brightness; } set { brightness = value; OnValueChanged(); } }

    private float hue = 0.5f;
    private float saturation = 0.5f;
    private float brightness = 0.5f;

    private void OnValueChanged()
    {
        colorEvent.Invoke(Color.HSVToRGB(hue, saturation, brightness));
    }
}
