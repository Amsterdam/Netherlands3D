using System;
using UnityEngine;
using UnityEngine.UI;

public class MessageText : MessageElement
{
    private string textMessage;
    public MessageText(string message, string elementName) : base(elementName)
    {
        textMessage = message;
    }
    public override GameObject GetElement()
    {
        return GameObject.Instantiate(new GameObject(elementName, typeof(Text)));
    }

    public override Type GetMessageType()
    {
        return typeof(Text);
    }
}
