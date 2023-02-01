using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MessageElement
{
    public string ElementName { get; protected set; }
    public MessageElement(string elementName)
    {
        ElementName = elementName;
    }
    //public abstract GameObject GetElement();
    //public abstract System.Type GetMessageType();

}
