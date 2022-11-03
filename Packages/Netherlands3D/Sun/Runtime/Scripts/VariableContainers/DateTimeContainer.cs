using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New DateTimeContainer", menuName = "DataContainers/DateTimeContainer")]
public class DateTimeContainer : BaseVariableContainer<DateTime>
{
    public override string ToString()
    {
        return Value.ToString();
    } 
}
