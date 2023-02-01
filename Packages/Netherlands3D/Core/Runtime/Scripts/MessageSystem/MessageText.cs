using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessageText : MessageElement
{
    public string Message { get; private set; }
    public float FontSize { get; private set; }
    public Color TextColor { get; private set; }

    public MessageText(string message, float size, Color textColor, string elementName) : base(elementName)
    {
        TextColor = textColor;
        FontSize = size;
        Message = message;
    }
    //public override GameObject GetElement()
    //{
    //    return txt.gameObject;
    //}

    //public override Type GetMessageType()
    //{
    //    return txt.GetType();
    //}
}
