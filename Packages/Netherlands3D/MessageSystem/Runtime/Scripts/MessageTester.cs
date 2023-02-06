using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageTester : MonoBehaviour
{
    [TextArea(3, 10)]
    [SerializeField] private string body;

    // Start is called before the first frame update
    void Start()
    {
        MessageSystem.Instance.DisplayMessage(
            new MessageText("Test Message", 14, Color.black, "Title"),
            new MessageText("Testing message system", 12, Color.black, "Body"),
            new MessageInputField("Input"),
            new MessageButton(MessageSystem.Instance.CloseMessage, "Close Button")
            );
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            MessageSystem.Instance.DisplayMessage(
                new MessageText("Pressed THAT Mouse Button", 20, Color.white, "Title"),
                new MessageText(body, 14, Color.black, "Body"),
                new MessageButton(MessageSystem.Instance.CloseMessage, "CloseButton")
                );
        }
    }


}
