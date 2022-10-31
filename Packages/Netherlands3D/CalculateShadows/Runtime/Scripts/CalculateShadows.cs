using System.Collections;
using UnityEngine;
using Netherlands3D.Events;

public class CalculateShadows: MonoBehaviour
{
    [SerializeField]
    private GameObject targetObject;
    [SerializeField]
    private Material targetMaterial;

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
    [Header("Listen to events")]
    [SerializeField]
    private DateTimeEvent getCurrentTime;

    private Camera mainCam;
    private Camera currentCam;
    [SerializeField]
    private GameObject areaFrame;
    [SerializeField]
    private RenderTexture camTexture;

    private bool resultShown = false;

    void Start()
    {
        mainCam = Camera.main;
        currentCam = this.gameObject.GetComponent<Camera>();
        if (getCurrentTime) getCurrentTime.started.AddListener(GetCurrentTIme);
    }

    void Update()
    {
        if (!resultShown)
        {
            transform.position = new Vector3(mainCam.transform.position.x, 200, mainCam.transform.position.z);

            transform.localRotation = mainCam.transform.localRotation;
            transform.localRotation = Quaternion.Euler(new Vector3(90, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z));

            transform.position = transform.position + transform.up * 800;

            areaFrame.transform.position = new Vector3(areaFrame.transform.position.x, 120, areaFrame.transform.position.z);
        }
    }

    public void StartCalc()
    {
        currentHour = 8; // Start at 8 in the morning
        setSunHour.Invoke(currentHour);

        StartCoroutine(PopulateTextures());
    }

    IEnumerator PopulateTextures()
    {
        targetObject.SetActive(false);
        targetMaterial.SetTexture("_MainTex", null);

        for (int i = 0; i <= textureRefs.Length; i++)
        {
            yield return new WaitForEndOfFrame();
            StartCoroutine(GetScreenshot(currentCam));
            // Set sun
            currentHour++;
            setSunHour.Invoke(currentHour);
        }

        // Take resulting screenshot
        targetObject.SetActive(true);
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
            targetObject.GetComponent<MeshRenderer>().material.SetTexture(textureRefs[textureIndex], myTexture);
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

        targetMaterial.SetTexture("_MainTex", myTexture);

        // Reset things
        setSunHour.Invoke(originalHour);
        textureIndex = 0;
        // resetShadows.started.Invoke(true);
        targetObject.SetActive(false);
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
        targetMaterial.SetTexture("_MainTex", null);
        for (int i = 0; i < textureRefs.Length; i++)
        {
            targetObject.GetComponent<MeshRenderer>().material.SetTexture(textureRefs[textureIndex], null);
        }
        resultShown = false;
        // resetShadows.started.Invoke(false);
    }

    void GetCurrentTIme(System.DateTime dateTime)
    {
        originalHour = dateTime.Hour;
        Debug.Log(originalHour);
        // Only get once, so remove listener
        getCurrentTime.started.RemoveAllListeners();
    }
}