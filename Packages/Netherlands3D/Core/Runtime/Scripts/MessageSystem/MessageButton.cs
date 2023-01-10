using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageButton : MessageElement
{
    private Action function;
    public MessageButton(Action buttonFunction, string elementName) : base(elementName)
    {
        function = buttonFunction;
    }

    public override GameObject GetElement()
    {
        Button btn = GameObject.Instantiate(new GameObject(elementName, typeof(Button))).GetComponent<Button>();
        btn.onClick.AddListener(function.Invoke);
        return btn.gameObject;
    }

    public override Type GetMessageType()
    {
        return typeof(Button);
    }

}
