using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using Netherlands3D.Events;
using System.Globalization;
using Netherlands3D.Core;
using System.Collections.Generic;

[RequireComponent(typeof(TMP_InputField))]

public class AddressSearch : MonoBehaviour
{
    private TMP_InputField searchInputField;
    private Camera mainCam;

    [Header("Listen to events")]
    [SerializeField]
    private TriggerEvent clearInput;


    [Header("Invoke events")]
    [SerializeField]
    private BoolEvent toggleClearButton;
    [SerializeField]
    private StringListEvent gotBuilding;

    [Space(10)]
    [SerializeField]
    private string searchWithinCity = "Amsterdam";
    [SerializeField]
    private int charactersNeededBeforeSearch = 2;
    [SerializeField]
    private GameObject resultsParent;
    [SerializeField]
    public AnimationCurve cameraMoveCurve;

    public string SearchInput { get => searchInputField.text; set => searchInputField.text = Regex.Replace(value, "<.*?>", string.Empty); }

    private const string REPLACEMENT_STRING = "{SEARCHTERM}";

    public bool IsFocused => searchInputField.isFocused;

    private void Start()
    {
        mainCam = Camera.main;

        searchInputField = GetComponent<TMP_InputField>();
        searchInputField.onValueChanged.AddListener(delegate { GetSuggestions(searchInputField.text); });

        if (clearInput) clearInput.AddListenerStarted(ClearInput);
    }

    public void ClearInput()
    {
        ClearSearchResults();
        searchInputField.text = "";
        GetSuggestions(searchInputField.text);
        searchInputField.Select();
    }

    public void GetSuggestions(string textInput = "")
    {
        var inputNotEmpty = (textInput != "");
        StopAllCoroutines();

        if (inputNotEmpty)
        {
            if (textInput.Length > charactersNeededBeforeSearch)
            {
                if (toggleClearButton) toggleClearButton.InvokeStarted(true);
                StartCoroutine(FindSearchSuggestions(textInput));
            }
            else
            {
                ClearSearchResults();
            }
        }
        else
        {
            ClearSearchResults();
            toggleClearButton.InvokeStarted(false);
        }
    }

    IEnumerator FindSearchSuggestions(string searchTerm)
    {
        string urlEncodedSearchTerm = UnityWebRequest.EscapeURL(searchTerm);
        string url = "https://geodata.nationaalgeoregister.nl/locatieserver/v3/suggest?q=" + urlEncodedSearchTerm + "%20and%20" + searchWithinCity + "%20and%20type:adres&rows=5".Replace(REPLACEMENT_STRING, urlEncodedSearchTerm);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Geen verbinding");
            }
            else
            {
                ClearSearchResults();

                string jsonStringResult = webRequest.downloadHandler.text;
                var JSONobj = SimpleJSON.JSON.Parse(jsonStringResult);
                var docs = JSONobj["response"]["docs"];

                for (int i = 0; i < docs.Count; i++)
                {
                    var weergavenaam = docs[i]["weergavenaam"];
                    var ID = docs[i]["id"];

                    GenerateResultItem(weergavenaam, ID);
                }
            }
        }
    }

    private void ClearSearchResults()
    {
        foreach (Transform child in resultsParent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    private void GenerateResultItem(string weergavenaam, string ID)
    {
        GameObject NewObj = new GameObject();
        NewObj.name = weergavenaam;

        RectTransform rt = NewObj.AddComponent<RectTransform>();
        rt.SetParent(resultsParent.transform);
        rt.localScale = new Vector3(1, 1, 1);
        rt.sizeDelta = new Vector2(160, 10);

        NewObj.SetActive(true);

        TextMeshProUGUI tmpComp = NewObj.AddComponent<TextMeshProUGUI>();
        tmpComp.color = Color.black;
        tmpComp.fontSize = 10;
        tmpComp.text = weergavenaam;
        tmpComp.margin = new Vector4(10, 10, 10, 10);

        Button btnComp = NewObj.AddComponent<Button>();
        btnComp.onClick.AddListener(delegate { GeoDataLookup(ID, weergavenaam); });
    }

    void GeoDataLookup(string ID, string weergavenaam)
    {
        searchInputField.onValueChanged.RemoveAllListeners();
        searchInputField.text = weergavenaam;
        StartCoroutine(GeoDataLookupRoutine(ID));
        ClearSearchResults();
        searchInputField.onValueChanged.AddListener(delegate { GetSuggestions(searchInputField.text); });

    }

    IEnumerator GeoDataLookupRoutine(string ID)
    {
        string url = "https://geodata.nationaalgeoregister.nl/locatieserver/v3/lookup?id=" + ID;
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Geen verbinding");
            }
            else
            {
                string jsonStringResult = webRequest.downloadHandler.text;
                var JSONobj = SimpleJSON.JSON.Parse(jsonStringResult);
                var docs = JSONobj["response"]["docs"];

                string centroid = docs[0]["centroide_ll"];
                string verblijfsobject_id = docs[0]["adresseerbaarobject_id"];

                Vector3 targetLocation = ExtractUnityLocation(ref centroid);
                var targetPos = new Vector3((targetLocation.x), 300, (targetLocation.z - 300));
                var targetRot = Quaternion.Euler(45, 0, 0);

                StartCoroutine(LerpCamera(mainCam.gameObject, targetPos, targetRot, 2));
                yield return new WaitForSeconds(2);
                StartCoroutine(GetBAGID(verblijfsobject_id));
            }
        }
        yield return null;
    }

    private static Vector3 ExtractUnityLocation(ref string locationData)
    {
        locationData = locationData.Replace("POINT(", "").Replace(")", "").Replace("\"", "");
        string[] lonLat = locationData.Split(' ');

        double.TryParse(lonLat[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double lon);
        double.TryParse(lonLat[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double lat);

        Vector3 unityLocation = CoordConvert.WGS84toUnity(lon, lat);
        return unityLocation;
    }

    IEnumerator LerpCamera(GameObject targetObj, Vector3 endPos, Quaternion endRot, float duration)
    {
        float t = 0;
        Vector3 startPos = targetObj.transform.position;
        Quaternion startRot = targetObj.transform.rotation;
        while (t < duration)
        {
            targetObj.transform.position = Vector3.Lerp(startPos, endPos, cameraMoveCurve.Evaluate(t / duration));
            targetObj.transform.rotation = Quaternion.Lerp(startRot, endRot, cameraMoveCurve.Evaluate(t / duration));
            t += Time.deltaTime;
            yield return null;
        }
        targetObj.transform.position = endPos;
    }

    IEnumerator GetBAGID(string verblijfsobject_id)
    {
        string pdokVerblijfsobjectWFS =
        "https://service.pdok.nl/lv/bag/wfs/v2_0?SERVICE=WFS&VERSION=2.0.0&outputFormat=geojson&REQUEST=GetFeature&typeName=bag:verblijfsobject&count=100&outputFormat=xml&srsName=EPSG:28992&filter=<Filter><PropertyIsEqualTo><PropertyName>identificatie</PropertyName><Literal>" + verblijfsobject_id + "</Literal></PropertyIsEqualTo></Filter>";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(pdokVerblijfsobjectWFS))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Geen verbinding");
            }
            else
            {
                string jsonStringResult = webRequest.downloadHandler.text;
                var JSONobj = SimpleJSON.JSON.Parse(jsonStringResult);
                var BAGID = JSONobj["features"][0]["properties"]["pandidentificatie"];

                Debug.Log("BAG ID: " + BAGID);

                List<string> bagIDs = new List<string>();
                bagIDs.Add(BAGID);

                if (gotBuilding) gotBuilding.InvokeStarted(bagIDs);
            }
        }
    }

}