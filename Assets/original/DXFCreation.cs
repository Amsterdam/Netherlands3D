using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Core;
using Netherlands3D.TileSystem;
using Netherlands3D.Events;

namespace Netherlands3D.FileExport
{
    public class DXFCreation : ModelFormatCreation
    {
        MeshClipper.RDBoundingBox boundingbox;
        bool boundingboxIsSet = false;
        bool layersAreSet = false;
        [Header("input")]
        [SerializeField] Vector3ListEvent Bounds;
        [SerializeField] StringListEvent Layers;
        [SerializeField] TriggerEvent begin;
        private void OnValidate()
        {
            if (Bounds is null)
            {
                Debug.LogError($"Vector3ListEvent for Bounds is required on {gameObject.name}",gameObject);
            }
        }

        void Start()
        {
            if (begin != null) begin.started.AddListener(startprocessing);
            if (Bounds != null) Bounds.started.AddListener(receiveBounds);
            if (Layers != null) Layers.started.AddListener(receiveLayerSelection);
        }

        void startprocessing()
        {
            if (!boundingboxIsSet)
            {
                if (onError != null) onError.started.Invoke("Boundingbox not set");
                return;
            }
            if (!layersAreSet)
            {
                if (onError != null) onError.started.Invoke("Layers not set");
                return;
            }

        }

        void receiveBounds(List<Vector3> input)
        {
            boundingboxIsSet = true;
            //TODO: create a boundingbox from the input
        }
        void receiveLayerSelection(List<string> input)
        {
            layersAreSet = true;
            //TODO: create a LayerList from the input
        }


        public void CreateDXF(Bounds UnityBounds, List<Layer> layerList)
        {
            StartCoroutine(CreateFile(UnityBounds, layerList));
        }

        private IEnumerator CreateFile(Bounds UnityBounds, List<Layer> layerList)
        {
            FreezeLayers(layerList, true);
            Debug.Log(layerList.Count);
            Vector3RD bottomLeftRD = CoordConvert.UnitytoRD(UnityBounds.min);
            Vector3RD topRightRD = CoordConvert.UnitytoRD(UnityBounds.max);
            boundingbox = new MeshClipper.RDBoundingBox(bottomLeftRD.x, bottomLeftRD.y, topRightRD.x, topRightRD.y);
            DxfFile dxfFile = new DxfFile();
            dxfFile.SetupDXF();
            yield return null;
            MeshClipper meshClipper = new MeshClipper();
            if (ShowHideprogressWindow != null) ShowHideprogressWindow.started.Invoke(true);
            if (progressTitle!=null) progressTitle.started.Invoke("DXF-bestand genereren...");
            if (progressMessage!=null) progressMessage.started.Invoke("");
            if (progressPercentage!=null) progressPercentage.started.Invoke(0.1f);
                        yield return new WaitForEndOfFrame();

            int layercounter = 0;
            foreach (var layer in layerList)
            {
                layercounter++;
                if (progressMessage != null) progressMessage.started.Invoke("Laag '" + layer.name + "' wordt omgezet...");
                if (progressPercentage != null) progressPercentage.started.Invoke((float)layercounter / ((float)layerList.Count + 1));
                yield return new WaitForEndOfFrame();

                List<GameObject> gameObjectsToClip = GetTilesInLayer(layer, bottomLeftRD, topRightRD);
                if (gameObjectsToClip.Count == 0)
                {
                    AddChildMeshesToClippableObjects(layer, gameObjectsToClip);
                }
                if (gameObjectsToClip.Count == 0)
                {
                    continue;
                }
                foreach (var gameObject in gameObjectsToClip)
                {
                    meshClipper.SetGameObject(gameObject);
                    for (int submeshID = 0; submeshID < gameObject.GetComponent<MeshFilter>().sharedMesh.subMeshCount; submeshID++)
                    {
                        string layerName = gameObject.GetComponent<MeshRenderer>().sharedMaterials[submeshID].name.Replace(" (Instance)", "");
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

                        if (progressMessage != null) progressMessage.started.Invoke("Laag '" + layer.name + "' object " + layerName + " wordt uitgesneden...");
                        yield return new WaitForEndOfFrame();

                        meshClipper.ClipSubMesh(boundingbox, submeshID);
                        dxfFile.AddLayer(meshClipper.clippedVerticesRD, layerName, GetColor(gameObject.GetComponent<MeshRenderer>().sharedMaterials[submeshID]));
                        yield return new WaitForEndOfFrame();
                    }
                    yield return new WaitForEndOfFrame();
                }
                yield return new WaitForEndOfFrame();
            }
            if (progressMessage != null) progressMessage.started.Invoke("Het AutoCAD DXF (.dxf) bestand wordt afgerond...");
            if (progressPercentage != null) progressPercentage.started.Invoke((float)layerList.Count / ((float)layerList.Count + 1));
            yield return new WaitForEndOfFrame();
            dxfFile.Save();
            
            FreezeLayers(layerList, false);
            if (filename != null) filename.started.Invoke(dxfFile.GetFullPath());
            if (ShowHideprogressWindow != null) ShowHideprogressWindow.started.Invoke(false);
        }

        private void AddChildMeshesToClippableObjects(Layer layer, List<GameObject> gameObjectsToClip)
        {
            if (layer.transform.childCount > 0)
            {
                MeshFilter[] meshFilterChildren = layer.GetComponentsInChildren<MeshFilter>(true);
                for (int i = 0; i < meshFilterChildren.Length; i++)
                {
                    gameObjectsToClip.Add(meshFilterChildren[i].gameObject);
                }
            }
        }

        private netDxf.AciColor GetColor(Material material)
        {
            if (material.GetColor("_BaseColor") != null)
            {
                byte r = (byte)(material.GetColor("_BaseColor").r * 255);
                byte g = (byte)(material.GetColor("_BaseColor").g * 255);
                byte b = (byte)(material.GetColor("_BaseColor").b * 255);
                return new netDxf.AciColor(r, g, b);
            }
            else if (material.GetColor("_FresnelColorHigh") != null)

            {
                byte r = (byte)(material.GetColor("_FresnelColorHigh").r * 255);
                byte g = (byte)(material.GetColor("_FresnelColorHigh").g * 255);
                byte b = (byte)(material.GetColor("_FresnelColorHigh").b * 255);
                return new netDxf.AciColor(r, g, b);
            }
            else
            {
                return netDxf.AciColor.LightGray;
            }
        }
    }
}