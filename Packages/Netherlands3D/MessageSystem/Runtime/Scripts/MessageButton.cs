using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageButton : MessageElement
{
    public Action ButtonFunction { get; private set; }
    public MessageButton(Action buttonFunction, string elementName) : base(elementName)
    {
        ButtonFunction = buttonFunction;
    }
}
