using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageSystem : MonoBehaviour
{
    public static MessageSystem Instance;
    [SerializeField] private LayoutGroup messageGroup;

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogWarning("Multiple MessageSystems exist! This should not happen!");
            return;
        }
        Instance = this;
    }

    public void DisplayMessage(params MessageElement[] messageElements)
    {
        messageGroup.transform.ClearAllChildren();
        foreach(MessageElement element in messageElements)
        {
            element.GetElement().transform.SetParent(messageGroup.transform);
        }
    }

}
