
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Networking;

namespace Netherlands3D.Core.Tiles
{
    public class ConfigLoader : MonoBehaviour
    {
        [Serializable]
        public class Config
        {
            public string viewerVersion = "0.0.1";
            public TileSet[] tilesets = new TileSet[0];
            [Serializable]
            public class TileSet
            {
                public string url = "";
                public int maximumScreenSpaceError = 5;
            }
        }

        [SerializeField,Tooltip("Relative to StreamingAssets")] private string configPath = "/config.json";

        [Header("Optional variables")]
        [SerializeField] GameObject[] enableAfterLoading;
        [SerializeField] GameObject[] disableAfterLoading;

        private Config config;
        private const string examplePath = "https://...";
        
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
                Debug.Log(jsonData );

                StartObjects();
                AddTileSets();
            }
        }

        private void AddTileSets()
        {
            var tilePrioritiser = GetComponent<TilePrioritiser>(); 

            foreach (var tileset in config.tilesets)
            {
                if (tileset.url == examplePath) continue;

                var newTileSet = new GameObject(tileset.url);
                newTileSet.transform.SetParent(this.transform);
                var tileSetReader = newTileSet.AddComponent<Read3DTileset>();
                tileSetReader.SetTilePrioritiser(tilePrioritiser);
                tileSetReader.tilesetUrl = tileset.url;
                tileSetReader.maximumScreenSpaceError = tileset.maximumScreenSpaceError;
            }
        }

        /// <summary>
        /// Optional to activate child objects to force order of operations
        /// </summary>
        private void StartObjects()
        {
            foreach (var enableTarget in enableAfterLoading)
            {
                Debug.Log(enableTarget.name + " enabled", enableTarget);
                enableTarget.SetActive(true);
            }

            foreach (var disable in disableAfterLoading)
            {
                Debug.Log(disable.name + " disabled", disable);
                disable.SetActive(false);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Generate new config file in StreamingAssets")]
        public void GenerateConfigFile()
        {
            var path = Application.streamingAssetsPath + configPath;
            if (!File.Exists(path))
            {
                //Generate an example config with 3 tilesets
                var newConfig = new Config();
                var exampleTileSet = new Config.TileSet()
                {
                    url = examplePath,
                };
                Config.TileSet[] tilesets = new Config.TileSet[3]{ exampleTileSet,exampleTileSet,exampleTileSet  };

                newConfig.tilesets = tilesets;

                File.WriteAllText(path, JsonUtility.ToJson(newConfig,true));

                Debug.Log(path + " generated");
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.Log(path + " already exists");
            }
        }
#endif
    }
}