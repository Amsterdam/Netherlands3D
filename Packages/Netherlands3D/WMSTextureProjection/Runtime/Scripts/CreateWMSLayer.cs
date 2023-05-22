/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/Netherlands3D/blob/main/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/

using UnityEngine;
using UnityEngine.Events;
using Netherlands3D.TileSystem;
using Netherlands3D.Rendering;

namespace Netherlands3D.WMS
{
    public partial class CreateWMSLayer : MonoBehaviour
    {
        [HideInInspector] public WMSImageLayer layer;

        [SerializeField] public TextureProjectorBase projectorPrefab;
        [SerializeField] private int tileSize = 1500;
        [SerializeField] private bool compressLoadedTextures = true;
        private bool DisplayState = true;

        [System.Serializable]
        public class WMSLOD
        {
            public int textureSize;
            public float maximumDistance;
        }

        [Header("WMS texture sizes")]
        [SerializeField]
        private WMSLOD[] wmsLods = new WMSLOD[3]
        {
            new WMSLOD() { textureSize = 16, maximumDistance = 6000 },
            new WMSLOD() { textureSize = 256, maximumDistance = 3000 },
            new WMSLOD() { textureSize = 2048, maximumDistance = 1000 }
        };

       
        [Header("Optional")]
        [Tooltip("If empty, the first TileHandler found will be used.")]
        [SerializeField] private TileHandler tileHandler;
        public TileHandler TileHandler { get => tileHandler; set => tileHandler = value; }
        public UnityEvent<LogType, string> onLogMessage = new();

        void Start()
        {
            if (!TileHandler)
            {
                TileHandler = FindObjectOfType<TileHandler>();
                if (!TileHandler)
                    onLogMessage.Invoke(LogType.Warning, "No TileHandler found. This script depends on a TileHandler.");
            }
        }

        /// <summary>
        /// turn the layer on or off
        /// </summary>
        /// <param name="OnOff"></param>
        public void ShowLayer(bool OnOff)
        {
            DisplayState = OnOff;
            onLogMessage.Invoke(LogType.Log, $"Show layer: {OnOff}");

            if (layer)
            {
                layer.isEnabled = OnOff;
                layer.gameObject.SetActive(OnOff);
            }
        }

        /// <summary>
        /// Clears layer from tilehandler
        /// </summary>
        public void UnloadLayer()
        {
            onLogMessage.Invoke(LogType.Log, "Removing WMS layer");
            TileHandler.RemoveLayer(layer);
            Destroy(layer.gameObject);
            layer = null;
        }

        /// <summary>
        /// Create a new layer using a WMS base url
        /// </summary>
        public void CreateLayer(string baseURL)
        {
            if (layer != null)
            {

                UnloadLayer();
            }

            onLogMessage.Invoke(LogType.Log, "Creating WMS layer");
            GameObject layerContainer = null;

            if (layer != null)
            {
                TileHandler.RemoveLayer(layer);
                layerContainer = layer.gameObject;
                layer = null;
            }

            if (layerContainer == null)
            {
                layerContainer = new GameObject("WMSLayer");
                layerContainer.layer = this.gameObject.layer;
                layerContainer.transform.parent = transform;
            }
            layer = layerContainer.AddComponent<WMSImageLayer>();
            layer.compressLoadedTextures = compressLoadedTextures;
            layer.tileSize = tileSize;

            AddWMSLayerDataSets(baseURL);

            TileHandler.AddLayer(layer);

            ShowLayer(DisplayState);
        }

        private void AddWMSLayerDataSets(string baseURL)
        {
            for (int i = 0; i < wmsLods.Length; i++)
            {
                var wmsLOD = wmsLods[i];
                DataSet dataSet = new DataSet();

                var wmsUrlTemplate = new WMSImageUrlTemplate(baseURL, wmsLOD.textureSize, wmsLOD.textureSize);
                string datasetURL = wmsUrlTemplate.Url;

                dataSet.path = datasetURL;
                dataSet.maximumDistance = wmsLOD.maximumDistance;

                layer.name = datasetURL;
                layer.ProjectorPrefab = projectorPrefab;
                layer.Datasets.Add(dataSet);
            }
        }
    }
}
