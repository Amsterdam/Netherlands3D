using Netherlands3D.Core;
using Netherlands3D.Core.Tiles;
using subtree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Netherlands3D
{
    public class GenerateImageDataset : MonoBehaviour
    {
        public string planeBase64 = "AACAvwAAAAAAAIA/AACAPwAAAAAAAIA/AACAvwAAAAAAAIC/AACAPwAAAAAAAIC/AAAAAAAAgD8AAACAAAAAAAAAgD8AAACAAAAAAAAAgD8AAACAAAAAAAAAgD8AAACAAAAAAAAAgD///38/AACAPwAAAAAAAIAz//9/PwAAgDMAAAEAAwAAAAMAAgA=";
        public string planeGltfTemplate = "{\"asset\":{\"generator\":\"Netherlands3D\",\"version\":\"2.0\"},\"scene\":0,\"scenes\":[{\"name\":\"Scene\",\"nodes\":[0]}],\"nodes\":[{\"mesh\":0,\"name\":\"0\",\"translation\":[1,0,-1]}],\"materials\":[{\"doubleSided\":true,\"name\":\"0\",\"pbrMetallicRoughness\":{\"baseColorTexture\":{\"index\":0},\"metallicFactor\":0,\"roughnessFactor\":0.5}}],\"meshes\":[{\"name\":\"Plane\",\"primitives\":[{\"attributes\":{\"POSITION\":0,\"NORMAL\":1,\"TEXCOORD_0\":2},\"indices\":3,\"material\":0}]}],\"textures\":[{\"sampler\":0,\"source\":0}],\"images\":[{\"bufferView\":4,\"mimeType\":\"image/png\",\"name\":\"texture\"}],\"accessors\":[{\"bufferView\":0,\"componentType\":5126,\"count\":4,\"max\":[1,0,1],\"min\":[-1,0,-1],\"type\":\"VEC3\"},{\"bufferView\":1,\"componentType\":5126,\"count\":4,\"type\":\"VEC3\"},{\"bufferView\":2,\"componentType\":5126,\"count\":4,\"type\":\"VEC2\"},{\"bufferView\":3,\"componentType\":5123,\"count\":6,\"type\":\"SCALAR\"}],\"bufferViews\":[{\"buffer\":0,\"byteLength\":48,\"byteOffset\":0},{\"buffer\":0,\"byteLength\":48,\"byteOffset\":48},{\"buffer\":0,\"byteLength\":32,\"byteOffset\":96},{\"buffer\":0,\"byteLength\":12,\"byteOffset\":128},{\"buffer\":0,\"byteLength\":<bytesLengthImage>,\"byteOffset\":140}],\"samplers\":[{\"magFilter\":9729,\"minFilter\":9987}],\"buffers\":[{\"byteLength\":<bytesLengthTotal>,\"uri\":\"<base64>\"}]}";
        public string pdokWMSServiceUrl = "https://service.pdok.nl/hwh/luchtfotorgb/wms/v1_0?request=GetMap&service=wms&version=1.3.0&layers=Actueel_ortho25&crs=EPSG:4326&bbox={bbox}&width={width}&height={height}&format=image/png";

        byte[] planeBytes;

        private string nldataset = "/nltileset/tileset.json";

        void Start()
        {
            planeBytes = Convert.FromBase64String(planeBase64);

            var readTileSet = this.gameObject.AddComponent<Read3DTileset>();
            readTileSet.tilesetUrl = "file:///" + Application.streamingAssetsPath + nldataset;

            var tileRoot = this.GetComponent<Read3DTileset>().root;
            StartCoroutine(DownloadImagesForTile(tileRoot));
        }

        private IEnumerator DownloadImagesForTile(Tile tile)
        {
            var wgsMin = new Vector3WGS((tile.boundingVolume.values[0] * 180.0f) / Mathf.PI, (tile.boundingVolume.values[1] * 180.0f) / Mathf.PI, tile.boundingVolume.values[4]);
            var wgsMax = new Vector3WGS((tile.boundingVolume.values[2] * 180.0f) / Mathf.PI, (tile.boundingVolume.values[3] * 180.0f) / Mathf.PI, tile.boundingVolume.values[5]);

            var constructUrl = pdokWMSServiceUrl.Replace("{bbox}", $"{wgsMin.lat},{wgsMin.lon},{wgsMax.lat},{wgsMax.lon}");
            constructUrl = constructUrl.Replace("{width}", "512");
            constructUrl = constructUrl.Replace("{height}", "512");

            Debug.Log($"Download: {constructUrl}");
            UnityWebRequest www = UnityWebRequest.Get(constructUrl);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error downloading image: {www.error}");
                yield break;
            }

            byte[] imageBytes = www.downloadHandler.data;
            yield return CreateGltfForTileWithTexture(imageBytes,tile);

            foreach (var child in tile.children)
            {
                yield return DownloadImagesForTile(child);
            }
        }


        IEnumerator CreateGltfForTileWithTexture(byte[] imageBytes, Tile tile)
        {
            byte[] combinedBytes = planeBytes.Concat(imageBytes).ToArray();
            string base64String = "data:application/octet-stream;base64," + Convert.ToBase64String(combinedBytes);

            // Return the data URI
            var base64encoded = $"{base64String}";

            //Inject into plane gltf and save it as a new tile
            var gltfContent = planeGltfTemplate.Replace("<base64>", base64encoded);
            gltfContent = gltfContent.Replace("<bytesLengthImage>", imageBytes.Length.ToString());
            gltfContent = gltfContent.Replace("<bytesLengthTotal>", combinedBytes.Length.ToString());

            var rootDir = Application.dataPath + "/../tiles";
            var subtreeDir = Application.dataPath + "/../tiles/subtrees";
            var contentDir = Application.dataPath + "/../tiles/content";

            Directory.CreateDirectory(rootDir);
            Directory.CreateDirectory(subtreeDir);
            Directory.CreateDirectory(contentDir);

            var gltfFile = contentDir + $"/{tile.Z}_{tile.X}_{tile.Y}.gltf";

            File.WriteAllText(gltfFile, gltfContent);
            Debug.Log("Written " + gltfFile);
            yield return null;
        }

        [ContextMenu("Write subtree")]
        private void GenerateSubtree()
        {
            var subtree = new Subtree();
            var bytes = SubtreeWriter.ToBytes(subtree);

            var subtreeDir = Application.dataPath + "/../tiles/subtrees";
            Directory.CreateDirectory(subtreeDir);

            File.WriteAllBytes(Application.dataPath + "/../tiles/subtrees/0.0.0.subtree", bytes);
        }
    }
}
