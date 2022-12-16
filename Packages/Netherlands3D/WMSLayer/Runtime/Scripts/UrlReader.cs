using UnityEngine;
using Netherlands3D.Events;
using UnityEngine.UI;

public class UrlReader : MonoBehaviour
{
    public static UrlReader Instance { get; private set; }

    public WMS ActiveWMS { get; private set; } 
    public WFS ActiveWFS { get; private set; }

    [SerializeField] private InputField urlField;

    [Header("Invoked Events")]
    [SerializeField] private TriggerEvent resetReaderEvent;
    [SerializeField] private BoolEvent requestUrlButtonEvent;
    //[SerializeField] private BoolEvent isWMSEvent;

    [SerializeField] private StringEvent wmsCreationEvent;
    [SerializeField] private StringEvent wfsCreationEvent;

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
    public void GetFromURL()
    {
        if (resetReaderEvent != null)
        {
            resetReaderEvent.Invoke();
        }
        
        string url = urlField.text.ToLower();
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new System.InvalidOperationException("You must input a valid URL to read");
        }

        string validatedURL = string.Empty;
        foreach(char c in url)
        {
            if(c == char.Parse("?"))
            {
                break;
            }
            validatedURL += c;
        }
        if (url.Contains("wms"))
        {
            if(wmsCreationEvent != null)
            {
                wmsCreationEvent.Invoke(validatedURL);
                //isWMSEvent.Invoke(true);
            }
        }
        else if (url.Contains("wfs"))
        {
            if(wfsCreationEvent != null)
            {
                wfsCreationEvent.Invoke(validatedURL);
                //isWMSEvent.Invoke(false);

            }
        }
    }
}
