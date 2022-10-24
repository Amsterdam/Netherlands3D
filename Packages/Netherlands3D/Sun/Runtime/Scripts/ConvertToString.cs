using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;


public class ConvertToString : MonoBehaviour
{
    [SerializeField] private StringEvent invokedStringEvent; 

    public void ConvertIntToString(int intToConvert)
    {
        invokedStringEvent.Invoke(intToConvert.ToString());
    }
    
    public void ConvertFloatToString(float floatToConvert)
    {
        invokedStringEvent.Invoke(floatToConvert.ToString());
    }


}
