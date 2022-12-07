using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.ModelParsing;


public class CreateGameObjects : MonoBehaviour
{
    
    [HideInInspector]
    GameObjectDataSet gameObjectData;

    [HideInInspector]
    Material BaseMaterial;
    
    [HideInInspector]
    public System.Action<float> BroadcastProgressPercentage;
    int gameobjectindex;
    int totalgameobjects;


    System.DateTime time;

    GameObject parentobject;
    Dictionary<string, Material> createdMaterials = new Dictionary<string, Material>();
    Vector3List vertices = new Vector3List();
    Vector3List normals = new Vector3List();
    intList indices = new intList();

    bool gameObjectCreated = false;
     //GameObject createdGameObject;
    bool meshcreated = false;
    Mesh createdMesh;

    System.Action<GameObject> callback;
    void BroadCastProgress()
    {
        float progress = 100 * gameobjectindex / totalgameobjects;
        if (BroadcastProgressPercentage!=null) BroadcastProgressPercentage(progress);
    }

    public void Create(GameObjectDataSet gameobjectDataset,Material basematerial, System.Action<GameObject> callbackToFunction = null)
    {
        gameObjectData = gameobjectDataset;
        BaseMaterial = basematerial;
        callback = callbackToFunction;
        time = System.DateTime.UtcNow;
        parentobject = new GameObject();
        parentobject.name = gameObjectData.name;
        totalgameobjects = gameObjectData.gameObjects.Count;
        gameobjectindex = 0;
        StartCoroutine(createGameObjects());
    }

    IEnumerator createGameObjects()
    {
        if (gameObjectData.gameObjects.Count==1)
        {
            yield return StartCoroutine(AddGameObject(gameObjectData.gameObjects[0],parentobject));
            parentobject.name = gameObjectData.name;
        }
        else 
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
        }
        if (callback !=null)
        {
            callback(parentobject);
        }
        parentobject = null;
    }


    IEnumerator AddGameObject(GameObjectData gameobjectdata, GameObject GameObject = null)
    {
        GameObject gameobject;
        if (GameObject != null)
        {
            gameobject = GameObject;
        }
        else
        {
            gameobject = new GameObject();
        }
        gameobject.name = gameobjectdata.name;
        gameobject.transform.parent = parentobject.transform;
        
        yield return StartCoroutine(CreateMesh(gameobjectdata.meshdata));

        if (createdMesh is null)
        {   gameObjectCreated = true;
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
        mr.materials = materiallist.ToArray();
        gameObjectCreated = true;
    }

    Material getMaterial(string materialname)
    {
        Material returnmaterial;
        //if (createdMaterials.ContainsKey(materialname))
        //{
        //    returnmaterial = createdMaterials[materialname];
        //}
        //else
        //{
            returnmaterial = new Material(BaseMaterial);
            returnmaterial.name = materialname;
            for (int i = 0; i < gameObjectData.materials.Count; i++)
            {
                if (gameObjectData.materials[i].Name==materialname)
                {
                    returnmaterial.name = gameObjectData.materials[i].DisplayName;
                    returnmaterial.color = gameObjectData.materials[i].Diffuse;
                    //createdMaterials.Add(materialname, returnmaterial);
                }
            }
        //}
        return returnmaterial;
    }


    IEnumerator CreateMesh(MeshData meshdata)
    {
        bool hasnormals = false;
        createdMesh = new Mesh();
        createdMesh.Clear();
        // add vertices
        
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
            if ((System.DateTime.UtcNow-time).TotalMilliseconds>400)
            {
                yield return null;
                time = System.DateTime.UtcNow;
            }
            meshvector3[i] = vertices.ReadItem(i);
        }
        createdMesh.vertices = meshvector3;
        //createdMesh.SetVertices(meshvector3);
        
        vertices.EndReading();
        vertices.RemoveData();

        // add indices
        
        indices.SetupReading(meshdata.indicesFileName);
        int indexcount = indices.numberOfVertices();
        int[] meshindices = new int[indexcount];
        for (int i = 0; i < indexcount; i++)
        {
            if ((System.DateTime.UtcNow - time).TotalMilliseconds > 400)
            {
                yield return null;
                time = System.DateTime.UtcNow;
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
           
            
            normals.SetupReading(meshdata.normalsFileName);
            int normalscount = normals.Count();
            if (normalscount==vertexcount)
            {
                if ((System.DateTime.UtcNow - time).TotalMilliseconds > 400)
                {
                    yield return null;
                    time = System.DateTime.UtcNow;
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
        if ((System.DateTime.UtcNow - time).TotalMilliseconds > 400)
        {
            yield return null;
            time = System.DateTime.UtcNow;
        }
        createdMesh.RecalculateBounds();
        meshcreated = true;
    }
}
