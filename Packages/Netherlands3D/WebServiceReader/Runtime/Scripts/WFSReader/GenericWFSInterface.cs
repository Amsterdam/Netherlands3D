using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenericWFSInterface : MonoBehaviour
{
    public string BaseUrl { get; private set; }
    public string Index { get; private set; } = "0";
    public string Count { get; private set; } = "1000";
    public string MinX { get; private set; }
    public string MaxX { get; private set; }
    public string MinY { get; private set; }
    public string MaxY { get; private set; }

    [SerializeField] private Button buttonPrefab;
    [SerializeField] private RectTransform featureLayoutGroup;

    [SerializeField] private StringEvent sendWfsUrlEvent;
    [SerializeField] private StringEvent sendFeatureRequestEvent;
    [SerializeField] private StringEvent sendIndexEvent;
    [SerializeField] private StringEvent sendCountEvent;
    [SerializeField] private StringListEvent onFeatureListReceived;

    public void SetBaseUrl(string url) => BaseUrl = url;
    public void SetStartIndex(string index)
    {
        if (sendIndexEvent)
        {
            sendIndexEvent.InvokeStarted(index);
        }
    }
    public void SetWebFeatureCount(string count)
    {
        if (sendCountEvent)
        {
            sendCountEvent.InvokeStarted(count);
        }
    }
    //public void SetBoundsMinX(string boundMinX) => MinX = boundMinX;
    //public void SetBoundsMaxX(string boundMaxX) => MaxX = boundMaxX;
    //public void SetBoundsMinY(string boundMinY) => MinY = boundMinY;
    //public void SetBoundsMaxY(string boundMaxY) => MaxY = boundMaxY;

    private void OnEnable()
    {
        onFeatureListReceived.AddListenerStarted(UpdateFeatureButtons);
    }
    private void OnDisable()
    {
        onFeatureListReceived.RemoveListenerStarted(UpdateFeatureButtons);
    }
    public void RequestWFS()
    {
        if (sendWfsUrlEvent)
        {
            sendWfsUrlEvent.InvokeStarted(BaseUrl);
        }
    }
    public void ResetFeatureButtons()
    {
        foreach(RectTransform button in featureLayoutGroup)
        {
            button.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(button.gameObject);
        }
    }
    private void UpdateFeatureButtons(List<string> featureNames)
    {
        ResetFeatureButtons();
        foreach(string f in featureNames)
        {
            Button featureButton = Instantiate(buttonPrefab, featureLayoutGroup);
            featureButton.GetComponentInChildren<TMPro.TMP_Text>().text = f;
            featureButton.onClick.AddListener(() => RequestFeature(f));
        }
    }
    private void RequestFeature(string featureName)
    {
        if (sendFeatureRequestEvent)
        {
            sendFeatureRequestEvent.InvokeStarted(featureName);
        }
    }

}
