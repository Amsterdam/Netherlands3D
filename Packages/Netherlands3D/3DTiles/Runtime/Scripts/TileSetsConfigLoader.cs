using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Networking;

namespace Netherlands3D.Tiles3D
{
    public class TileSetsConfigLoader : MonoBehaviour
    {
        [Serializable]
        public class Config
        {
            public string viewerVersion = "2.0.0";
            public TileSet[] tilesets = new TileSet[0];
            [Serializable]
            public class TileSet
            {
                public string url = "";
                public int maximumScreenSpaceError = 5;
            }
        }

        [SerializeField,Tooltip("Relative to StreamingAssets")] private string configPath = "/config.json";

        private Config config;
        
        private void Awake()
        {
            StartCoroutine(LoadJsonConfig(Application.streamingAssetsPath + configPath));
        }

        IEnumerator LoadJsonConfig(string configPath)
        {
            UnityWebRequest webRequest = UnityWebRequest.Get((new Uri(configPath)));
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning(webRequest.error);
            }
            else
            {
                string jsonData = webRequest.downloadHandler.text;
                config = JsonUtility.FromJson<Config>(jsonData);
                AddTileSets();
            }
        }

        private void AddTileSets()
        {
            var tilePrioritiser = GetComponent<TilePrioritiser>(); 

            foreach (var tileset in config.tilesets)
            {
                var newTileSet = new GameObject(tileset.url);
                newTileSet.transform.SetParent(this.transform);

                var tileSetReader = newTileSet.AddComponent<Read3DTileset>();
                tileSetReader.Initialize(tileset.url, tileset.maximumScreenSpaceError, tilePrioritiser);
            }
        }

#if UNITY_EDITOR
        private bool configGenerated = false;
        private void OnValidate()
        {
            if(!configGenerated)
                GenerateConfigFile();
        }

        [ContextMenu("Generate new config file in StreamingAssets")]
        public void GenerateConfigFile()
        {
            var path = Application.streamingAssetsPath + configPath;
            if (!File.Exists(path))
            {
                //Make sure StreamingAssets path exists
                Directory.CreateDirectory(Path.GetDirectoryName(Application.streamingAssetsPath + configPath));

                //Generate an example config with 2 tilesets
                var newConfig = new Config();
                var exampleBuildingsTileSet = new Config.TileSet()
                {
                    url = "https://3d.test.kadaster.nl/3dtiles/2020/buildings/tileset.json",
                };
                var exampleTerrainTileSet = new Config.TileSet()
                {
                    url = "https://3d.test.kadaster.nl/3dtiles/2020/terrain/tileset.json",
                };
                Config.TileSet[] tilesets = new Config.TileSet[2]{ 
                    exampleBuildingsTileSet, 
                    exampleTerrainTileSet 
                };

                newConfig.tilesets = tilesets;

                File.WriteAllText(path, JsonUtility.ToJson(newConfig,true));

                configGenerated = true;

                Debug.Log(path + " generated");
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.Log(path + " found.", this.gameObject);
            }
        }
#endif
    }
}