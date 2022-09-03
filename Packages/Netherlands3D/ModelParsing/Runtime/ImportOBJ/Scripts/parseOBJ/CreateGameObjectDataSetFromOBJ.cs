using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;

namespace Netherlands3D.ModelParsing
{
    public class CreateGameObjectDataSetFromOBJ : MonoBehaviour
    {

        const string glyphs = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"; //add the characters you want

        //actions for progress-callback
        public System.Action<float> broadcastProgressPercentage;
        public System.Action<string> broadcastCurrentAction;

        // variables for determining the progress
        int totalSubmeshCount = 0;
        int currentSubmeshindex = 0;
        int maximumFrameDurationInMilliseconds = 400;
        System.DateTime time;

        // variables determining how the gameobjectDataSet is set up
        public bool createSingelMesh = true;
        public bool isRD = false;

        // variables containing the input
        public Vector3List vertices;
        public Vector3List normals;
        public List<Submesh> submeshes;

        // variable for creating the dataset
        Vector3List meshVertices;
        Vector3List meshNormals;
        intList meshIndices;
        MeshData createdMeshData;
        SubMeshData createdSubMeshData;
        GameObjectDataSet container = new GameObjectDataSet();
        int nextindex = 0;

        // variable for string the returnAdress
        System.Action<GameObjectDataSet> callback;

        // variable for cancelling
        bool needToCancel = false;

        public void Cancel()
        {
            needToCancel = true;
            
        }


        public void CreateGameObjectDataSet(System.Action<GameObjectDataSet> callbacktoFunction,bool createMultipleObjects=false)
        {
            needToCancel = false;
            container = new GameObjectDataSet();
            totalSubmeshCount = submeshes.Count;
            currentSubmeshindex = 0;
            callback = callbacktoFunction;

            if (createMultipleObjects)
            {
                StartCoroutine(CreateSingleSubmesh());
            }
            else
            {
                StartCoroutine(CreateMultiSubmesh());
            }
        }

        void Finish()
        {
            if (needToCancel)
            {
                container.Clear();
                container = null;
                foreach (var item in submeshes)
                {

                    item.rawData.RemoveData();
                }
                submeshes.Clear();
            }
            if (broadcastCurrentAction != null)
                broadcastCurrentAction("");
            vertices.RemoveData();
            normals.RemoveData();
            callback(container);
        }

        IEnumerator CreateMultiSubmesh()
        {
            time = System.DateTime.UtcNow;
            GameObjectData childObject = new GameObjectData();
            childObject.name = submeshes[0].name;
             yield return StartCoroutine(CreateMeshData(submeshes,true));
            childObject.meshdata = createdMeshData;
            container.gameObjects.Add(childObject);
            
            Finish();
        }

        IEnumerator CreateSingleSubmesh()
        {
            time = System.DateTime.UtcNow;
            List<Submesh> submesheslist = new List<Submesh>();
            int currentsubmesh = 1;
            int submeshcount = submeshes.Count;
            foreach (var submesh in submeshes)
            {
                if (broadcastCurrentAction != null)
                    broadcastCurrentAction("object-onderdelen samenstellen. " + currentsubmesh++ + " van " + submeshcount);

                submesheslist.Add(submesh);
                GameObjectData childObject = new GameObjectData();
                childObject.name = submesh.name;

                yield return StartCoroutine(CreateMeshData(submesheslist));

                childObject.meshdata = createdMeshData;
                submesheslist.Clear();
                container.gameObjects.Add(childObject);
                if(needToCancel)
                {
                    Finish();
                    yield break;
                }
            }
            
            Finish();
        }

       IEnumerator CreateMeshData(List<Submesh> submeshes, bool broadcast = false)
        {
            meshVertices = new Vector3List();
            string meshvertexname = randomString(20);
            meshNormals = new Vector3List();
            string meshnormalname = randomString(20);
            meshVertices.SetupWriting(meshvertexname);
            meshNormals.SetupWriting(meshnormalname);
            meshIndices = new intList();
            string meshindicesname = randomString(20);
            meshIndices.SetupWriting(meshindicesname);

            createdMeshData = new MeshData();
            createdMeshData.name = submeshes[0].name;
            createdMeshData.vertexFileName = meshvertexname;
            createdMeshData.normalsFileName = meshnormalname;
            createdMeshData.indicesFileName = meshindicesname;
            nextindex = 0;
            int currentsubmesh = 1;
            int submeshcount = submeshes.Count;
            foreach (var submesh in submeshes)
            {
                if (broadcast)
                {
                     if (broadcastCurrentAction != null)
                        broadcastCurrentAction("objecten creeren. " + currentsubmesh++ + " van " + submeshcount);
                }
                yield return StartCoroutine(CreateSubMeshData(submesh));
                createdMeshData.submeshes.Add(createdSubMeshData);
                if(needToCancel)
                {
                    meshVertices.EndWriting();
                    meshVertices.RemoveData();
                    meshNormals.EndWriting();
                    meshNormals.RemoveData();
                    meshIndices.EndWriting();
                    meshIndices.RemoveData();
                    yield break;
                }
            }
            meshVertices.EndWriting();
            meshNormals.EndWriting();
            meshIndices.EndWriting();
        }
        IEnumerator CreateSubMeshData(Submesh submesh)
        {
            currentSubmeshindex++;

            createdSubMeshData = new SubMeshData();
            SubMeshRawData smrd = submesh.rawData;
            createdSubMeshData.materialname = submesh.name;
            smrd.SetupReading();
            vertices.SetupReading();
            normals.SetupReading();
            long indexcount = smrd.numberOfVertices();
            createdSubMeshData.startIndex = nextindex;
            for (int i = 0; i < indexcount; i++)
            {
                if (needToCancel)
                {
                    smrd.EndReading();
                    smrd.RemoveData();
                    vertices.EndReading();
                    normals.EndReading();
                    vertices.RemoveData();
                    normals.RemoveData();
                    yield break;
                }
                if ((System.DateTime.UtcNow-time).TotalMilliseconds > maximumFrameDurationInMilliseconds)
                {

                    float size = 1f / totalSubmeshCount;
                    float progress = size* i / indexcount;
                     if (broadcastProgressPercentage != null)
                        broadcastProgressPercentage(100 * (((currentSubmeshindex - 1) * size) + progress));
                    
                    yield return null;
                    time = System.DateTime.UtcNow;
                }
                Vector3Int data = smrd.ReadNext();
                Vector3 vertex = vertices.ReadItem(data.x);
                Vector3 normal = normals.ReadItem(data.y);
                meshVertices.Add(vertex.x, vertex.y, vertex.z);
                meshNormals.Add(normal.x, normal.y, normal.z);
                meshIndices.Add(nextindex++);
            }
            createdSubMeshData.Indexcount = nextindex - createdSubMeshData.startIndex;
            
            smrd.EndReading();
            smrd.RemoveData();
            vertices.EndReading();
            normals.EndReading();
        }

        string randomString(int length)
        {
            string returnstring = "";
            for (int i = 0; i < length; i++)
            {
                returnstring += glyphs[Random.Range(0, glyphs.Length)];
            }
            return returnstring;
        }

    }
}