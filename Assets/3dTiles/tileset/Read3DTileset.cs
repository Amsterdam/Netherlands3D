using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;

public class Read3DTileset : MonoBehaviour
{
    string tilesetUrl = "https://storage.googleapis.com/ahp-research/maquette/kadaster/3dbasisvoorziening/test/landuse_1_1/tileset.json";
    //[SerializeField] importLosseB3dm b3dmImporter;
    
    public TileSet tileset;
    public double[] transformValues;

    public int tilecount;
    public int nestingDepth;

    public GameObject cubePrefab;

    Plane[] viewFrustrum= new Plane[6];
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadTileset());
    }

        IEnumerator LoadTileset()
    {

        UnityWebRequest www = UnityWebRequest.Get(tilesetUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {

            string jsonstring = www.downloadHandler.text;
            tileset = JsonConvert.DeserializeObject<TileSet>(jsonstring);
            tileset.filepath = tilesetUrl;

        }

        

        
    }

    public void LoadTile(Tile tile)
    {
        Debug.Log("hoi");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetViewFrustrum()
    {
        viewFrustrum = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        //return GeometryUtility.TestPlanesAABB(viewFrustrum, renderer.bounds);
    }

    public void SetSSEComponent()
    {
        float ssecomponent = Screen.height / (2 * Mathf.Tan(Mathf.Deg2Rad * Camera.main.fieldOfView / 2));
        // multiply with Geomettric Error and
        // divide by distance to camera
        // to get the screenspaceError in pixels;

    }
}
