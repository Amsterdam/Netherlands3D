
public abstract class MessageElement
{
    public string ElementName { get; protected set; }
    public MessageElement(string elementName)
    {
        ElementName = elementName;
    }

}
