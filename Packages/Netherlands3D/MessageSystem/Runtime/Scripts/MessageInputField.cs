using System;

public class MessageInputField : MessageElement
{
    public string Placeholder { get; private set; }
    public Action<string> OnSubmitFunction { get; private set; }
    public MessageInputField(string elementName, Action<string> onSubmitFunction = null, string placeholder = "Enter text...") : base(elementName)
    {
        Placeholder = placeholder;
        OnSubmitFunction = onSubmitFunction;
    }
}
