using Netherlands3D.Events;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class AnnotationHeaderText : MonoBehaviour
{
    private TextMeshProUGUI text;
    private string defaultText;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        defaultText = text.text;
    }

    public void SetText(int id)
    {
        text.text = "Annotation " + id;
    }

    public void ResetText()
    {
        text.text = defaultText;
    }
}
