using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DualTextComponent : MonoBehaviour
{

    public Button closeButton;

    [SerializeField] private Text mainText;
    [SerializeField] private Text subText;

    private void Awake()
    {
        if (mainText == null || subText == null)
        {
            throw new System.NullReferenceException("Text elements haven't been properly assigned!");
        }
    }

    public void SetMainText(string newText)
    {
        mainText.text = newText;
    }

    public void SetSubText(string newText)
    {
        subText.text = newText;
    }

    public string GetMainText() => mainText.text;
    public string GetSubText() => subText.text;

}
