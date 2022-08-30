using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.ModelParsing;
using Netherlands3D.Events;

public class CreateGameObjects : MonoBehaviour
{
    
    public GameObjectDataSet gameObjectData;
    // Start is called before the first frame update
    public Material BaseMaterial;
    
    [HideInInspector]
    public FloatEvent progressPercentage;
    public GameObjectEvent createdGameObject;
    int gameobjectindex;
    int totalgameobjects;


    System.DateTime time;

    GameObject parentobject;
    Dictionary<string, Material> createdMaterials = new Dictionary<string, Material>();


    bool gameObjectCreated = false;
     //GameObject createdGameObject;
    bool meshcreated = false;
    Mesh createdMesh;

    System.Action<GameObject> callback;
    void BroadCastProgress()
    {
        float progress = 100 * gameobjectindex / totalgameobjects;
        if (progressPercentage) progressPercentage.started.Invoke(progress);
    }

    public void Create(System.Action<GameObject> callbackToFunction = null)
    {
        callback = callbackToFunction;
        time = System.DateTime.Now;
        parentobject = new GameObject();
        totalgameobjects = gameObjectData.gameObjects.Count;
        gameobjectindex = 0;
        StartCoroutine(createGameObjects());
    }

    IEnumerator createGameObjects()
    {
        foreach (var gameobjectdata in gameObjectData.gameObjects)
        {
            gameobjectindex++;
            BroadCastProgress();
            gameObjectCreated = false;
            StartCoroutine(AddGameObject(gameobjectdata));
            while (gameObjectCreated==false)
            {
                yield return null;
            }
        }

        if (createdGameObject) createdGameObject.started.Invoke(parentobject);
        if (callback !=null)
        {
            callback(parentobject);
        }
        parentobject = null;
    }


    IEnumerator AddGameObject(GameObjectData gameobjectdata)
    {
        GameObject gameobject = new GameObject();
        gameobject.name = gameobjectdata.name;
        gameobject.transform.parent = parentobject.transform;
        meshcreated = false;
        StartCoroutine(CreateMesh(gameobjectdata.meshdata));
        while (meshcreated==false)
        {
            yield return null;
        }
        if (createdMesh is null)
        {
            gameObjectCreated = true;
            Destroy(gameobject);
            yield break;
        }
        MeshFilter mf = gameobject.AddComponent<MeshFilter>();
        mf.sharedMesh = createdMesh;
        MeshRenderer mr = gameobject.AddComponent<MeshRenderer>();
        List<Material> materiallist = new List<Material>();

        int submeshcount = gameobjectdata.meshdata.submeshes.Count;
        materiallist.Capacity = submeshcount;
        for (int i = 0; i < submeshcount; i++)
        {
            materiallist.Add(getMaterial(gameobjectdata.meshdata.submeshes[i].materialname));
        }
        mr.sharedMaterials = materiallist.ToArray();
        gameObjectCreated = true;
    }

    Material getMaterial(string materialname)
    {
        Material returnmaterial;
        if (createdMaterials.ContainsKey(materialname))
        {
            returnmaterial = createdMaterials[materialname];
        }
        else
        {
            returnmaterial = new Material(BaseMaterial);
            returnmaterial.name = materialname;
            for (int i = 0; i < gameObjectData.materials.Count; i++)
            {
                if (gameObjectData.materials[i].Name==materialname)
                {
                    returnmaterial.color = gameObjectData.materials[i].Diffuse;
                    createdMaterials.Add(materialname, returnmaterial);
                }
            }
        }
        return returnmaterial;
    }


    IEnumerator CreateMesh(MeshData meshdata)
    {
        bool hasnormals = false;
        createdMesh = new Mesh();
        createdMesh.Clear();
        // add vertices
        Vector3List vertices = new Vector3List();
        vertices.SetupReading(meshdata.vertexFileName);
        int vertexcount = vertices.Count();
        if (vertexcount==0)
        {
            Debug.Log(meshdata.name + "has no vertices");
            Destroy(createdMesh);
            vertices.EndReading();
            vertices.RemoveData();
            meshcreated = true;
            yield break;
        }
        Vector3[] meshvector3 = new Vector3[vertexcount];
        for (int i = 0; i < vertexcount; i++)
        {
            if ((System.DateTime.Now-time).TotalMilliseconds>400)
            {
                yield return null;
                time = System.DateTime.Now;
            }
            meshvector3[i] = vertices.ReadItem(i);
        }
        createdMesh.vertices = meshvector3;
        //createdMesh.SetVertices(meshvector3);
        
        vertices.EndReading();
        vertices.RemoveData();

        // add indices
        intList indices = new intList();
        indices.SetupReading(meshdata.indicesFilename);
        int indexcount = indices.numberOfVertices();
        int[] meshindices = new int[indexcount];
        for (int i = 0; i < indexcount; i++)
        {
            if ((System.DateTime.Now - time).TotalMilliseconds > 400)
            {
                yield return null;
                time = System.DateTime.Now;
            }
            meshindices[i] = indices.ReadNext();
        }
        createdMesh.SetIndexBufferParams(indexcount, UnityEngine.Rendering.IndexFormat.UInt32);
        createdMesh.SetIndexBufferData(meshindices, 0, 0, indexcount);

        indices.EndReading();
        indices.RemoveData();

        // add normals
        if (meshdata.normalsFileName!="")
        {
           
            Vector3List normals = new Vector3List();
            normals.SetupReading(meshdata.normalsFileName);
            int normalscount = normals.Count();
            if (normalscount==vertexcount)
            {
                if ((System.DateTime.Now - time).TotalMilliseconds > 400)
                {
                    yield return null;
                    time = System.DateTime.Now;
                }
                hasnormals = true;
                Vector3[] meshnormals = new Vector3[normalscount];
                for (int i = 0; i < normalscount; i++)
                {
                    meshnormals[i] = normals.ReadItem(i);
                }

                createdMesh.normals = meshnormals;
            }
            else
            {
                normals.EndReading();
                normals.RemoveData();
                Debug.Log(meshdata.name + "number of normals != number of vertices");
                Destroy(createdMesh);
                meshcreated = true;
                yield break;

            }
            normals.EndReading();
            normals.RemoveData();

        }

        createdMesh.subMeshCount = meshdata.submeshes.Count;
        for (int i = 0; i < meshdata.submeshes.Count; i++)
        {
            UnityEngine.Rendering.SubMeshDescriptor smd = new UnityEngine.Rendering.SubMeshDescriptor();
            smd.indexStart = (int)meshdata.submeshes[i].startIndex;
            smd.indexCount = (int)meshdata.submeshes[i].Indexcount ;
            smd.topology = MeshTopology.Triangles;
            //              smd.baseVertex = sm.Value.startVertex;
            //              smd.vertexCount = sm.Value.vertexCount;
            createdMesh.SetSubMesh(i, smd);
        }
        if (hasnormals ==false)
        {
            createdMesh.RecalculateNormals();
        }
        if ((System.DateTime.Now - time).TotalMilliseconds > 400)
        {
            yield return null;
            time = System.DateTime.Now;
        }
        createdMesh.RecalculateBounds();
        meshcreated = true;
    }
}
