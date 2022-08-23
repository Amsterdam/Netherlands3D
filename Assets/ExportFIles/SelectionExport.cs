using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
using System.IO;
using Netherlands3D.FileExport.DXF;
using System.Runtime.InteropServices;
using System.Text;

public class SelectionExport : MonoBehaviour
{


    #region Events



    [Header("geometry-interaction")]
    [Tooltip("called by Geometry-objects that want their data clipped")]
    [SerializeField] GameObjectEvent registerGameobject;
    [Tooltip("called by Geometry-objects that don't want their data clipped anymore")]
    [SerializeField] GameObjectEvent unregisterGameobject;

    [Tooltip("(optional) tells the tilehandler to pause loading and changing objects")]
    [SerializeField] BoolEvent PauseTileHandler;
    [Tooltip("(optional) if set: waits for this call, bwfore starting the clipping-procedure")]
    [SerializeField] BoolEvent TileHandlerIsBusy;


    [Header ("clipsettings")]
    [Tooltip("tells the clipper alongt which boundary to clip")]
    [SerializeField] Vector3ListEvent receiveBoundaryPolygon;
    [Tooltip("event is sent when a Boundary-polygon has been received")]//boundary-polygon-creator should de-activate itself after creating a polygon?
    [SerializeField] BoolEvent DeactivatePolygonSelection;
    [Tooltip("which filetype to create. dxf or collada")]
    [SerializeField] StringEvent ExportFileType;

    [SerializeField] TriggerEvent startClipping;
    


    [Header("progressEvents (optional)")]
    [SerializeField] BoolEvent startedFinished;
    [SerializeField] StringEvent progresstext;
    [SerializeField] FloatEvent progresspercentage;
    [SerializeField] StringEvent errorMessage;

    [Header("not yet inplemented")]
    [SerializeField] TriggerEvent CancelClipping;


    List<string> resultingFiles = new List<string>();
    #endregion


    #region input
    List<GameObject> selectedGameObjects = new List<GameObject>();
    List<Vector3> boundaryPolygon = new List<Vector3>();
    #endregion

    #region variables
    string filetype;
    bool tilehandlerIsBusy;
    List<MeshFilter> meshFiltersToClip = new List<MeshFilter>();
    long trianglesToClipCount;    
    List<Vector2> boundaryPolygon2D = new List<Vector2>();
    Bounds boundaryBounds;
    Geometryfile outputcreator;
    #endregion


    ClipConcave clipper;


    private void Awake()
    {
        registerGameobject.started.AddListener(OnRegisterGameObject);
        unregisterGameobject.started.AddListener(OnUnregisterGameObject);
        receiveBoundaryPolygon.started.AddListener(OnReceiveBoundaryPolygon);
        startClipping.started.AddListener(OnStartClipping);
        TileHandlerIsBusy.started.AddListener(OnTileHandlerChangedStatus);
        if (ExportFileType)
        {
            ExportFileType.started.AddListener(OnFileTypeChanged);
        }

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
       
        
        boundaryPolygon.Clear();
        boundaryBounds = new Bounds();

        for (int i = 0; i < value.Count; i++)
        {
            boundaryPolygon.Add(value[i]);
            boundaryBounds.Encapsulate(boundaryPolygon[i]);
        }
        if (errorMessage)
        {
            errorMessage.started.Invoke("");
        }
        if (DeactivatePolygonSelection)
        {
            DeactivatePolygonSelection.started.Invoke(false);
        }
    }

    void OnTileHandlerChangedStatus(bool value)
    {
        tilehandlerIsBusy = value;
    }
    void OnFileTypeChanged(string value)
    {
        if (value == "collada")
        {
            filetype = value;
        }
        else if (value == "dxf")
        {
            filetype = value;
        }
        else
        {
            Debug.LogError($"(value) is not a valid filetype");
        }
    }

    void OnStartClipping()
    {
        //ReadClipLineRenderer();
        clipper = new ClipConcave();
        //check if input is complete
        if (boundaryPolygon.Count==0)
        {
            if (errorMessage)
            {
                errorMessage.started.Invoke("please select an area to download first");
            }
            return;
        }
        if (errorMessage)
        {
            errorMessage.started.Invoke("");
        }


        if (selectedGameObjects.Count == 0)
        {
            // nothing to export
            return;
        }
        StartCoroutine(Clipit());
    }

    void BroadcastProgressPercentage(float value)
    {
        if (progresspercentage!=null)
        {
            progresspercentage.started.Invoke(value);
        }
    }

    void BroadcastProgresstext(string value)
    {
        if (progresstext != null)
        {
            progresstext.started.Invoke(value);
        }
    }
    void BroadcastStartedFInished(bool value)
    {
        if (startedFinished!=null)
        {
            startedFinished.started.Invoke(value);
        }
    }
    void SelectGameObjects()
    {
        meshFiltersToClip.Clear();
        int submeshcount;
        trianglesToClipCount = 0;
        foreach (var gameobject in selectedGameObjects)
        {
            MeshFilter[] meshfilters = gameobject.GetComponentsInChildren<MeshFilter>(false);
            foreach (var meshfilter in meshfilters)
            {
                if (!boundaryBounds.Intersects(meshfilter.gameObject.GetComponent<MeshRenderer>().bounds))
                {
                   //this mesh is completely outside the selectionpolygon
                    continue;
                }
                meshFiltersToClip.Add(meshfilter);
                submeshcount = meshfilter.sharedMesh.subMeshCount;
                for (int i = 0; i < submeshcount; i++)
                {
                    trianglesToClipCount += meshfilter.sharedMesh.GetIndexCount(i);
                }
            }
        }
    }

    IEnumerator Clipit()
    {
        //set up all the variables
        System.DateTime datetime = System.DateTime.Now;
        List<Vector3> triangleVertices = new List<Vector3>();
        triangleVertices.Add(Vector3.zero);
        triangleVertices.Add(Vector3.zero);
        triangleVertices.Add(Vector3.zero);
        List<Vector3> ClippedVertices = new List<Vector3>();
        Mesh thismesh;
        string layerName;
        long trianglesprocessed=0;
        Vector3 offset;
        bool resultAvailable;
        resultingFiles.Clear();
        BroadcastStartedFInished(true);
        

        //pause the tilehandler
        if (PauseTileHandler)
        {
            PauseTileHandler.started.Invoke(true);
        }
        //wait for the tilehandler to finish
        BroadcastProgresstext("Waiting for all the data to be loaded...");
        while (tilehandlerIsBusy)
        {
            yield return null;
        }

        // clip all the gameobjects
        BroadcastProgresstext("Cutting out the model.");
        SelectGameObjects();
        foreach (var meshfilter in meshFiltersToClip)
            {
            offset = meshfilter.gameObject.transform.position;
                UpdatePolygon(offset);
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
                            for (int ind = 0; ind < (indicescount - 3); ind += 3)
                            {
                                triangleVertices[0] = vertices[indices[ind + startindex]];
                                triangleVertices[1] = vertices[indices[ind + startindex+1]];
                                triangleVertices[2] = vertices[indices[ind + startindex + 2]];
                                resultAvailable = clipper.setTriangle(triangleVertices);
                            trianglesprocessed+=3;
                                while(resultAvailable)
                                {
                                    ClippedVertices.Clear();
                                    resultAvailable = clipper.FindNextPolygon(ClippedVertices);
                                //move the vertices to worldspace
                                for (int j = 0; j < ClippedVertices.Count; j++)
                                {
                                    ClippedVertices[j] += offset;
                                }
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
                                    BroadcastProgressPercentage(100 * trianglesprocessed / trianglesToClipCount);
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


        //pause the tilehandler
        if (PauseTileHandler)
        {
            PauseTileHandler.started.Invoke(false);
        }
        StartCoroutine(WriteToFile());
    }

    void UpdatePolygon(Vector3 offset)
    {
        boundaryPolygon2D.Clear();
        for (int i = 0; i < boundaryPolygon.Count; i++)
        {
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

    IEnumerator WriteToFile()
    {
        outputcreator = null;
        if (filetype=="dxf")
        {
            outputcreator = new dxf();
        }
        if (filetype =="collada")
        {
            outputcreator = new Collada();
        }
        if (outputcreator is null)
        {
            Debug.Log("cannot create " + filetype + "-files");
            yield break; ;
        }
        
        outputcreator.SetupFile(Application.persistentDataPath, "output");
        for (int i = 0; i < resultingFiles.Count; i++)
        {
            string filename = Path.GetFileName(resultingFiles[i]).Replace(".bin","");
            BroadcastProgresstext($"Writing {filename} to file");
            BroadcastProgressPercentage(100 * i / resultingFiles.Count);
            yield return null;
            outputcreator.AddMesh(resultingFiles[i]);
            File.Delete(resultingFiles[i]);
        }
        outputcreator.SaveFile();

        BroadcastStartedFInished(false);
        Debug.Log("file created: "+Path.Combine(Application.persistentDataPath,"output.dxf" ));




    }

   
}
