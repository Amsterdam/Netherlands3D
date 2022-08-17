using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;

public class cliptool : MonoBehaviour
{
    ClipConcave clipper = new ClipConcave();
    bool clipperIsActive = false;
    bool tileDataLoaded = false;


    [Header("For Testing")]
    public GameObject GameobjectToClip;
    public LineRenderer boundary;
    

    
    Mesh resultMesh;
    [SerializeField] MeshFilter resultMeshFilter;
    // Start is called before the first frame update
    void Start()
    {
        //start all the listeners
        //startClipping.started.AddListener(ClipStart);
    }


    public void Clipstart()
    {
        if (clipperIsActive) return;
        clipperIsActive = true;
        //check if boundingPolygon is set


        //wait for tilehandler to finish loading
        StartCoroutine(waitForTiledData());
        //ignore if not yet finished
        //delete old outputfiles?
        

        //when we start, the tilehander should not be loading/replacing/removing data.

    }
    IEnumerator waitForTiledData() // keeps looping until tileDataLoaded is true, then triggers ClipStart
    {

        //send progress-feedback "waiting for data to be loaded"
        if (!tileDataLoaded)
        {
            yield return null;
        }
        tileDataLoaded = true;
        //send progress-feedback "all data loaded"
        //start clipping.

    }

    void ClipStart()
    {
        StartCoroutine(clipTheMesh());
    }

    IEnumerator clipTheMesh()
    {
        Mesh originalMesh = GameobjectToClip.GetComponent<MeshFilter>().sharedMesh;
        Vector3 meshOrigin = GameobjectToClip.transform.position;
        //move the boundary form worldSpace to the localSpace of the mesh;
        Vector2 meshOrigin2d = new Vector2(meshOrigin.x, meshOrigin.z);
        List<Vector2> boundary2d = new List<Vector2>();
        for (int i = 0; i < boundary.positionCount; i++)
        {
            boundary2d.Add(new Vector2(boundary.GetPosition(i).x, boundary.GetPosition(i).z)-meshOrigin2d);
        }
        clipper.setBoundary(boundary2d);



        resultMesh = new Mesh();
        resultMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        Vector3[] originalVertices = originalMesh.vertices;

        List<Vector3> resultVertices = new List<Vector3>();
        List<int> resultIndices = new List<int>();
        
        List<Vector3> ClippedVertices = new List<Vector3>();

        int[] originalIndices = originalMesh.GetIndices(0);
        int indicescount = originalIndices.Length;

        List<Vector3> TriangleToClip = new List<Vector3>();
        TriangleToClip.Add(Vector3.zero);
        TriangleToClip.Add(Vector3.zero);
        TriangleToClip.Add(Vector3.zero);
        uint baseIndex = originalMesh.GetBaseVertex(0);

        System.DateTime datetime = System.DateTime.Now;
        

        bool triangleIsClippable = true;
        for (int i = 0; i < indicescount-3; i+=3)
        {
            if ((System.DateTime.Now-datetime).TotalMilliseconds>400)
            {
                yield return null;

                datetime = System.DateTime.Now;
                
            }
            Vector3 vert1 = originalVertices[originalIndices[i + baseIndex]];
            Vector3 vert2 = originalVertices[originalIndices[i + baseIndex+1]];
            Vector3 vert3 = originalVertices[originalIndices[i + baseIndex+2]];
            TriangleToClip[0] = vert1;
            TriangleToClip[1] = vert2;
            TriangleToClip[2] = vert3;
            triangleIsClippable = clipper.setTriangle(TriangleToClip);
            if (!triangleIsClippable)
            {
                Debug.Log("fout");
            }
            while (triangleIsClippable)
            {
                ClippedVertices.Clear();
                triangleIsClippable = clipper.FindNextPolygon(ClippedVertices);
                //move the vertices from local Space to WorldSpace.
                if (ClippedVertices.Count>2)
                {
                    int vertexcount = ClippedVertices.Count;
                    for (int j = 0; j < vertexcount; j++)
                    {
                        ClippedVertices[j] = ClippedVertices[j] + meshOrigin;
                    }
                }

                    if (ClippedVertices.Count==3)
                    {
                        int startindex = resultIndices.Count;
                        resultVertices.Add(ClippedVertices[0]);
                        resultVertices.Add(ClippedVertices[1]);
                        resultVertices.Add(ClippedVertices[2]);
                        resultIndices.Add(startindex);
                        resultIndices.Add(startindex+1);
                        resultIndices.Add(startindex+2);
                    }
                    else if(ClippedVertices.Count == 4)
                    {
                        int startindex = resultIndices.Count;
                        resultVertices.Add(ClippedVertices[0]);
                        resultVertices.Add(ClippedVertices[1]);
                        resultVertices.Add(ClippedVertices[2]);

                        resultIndices.Add(startindex);
                        resultIndices.Add(startindex + 1);
                        resultIndices.Add(startindex + 2);

                        resultVertices.Add(ClippedVertices[0]);
                        resultVertices.Add(ClippedVertices[2]);
                        resultVertices.Add(ClippedVertices[3]);

                        resultIndices.Add(startindex+3);
                        resultIndices.Add(startindex + 4);
                        resultIndices.Add(startindex + 5);
                    }
            }

        }

        resultMesh.SetVertices(resultVertices);
        resultMesh.SetIndices(resultIndices.ToArray(), MeshTopology.Triangles, 0);
        resultMeshFilter.sharedMesh = resultMesh;
        resultMesh.RecalculateNormals();
        resultMesh.RecalculateBounds();
        Debug.Log(resultMesh.bounds.ToString());
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
