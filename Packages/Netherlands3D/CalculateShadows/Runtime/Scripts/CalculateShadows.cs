using System.Collections;
using UnityEngine;
using Netherlands3D.Events;

public class CalculateShadows : MonoBehaviour
{
    [SerializeField]
    private GameObject resultTextureObject;
    [SerializeField]
    private Material resultMaterial;
    [SerializeField]
    private GameObject resultProjectorObject;
    [SerializeField]
    private bool moveWithCamera;

    [SerializeField]
    private string[] textureRefs;
    private int textureIndex = 0;
    private int originalHour;
    private int currentHour;

    [Header("Invoke events")]
    [SerializeField]
    private IntEvent setSunHour;
    [SerializeField]
    private BoolEvent resetShadows;
    [SerializeField]
    private TriggerEvent getSunTime;
    [Header("Listen to events")]
    [SerializeField]
    private DateTimeEvent receiveSunTime;

    [Space(20)]
    [SerializeField]
    private GameObject areaFrame;
    [SerializeField]
    private RenderTexture camTexture;

    private bool resultShown = false;
    private Camera mainCam;
    private Camera currentCam;

    void Start()
    {
        mainCam = Camera.main;
        currentCam = this.gameObject.GetComponent<Camera>();
        currentCam.enabled = false;
        if(receiveSunTime) receiveSunTime.AddListener(ReceiveSunTime);
    }

    void Update()
    {

    }

    public void StartCalc()
    {
        currentCam.enabled = true;
        // Get original hours
        getSunTime?.started.Invoke();

        currentHour = 8; // Start at 8 in the morning
        setSunHour.Invoke(currentHour);

        StartCoroutine(PopulateTextures());
    }

    IEnumerator PopulateTextures()
    {
        resultTextureObject.SetActive(false);
        resultProjectorObject.SetActive(true);
        resultMaterial.SetTexture("_MainTex", null);

        for (int i = 0; i <= textureRefs.Length; i++)
        {
            yield return new WaitForEndOfFrame();
            StartCoroutine(GetScreenshot(currentCam));
            // Set sun
            currentHour++;
            setSunHour.Invoke(currentHour);
        }

        // Take resulting screenshot
        resultTextureObject.SetActive(true);
        StartCoroutine(SetResultTexture(currentCam));

        resultShown = true;

        yield break;
    }

    IEnumerator GetScreenshot(Camera camera)
    {
        camera.Render();

        Rect regionToReadFrom = new Rect(Screen.width / 2 - 512, Screen.height / 2 - 512, 1024, 1024);

        Texture2D image = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
        Texture2D myTexture = ToTexture2D(camTexture);

        yield return new WaitForEndOfFrame();

        if (textureIndex < textureRefs.Length)
        {
            resultTextureObject.GetComponent<MeshRenderer>().material.SetTexture(textureRefs[textureIndex], myTexture);
        }

        textureIndex++;
    }

    IEnumerator SetResultTexture(Camera camera)
    {
        camera.Render();

        Rect regionToReadFrom = new Rect(Screen.width / 2 - 512, Screen.height / 2 - 512, 1024, 1024);

        Texture2D image = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
        Texture2D myTexture = ToTexture2D(camTexture);

        yield return new WaitForEndOfFrame();

        resultMaterial.SetTexture("_MainTex", myTexture);

        // Reset things
        setSunHour.Invoke(originalHour);
        textureIndex = 0;
        resultTextureObject.SetActive(false);
        currentCam.enabled = false;
    }

    Texture2D ToTexture2D(RenderTexture rTex) // Helper
    {
        Texture2D tex = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

    public void ResetShadows()
    {
        resultProjectorObject.SetActive(false);
        resultMaterial.SetTexture("_MainTex", null);
        for (int i = 0; i < textureRefs.Length; i++)
        {
            resultTextureObject.GetComponent<MeshRenderer>().material.SetTexture(textureRefs[textureIndex], null);
        }
        resultShown = false;
        resetShadows?.started.Invoke(false);
    }

    void ReceiveSunTime(System.DateTime dateTime)
    {
        originalHour = dateTime.Hour;
    }
}