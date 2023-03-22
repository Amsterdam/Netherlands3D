using UnityEngine;
using Netherlands3D.Events;
using TMPro;
using System.Xml;
using UnityEngine.Networking;
using System.Collections;

public enum WebServiceType { NONE, WMS, WFS };

public class UrlReader : MonoBehaviour
{
    public static UrlReader Instance { get; private set; }

    public WMS ActiveWMS { get; private set; } 
    public WFS ActiveWFS { get; private set; }

    [SerializeField] private TMP_InputField wfsInputField;
    [SerializeField] private TMP_InputField wmsInputField;


    [Header("Invoked Events")]
    [SerializeField] private TriggerEvent resetReaderEvent;
    [SerializeField] private BoolEvent requestUrlButtonEvent;
    //[SerializeField] private BoolEvent isWMSEvent;

    [SerializeField] private StringEvent wmsCreationEvent;
    [SerializeField] private StringEvent wfsCreationEvent;

    private string xmlResult;

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogWarning("Instance has already been set, duplicate reader found!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if(requestUrlButtonEvent != null)
        {
            requestUrlButtonEvent.Invoke(Application.isEditor);
        }
    }
    public void ReadAsWMS()
    {
        wmsCreationEvent.Invoke(ValidateUrl(wmsInputField.text));
    }
    public void ReadAsWFS()
    {
        wfsCreationEvent.Invoke(ValidateUrl(wfsInputField.text));
    }

    private string ValidateUrl(string url)
    {
        if (resetReaderEvent)
            resetReaderEvent.InvokeStarted();

        url.Trim();
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new System.InvalidOperationException("You must input a valid URL to read");
        }

        string validatedURL = string.Empty;
        foreach (char c in url)
        {
            if (c == char.Parse("?"))
            {
                break;
            }
            validatedURL += c;
        }

        return validatedURL;

    }
    private IEnumerator GetWebString(string url)
    {

        while(xmlResult == null)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                xmlResult = request.downloadHandler.text;
            }

        }

    }
}
