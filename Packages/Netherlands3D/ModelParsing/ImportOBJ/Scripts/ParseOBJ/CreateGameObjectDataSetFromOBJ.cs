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
        string objfilename;

        // variables containing the input
        public Vector3List vertices = new Vector3List();
        public Vector3List normals = new Vector3List();
        public Vector2List uvs = new Vector2List();
        public List<Submesh> submeshes;

        // variable for creating the dataset
        Vector3List meshVertices = new Vector3List();
        Vector3List meshNormals = new Vector3List();
        Vector2List meshUVs = new Vector2List();

        intList meshIndices = new intList();
        MeshData createdMeshData;
        SubMeshData createdSubMeshData;
        GameObjectDataSet container = new GameObjectDataSet();
        SubMeshRawData rawdata = new SubMeshRawData();
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

                    rawdata.RemoveData(item.filename);
                }
                submeshes.Clear();
            }
            if (broadcastCurrentAction != null)
                broadcastCurrentAction("");
            vertices.RemoveData();
            normals.RemoveData();
            uvs.RemoveData();
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
                childObject.name = submesh.displayName;

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

            string meshvertexname = randomString(20);
            string meshnormalname = randomString(20);
            
            meshVertices.SetupWriting(meshvertexname);
            meshNormals.SetupWriting(meshnormalname);

            string meshindicesname = randomString(20);
            meshIndices.SetupWriting(meshindicesname);

            string meshuvname = randomString(20);
            meshUVs.SetupWriting(meshuvname);

            createdMeshData = new MeshData();
            createdMeshData.name = submeshes[0].name;
            createdMeshData.vertexFileName = meshvertexname;
            createdMeshData.normalsFileName = meshnormalname;
            createdMeshData.indicesFileName = meshindicesname;
            createdMeshData.uvFileName = meshuvname;
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

                    meshUVs.EndWriting();
                    meshUVs.RemoveData();


                    yield break;
                }
            }
            meshVertices.EndWriting();
            meshNormals.EndWriting();
            meshIndices.EndWriting();

            meshUVs.EndWriting();            
        }
        IEnumerator CreateSubMeshData(Submesh submesh)
        {
            currentSubmeshindex++;

            createdSubMeshData = new SubMeshData();
            
            createdSubMeshData.materialname = submesh.name;
            rawdata.SetupReading(submesh.filename);
            vertices.SetupReading();
            normals.SetupReading();
            uvs.SetupReading();
            long indexcount = rawdata.numberOfVertices();
            createdSubMeshData.startIndex = nextindex;
            for (int i = 0; i < indexcount; i++)
            {
                if (needToCancel)
                {
                    rawdata.EndReading();
                    rawdata.RemoveData(submesh.filename);
                    vertices.EndReading();
                    normals.EndReading();
                    vertices.RemoveData();
                    normals.RemoveData();

                    uvs.EndReading();
                    uvs.RemoveData();

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
                Vector3Int data = rawdata.ReadNext();
                Vector3 vertex = vertices.ReadItem(data.x);
                meshVertices.Add(vertex.x, vertex.y, vertex.z);

                Vector2 uv = uvs.ReadItem(data.x);
                meshUVs.Add(uv.x, uv.y);                

                if (normals.Count() > 0)
                {
                    Vector3 normal = normals.ReadItem(data.y);
                    meshNormals.Add(normal.x, normal.y, normal.z);
                }

                meshIndices.Add(nextindex++);
            }
            createdSubMeshData.Indexcount = nextindex - createdSubMeshData.startIndex;
            
            rawdata.EndReading();
            rawdata.RemoveData(submesh.filename);
            vertices.EndReading();
            normals.EndReading();

            uvs.EndReading();            
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