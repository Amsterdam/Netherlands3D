using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
using System.IO;

public class SelectionExport : MonoBehaviour
{
    #region Events
    [Header("listening To (required:")]
    [SerializeField] GameObjectEvent registerGameobject;
    [SerializeField] GameObjectEvent unregisterGameobject;
    [SerializeField] Vector3ListEvent receiveBoundaryPolygon;
    [SerializeField] TriggerEvent startClipping;
    [SerializeField] BoolEvent HasTilehandlerFineshedLoading;
    
    [Header("listening To (optional:")]
    [SerializeField] StringEvent ExportFileType; //needed?
    [SerializeField] TriggerEvent CancelClipping;
    [Header("progressEvents (optional)")]
    [SerializeField] BoolEvent startedFinished;
    [SerializeField] StringEvent progresstext;
    [SerializeField] StringEvent errorMessage;

    [Header("outputEvents")]
    [SerializeField] StringListEvent resultingData; //only listen if we are expecting data.
    List<string> resultingFiles;
    #endregion


    #region input
    List<GameObject> selectedGameObjects = new List<GameObject>();
    List<Vector3> boundaryPolygon = new List<Vector3>();
    List<Vector2> boundaryPolygon2D = new List<Vector2>();
    Bounds boundaryBounds;
    #endregion


    ClipConcave clipper;
    public LineRenderer clipboundary;
    void ReadClipLineRenderer()
    {
        boundaryBounds = new Bounds();
        boundaryPolygon.Clear();
        for (int i = 0; i < clipboundary.positionCount; i++)
        {
            boundaryPolygon.Add(clipboundary.GetPosition(i));
            boundaryBounds.Encapsulate(clipboundary.GetPosition(i));
        }
    }
   


    private void Awake()
    {
        registerGameobject.started.AddListener(OnRegisterGameObject);
        unregisterGameobject.started.AddListener(OnUnregisterGameObject);
        receiveBoundaryPolygon.started.AddListener(OnReceiveBoundaryPolygon);
        startClipping.started.AddListener(OnStartClipping);
    }


    void OnRegisterGameObject(GameObject value)
    {
        if (!selectedGameObjects.Contains(value))
        {
            selectedGameObjects.Add(value);
        }
    }
    void OnUnregisterGameObject(GameObject value)
    {
        if (selectedGameObjects.Contains(value))
        {
            selectedGameObjects.Remove(value);
        }
    }
    void OnReceiveBoundaryPolygon(List<Vector3> value)
    {
        boundaryPolygon = value;
        boundaryBounds = new Bounds();

        for (int i = 0; i < boundaryPolygon.Count; i++)
        {
            boundaryBounds.Encapsulate(boundaryPolygon[i]);
        }
    }

    void OnStartClipping()
    {
        ReadClipLineRenderer();
        clipper = new ClipConcave();
        //check if input is complete
        if (boundaryPolygon is null)
        {
            return;
        }
        if (selectedGameObjects.Count == 0)
        {
            // nothing to export
            return;
        }
        StartCoroutine(Clipit());
    }
    IEnumerator Clipit()
    {
        List<Vector3> triangleVertices = new List<Vector3>();
        triangleVertices.Add(Vector3.zero);
        triangleVertices.Add(Vector3.zero);
        triangleVertices.Add(Vector3.zero);

        List<Vector3> ClippedVertices = new List<Vector3>();
        Mesh thismesh;
        string layerName;
        System.DateTime datetime = System.DateTime.Now;

        bool resultAvailable = false;
        foreach (var gameobject in selectedGameObjects)
        {
            MeshFilter[] meshfilters = gameobject.GetComponentsInChildren<MeshFilter>(false);
            foreach (var meshfilter in meshfilters)
            {
                UpdatePolygon(meshfilter.gameObject.transform.position);
                if (!boundaryBounds.Intersects(meshfilter.gameObject.GetComponent<MeshRenderer>().bounds))
                {
                    Debug.Log(meshfilter.gameObject.name + " is outside");
                    continue;
                }
                Debug.Log(meshfilter.gameObject.name + " overlaps");
                thismesh = meshfilter.sharedMesh;
                Vector3[] vertices = thismesh.vertices;

                clipper.setBoundary(boundaryPolygon2D);
                for (int i = 0; i < thismesh.subMeshCount; i++)
                {
                    layerName = getlayerName(meshfilter.gameObject.GetComponent<MeshRenderer>().sharedMaterials[i].name)+".bin";
                    string fileName = Path.Combine(Application.persistentDataPath,layerName);
                    Debug.Log(fileName);
                    if (!resultingFiles.Contains(fileName))
                    {
                        resultingFiles.Add(fileName);
                    }
                    int[] indices = thismesh.GetIndices(i);
                    int indicescount = indices.Length;
                    uint startindex = thismesh.GetBaseVertex(i);
                    using (var stream = File.Open(fileName, FileMode.OpenOrCreate))
                    {
                        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, false))
                        {
                            for (int ind = 0; ind < (indicescount - 4); ind += 3)
                            {
                                triangleVertices[0] = vertices[indices[ind + startindex]];
                                triangleVertices[1] = vertices[indices[ind + startindex+1]];
                                triangleVertices[2] = vertices[indices[ind + startindex + 2]];
                                resultAvailable = clipper.setTriangle(triangleVertices);
                                while(resultAvailable)
                                {
                                    ClippedVertices.Clear();
                                    resultAvailable = clipper.FindNextPolygon(ClippedVertices);
                                    if (ClippedVertices.Count > 4)
                                    {
                                        Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();
                                        poly.outside = ClippedVertices;
                                        Mesh tempmesh = Poly2Mesh.CreateMesh(poly);
                                        List<Vector3> polyverts = new List<Vector3>();
                                        tempmesh.GetVertices(polyverts);

                                        int[] polyints = tempmesh.GetIndices(0);
                                        for (int j = 0; j < polyints.Length; j++)
                                        {
                                            Vector3 point = polyverts[polyints[j]];
                                            writer.Write(point.x);
                                            writer.Write(point.y);
                                            writer.Write(point.z);
                                        }

                                    }
                                    if (ClippedVertices.Count == 4)
                                    {
                                        // write 0-2-3
                                        writer.Write(ClippedVertices[0].x);
                                        writer.Write(ClippedVertices[0].y);
                                        writer.Write(ClippedVertices[0].z);
                                        writer.Write(ClippedVertices[2].x);
                                        writer.Write(ClippedVertices[2].y);
                                        writer.Write(ClippedVertices[2].z);
                                        writer.Write(ClippedVertices[3].x);
                                        writer.Write(ClippedVertices[3].y);
                                        writer.Write(ClippedVertices[3].z);
                                    }
                                    if (ClippedVertices.Count > 2)
                                    {
                                        // write 0-1-2
                                        writer.Write(ClippedVertices[0].x);
                                        writer.Write(ClippedVertices[0].y);
                                        writer.Write(ClippedVertices[0].z);
                                        writer.Write(ClippedVertices[1].x);
                                        writer.Write(ClippedVertices[1].y);
                                        writer.Write(ClippedVertices[1].z);
                                        writer.Write(ClippedVertices[2].x);
                                        writer.Write(ClippedVertices[2].y);
                                        writer.Write(ClippedVertices[2].z);
                                    }


                                    if ((System.DateTime.Now - datetime).TotalMilliseconds > 400)
                                    {
                                        yield return null;
                                        datetime = System.DateTime.Now;
                                    }
                                }
                                writer.Flush();
                            }

                        }
                    }

                }
            }

        }

        Debug.Log("finished");
    }

    void UpdatePolygon(Vector3 offset)
    {
        boundaryBounds = new Bounds();
        boundaryPolygon2D.Clear();
        for (int i = 0; i < boundaryPolygon.Count; i++)
        {
            boundaryBounds.Encapsulate(boundaryPolygon[i]);
            boundaryPolygon2D.Add(new Vector2(boundaryPolygon[i].x - offset.x, boundaryPolygon[i].z - offset.z));
        }
        //boundaryBounds.Encapsulate(boundaryBounds.center-new Vector3(0,-1000,0));
       // boundaryBounds.Encapsulate(boundaryBounds.center - new Vector3(0, +1000, 0));
    }

    string getlayerName(string materialname)
    {
        string layerName = materialname.Replace(" (Instance)", "");
        layerName = layerName.Replace("=", "");
        layerName = layerName.Replace("\\", "");
        layerName = layerName.Replace("<", "");
        layerName = layerName.Replace(">", "");
        layerName = layerName.Replace("/", "");
        layerName = layerName.Replace("?", "");
        layerName = layerName.Replace("\"", "");
        layerName = layerName.Replace(":", "");
        layerName = layerName.Replace(";", "");
        layerName = layerName.Replace("*", "");
        layerName = layerName.Replace("|", "");
        layerName = layerName.Replace(",", "");
        layerName = layerName.Replace("'", "");
        return layerName;
    }

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
