using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessageSystem : MonoBehaviour
{
    public static MessageSystem Instance;
    [SerializeField] private LayoutGroup messageGroup;

    [SerializeField] private TMP_Text textPrefab;
    [SerializeField] private TMP_InputField inputFieldPrefab;
    [SerializeField] private Button buttonPrefab;

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogWarning("Multiple MessageSystems exist! This should not happen!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        messageGroup.gameObject.SetActive(false);
    }

    public void DisplayMessage(params MessageElement[] messageElements)
    {
        messageGroup.gameObject.SetActive(true);
        messageGroup.transform.ClearAllChildren();
        foreach(MessageElement element in messageElements)
        {
            switch (element)
            {
                case MessageText:
                    TMP_Text txt = Instantiate(textPrefab, messageGroup.transform);
                    MessageText mText = (MessageText)element;
                    txt.text = mText.Message;
                    txt.fontSize = mText.FontSize;
                    txt.color = mText.TextColor;
                    txt.name = mText.ElementName;
                    break;
                case MessageInputField:
                    TMP_InputField field = Instantiate(inputFieldPrefab, messageGroup.transform);
                    MessageInputField mField = (MessageInputField)element;
                    if(mField.OnSubmitFunction != null)
                    {
                        field.onSelect.AddListener((string s) => mField.OnSubmitFunction(s));
                    }
                    field.text = mField.Placeholder;
                    field.name = mField.ElementName;
                    break;
                case MessageButton:
                    Button btn = Instantiate(buttonPrefab, messageGroup.transform);
                    MessageButton mButton = (MessageButton)element;
                    btn.onClick.AddListener(mButton.ButtonFunction.Invoke);
                    btn.name = mButton.ElementName;
                    break;
                default:
                    throw new System.NotImplementedException("Attempting to evaluate a MessageElement that doesn't exist! " +
                        "Either add it to the cases or remove it!");

            }
        }
    }
    public void CloseMessage()
    {
        messageGroup.transform.ClearAllChildren();
        messageGroup.gameObject.SetActive(false);
    }

}
