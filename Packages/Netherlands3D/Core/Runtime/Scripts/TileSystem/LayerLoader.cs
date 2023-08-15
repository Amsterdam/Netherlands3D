using Netherlands3D.Core;
using Netherlands3D.Events;
using Netherlands3D.TileSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Netherlands3D.Coordinates;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Netherlands3D.TileSystem
{
	[HelpURL("https://3d.amsterdam.nl/netherlands3d/help#LayersLoader")]
	public class LayerLoader : MonoBehaviour
	{
		public string streamingAssetsConfigFile = "LayerConfig.json";
		private string configPath = "";

		[SerializeField]
		private TileHandlerConfig configuration;
		private TileHandler tileHandler;

		[Header("Visuals")]
		[SerializeField]
		private Material[] materialSlots;
		[SerializeField]
		private GameObject textIntersectPrefab;
		[SerializeField]
		private GameObject textOverlayPrefab;
		[SerializeField]
		private Material lineShader;
		void Start()
		{
			configPath = Application.streamingAssetsPath + "/" + streamingAssetsConfigFile;
			configuration.dataChanged.AddListener(TileLayerSettingsUpdate);

			if (streamingAssetsConfigFile != "")
			{
				StartCoroutine(LoadExternalConfig());
			}
			else{
				configuration.dataChanged.Invoke();
			}
		}

		private void TileLayerSettingsUpdate()
		{
			tileHandler = gameObject.AddComponent<TileHandler>();

			CoordinateConverter.relativeCenterRD = new Vector2RD(configuration.rdCenterX, configuration.rdCenterY);
			CoordinateConverter.zeroGroundLevelY = configuration.groundYZero;

			foreach (var binaryMeshLayer in configuration.binaryMeshLayers)
			{
				var newLayer = new GameObject().AddComponent<BinaryMeshLayer>();
				newLayer.transform.SetParent(this.transform);
				newLayer.layerPriority = binaryMeshLayer.priority;
				newLayer.name = binaryMeshLayer.layerName;
				newLayer.enabled = binaryMeshLayer.visible;
				newLayer.brotliCompressedExtention = binaryMeshLayer.brotliCompressedExtention;
				/*if (binaryMeshLayer.selectableSubobjects)
				{
					newLayer.gameObject.AddComponent<SelectSubObjects>();
				}*/

				foreach (var materialIndex in binaryMeshLayer.materialLibraryIndices)
				{
					newLayer.DefaultMaterialList.Add(materialSlots[materialIndex]);
				}

				for (int i = 0; i < binaryMeshLayer.lods.Length; i++)
				{
					var lod = binaryMeshLayer.lods[i];
					newLayer.Datasets.Add(
						new DataSet()
						{
							maximumDistance = lod.drawDistance,
							path = lod.sourcePath
						}
					);
				}
				tileHandler.layers.Add(newLayer);
			}

			foreach (var geoJsonLayer in configuration.geoJsonLayers)
			{
				var newLayer = new GameObject().AddComponent<GeoJSONTextLayer>();
				newLayer.transform.SetParent(this.transform);
				newLayer.name = geoJsonLayer.layerName;
				newLayer.textPrefab = (geoJsonLayer.overlay) ? textOverlayPrefab : textIntersectPrefab;
				newLayer.lineRenderMaterial = lineShader;
				newLayer.enabled = geoJsonLayer.visible;

				ColorUtility.TryParseHtmlString(geoJsonLayer.lineColor, out Color lineColor);
				newLayer.lineColor = lineColor;
				newLayer.layerPriority = geoJsonLayer.priority;
				newLayer.geoJsonUrl = geoJsonLayer.sourcePath;
				newLayer.drawGeometry = geoJsonLayer.drawOutlines;
				newLayer.filterUniqueNames = geoJsonLayer.filterUniqueNames;
				if (geoJsonLayer.angleProperty != "")
				{
					newLayer.readAngleFromProperty = true;
					newLayer.angleProperty = geoJsonLayer.angleProperty;
				}
				newLayer.SetAutoOrientationMode(geoJsonLayer.autoOrientationMode);
				newLayer.SetPositionSourceType(geoJsonLayer.positionSourceType);
				newLayer.Datasets.Add(new DataSet() { maximumDistance = geoJsonLayer.drawDistance });
				foreach (var text in geoJsonLayer.texts)
				{
					newLayer.textsAndSizes.Add(new GeoJSONTextLayer.TextsAndSize()
					{
						textPropertyName = text.propertyName,
						drawWithSize = text.size,
						offset = text.offset[1]
					});
				}

				tileHandler.layers.Add(newLayer);
			}
		}

		public void LoadConfig(string jsonConfig)
		{
			JsonUtility.FromJsonOverwrite(jsonConfig, configuration);
			configuration.dataChanged.Invoke();
		}


		IEnumerator LoadExternalConfig()
		{
			Debug.Log($"Loading layers config file: {configPath}");
#if UNITY_WEBGL && !UNITY_EDITOR
			UnityWebRequest webRequest = UnityWebRequest.Get(configPath);

			yield return webRequest.SendWebRequest();
			if (webRequest.result == UnityWebRequest.Result.Success)
			{
				LoadConfig(webRequest.downloadHandler.text);
			}
			else
			{
				Debug.Log($"Could not load {configPath}");
			}
			yield return null;
#else
			if (!File.Exists(configPath))
			{
				Debug.Log($"Could not load {configPath} in StreamingAssets. It will be generated for you.");
				var configJson = JsonUtility.ToJson(configuration, true);

				FileInfo file = new FileInfo(configPath);
				file.Directory.Create();
				File.WriteAllText(file.FullName, configJson);

				yield break;
			}
			LoadConfig(File.ReadAllText(configPath));
			yield return null;
#endif
		}
	}
}
