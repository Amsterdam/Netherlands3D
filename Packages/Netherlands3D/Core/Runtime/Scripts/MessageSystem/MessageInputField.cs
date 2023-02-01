using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MessageInputField : MessageElement
{
    public string Placeholder { get; private set; }
    public Action<string> OnSubmitFunction { get; private set; }
    public MessageInputField(string elementName, Action<string> onSubmitFunction = null, string placeholder = "Enter text...") : base(elementName)
    {
        Placeholder = placeholder;
        OnSubmitFunction = onSubmitFunction;
    }
    //public override GameObject GetElement()
    //{
    //    return field.gameObject;
    //}

    //public override Type GetMessageType()
    //{
    //    return field.GetType();   
    //}
}
