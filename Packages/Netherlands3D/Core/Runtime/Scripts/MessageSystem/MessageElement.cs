using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MessageElement
{
    protected string elementName;
    public MessageElement(string elementName)
    {
        this.elementName = elementName;
    }
    public abstract GameObject GetElement();
    public abstract System.Type GetMessageType();

}
