using System.Collections.Generic;
using Netherlands3D.Coordinates;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Netherlands3D.Minimap
{
	/// <summary>
	/// Handles the functionality of the minimap
	/// </summary>
	[HelpURL("https://portal.opengeospatial.org/files/?artifact_id=35326")]
	public class WMTSMap : MonoBehaviour
	{
		/// <summary>
		/// Center the pointer of the minimap in the view
		/// </summary>
		public bool CenterPointerInView { get => centerPointerInView; set => centerPointerInView = value; }

		[Header("Values")]
		[Tooltip("Should the pointer be centerd in the view?")]
		[SerializeField] private bool centerPointerInView = true;
		[Tooltip("The start indexLayer of the map")]
		[SerializeField] private int layerStartIndex = 6;
		[Tooltip("Top right coords")]
		[SerializeField] private Vector2RD topRight = new Vector2RD(141000, 501000);
		[Tooltip("Bottom left coords")]
		[SerializeField] private Vector2RD bottomLeft = new Vector2RD(109000, 474000);

		[Header("Components")]
		[Tooltip("The rect tranform of the pointer")]
		[SerializeField] private RectTransform pointer;
		[Tooltip("The rect transform of the fov")]
		[SerializeField] private RectTransform fov;
		[Tooltip("The used configuration of the minimap")]
		[SerializeField] private MinimapConfig minimapConfig;

        [Header("Events")]
        [SerializeField] private UnityEvent<int> onZoom = new();
        [SerializeField] private UnityEvent<Coordinate> onClick = new();

		/// <summary>
		/// The current index layer of tile layers
		/// </summary>
		private int layerIndex = 5;
		/// <summary>
		/// The size of 1 tile
		/// </summary>
		private float tileSize = 256;
		/// <summary>
		/// The base size of 1 tile
		/// </summary>
		private float baseTileSize;
		/// <summary>
		/// The start meter in pixels
		/// </summary>
		private float startMeterInPixels = 0;
		/// <summary>
		/// The tile size in meters
		/// </summary>
		private double tileSizeInMeters = 0;
		/// <summary>
		/// Used for calculating the minimap scale
		/// </summary>
		private double divide = 0;
		/// <summary>
		/// 1 pixel in meters
		/// </summary>
		private double pixelInMeters = 0.00028;
		/// <summary>
		/// The minimap scale denominator, Zero zoomlevel is 1:12288000
		/// </summary>
		private double scaleDenominator = 12288000;
		/// <summary>
		/// The map size in meters
		/// </summary>
		private double mapSizeInMeters = 0;
		/// <summary>
		/// The bounds of tiles in meters
		/// </summary>
		private Vector2 boundsInMeters;
		/// <summary>
		/// The bounds of the tiles
		/// </summary>
		private Vector2 boundsTiles;
		/// <summary>
		/// The offset of a tile
		/// </summary>
		private Vector2 tileOffset;
		/// <summary>
		/// The layer tiles offset
		/// </summary>
		private Vector2 layerTilesOffset;
		/// <summary>
		/// The minimap top left x/y in meters(?)
		/// </summary>
		private Vector2RD minimapTopLeft = new Vector2RD(-285401.92, 903401.92);
		/// <summary>
		/// The minimap UI that handles all the UI
		/// </summary>
		private MinimapUI minimapUI;
		/// <summary>
		/// The rect transform of the minimap
		/// </summary>
		private RectTransform rectTransformMinimapUI;
		/// <summary>
		/// The rect transform
		/// </summary>
		private RectTransform rectTransform;
		/// <summary>
		/// Contains the minimap layers with tiles
		/// </summary>
		private Dictionary<int, Dictionary<Vector2, Tile>> tileLayers = new Dictionary<int, Dictionary<Vector2, Tile>>();

        private void Awake()
        {
			minimapUI = GetComponentInParent<MinimapUI>();
			rectTransform = GetComponent<RectTransform>();
			rectTransformMinimapUI = (RectTransform)minimapUI.transform;
        }

        private void Start()
		{
			layerIndex = layerStartIndex;

			// Use config values
			tileSize = minimapConfig.TileMatrixSet.TileSize;
			pixelInMeters = minimapConfig.TileMatrixSet.PixelInMeters;
			scaleDenominator = minimapConfig.TileMatrixSet.ScaleDenominator;

			// Coverage of our application bounds
			boundsInMeters.x = (float)topRight.x - (float)bottomLeft.x;
			boundsInMeters.y = (float)topRight.y - (float)bottomLeft.y;

			baseTileSize = tileSize;

			// Calculate map width in meters based on zoomlevel 0 setting values
			mapSizeInMeters = baseTileSize * pixelInMeters * scaleDenominator;

			DetermineTopLeftOrigin();
			CalculateGridScaling();
			ActivateMapLayer();

			// Calculate base meters in pixels to do calculations converting local coordinates to meters
			startMeterInPixels = (float)tileSizeInMeters / (float)baseTileSize;

			pointer.gameObject.SetActive(true);
		}

		private void Update()
		{
			Clamp();

			//Continiously check if tiles of the active layer identifier should be loaded
			UpdateLayerTiles(tileLayers[layerIndex]);
			MovePointer();
		}

		/// <summary>
		/// Clamp the minimap
		/// </summary>
		public void Clamp()
		{
			var maxPositionXInUnits = -(boundsInMeters.x / startMeterInPixels) * transform.localScale.x;
			var maxPositionYInUnits = (boundsInMeters.y / startMeterInPixels) * transform.localScale.x;

			this.transform.localPosition = new Vector3(
				Mathf.Clamp(this.transform.localPosition.x, maxPositionXInUnits + rectTransformMinimapUI.sizeDelta.x, 0),
				Mathf.Clamp(this.transform.localPosition.y, rectTransformMinimapUI.sizeDelta.y, maxPositionYInUnits),
				0
			);
		}

		/// <summary>
		/// Called from minimap UI when the user clicked on the map
		/// </summary>
		/// <param name="eventData"></param>
		public void ClickedMap(PointerEventData eventData)
		{
			//The point we clicked on the map in local coordinates
			Vector3 localClickPosition = transform.InverseTransformPoint(eventData.position);

			//Distance in meters from top left corner of this map
			var meterX = localClickPosition.x * startMeterInPixels;
			var meterY = localClickPosition.y * startMeterInPixels;

            var rdCoordinate = new Coordinate(
                CoordinateSystem.RD,
                bottomLeft.x + meterX,
                (float)topRight.y + meterY,
                0.0d
            );
			var unityCoordinate = CoordinateConverter.ConvertTo(rdCoordinate, CoordinateSystem.Unity).ToVector3();
            unityCoordinate.y = Camera.main.transform.position.y;

            onClick.Invoke(rdCoordinate);
			print(unityCoordinate);

			Camera.main.transform.position = unityCoordinate;
		}

		/// <summary>
		/// Return the local unity map coordinates
		/// </summary>
		/// <param name="sourceRDPosition">The source RD position</param>
		/// <returns></returns>
		public Vector3 DeterminePositionOnMap(Vector3RD sourceRDPosition)
		{
			var meterX = sourceRDPosition.x - (float)bottomLeft.x;
			var meterY = sourceRDPosition.y - (float)topRight.y;

			var pixelX = meterX / startMeterInPixels;
			var pixelY = meterY / startMeterInPixels;

			return new Vector3((float)pixelX, (float)pixelY);
		}

		/// <summary>
		/// Position a RectTransform object on the map using RD coordinates
		/// Handy if you want to place markers/location indicators on the minimap.
		/// </summary>
		/// <param name="targetObject">RectTransform object to be placed</param>
		/// <param name="targetPosition">RD coordinate to place the object</param>
		public void PositionObjectOnMap(RectTransform targetObject, Vector3RD targetPosition)
		{
			targetObject.transform.localScale = Vector3.one / rectTransform.localScale.x;
			targetObject.transform.localPosition = DeterminePositionOnMap(targetPosition);
		}


		/// <summary>
		/// The zoomlevel of the viewer. Not to be confused with the map identifier.
		/// The viewer starts at zoom level 0, our map identifier can start at a different identifier.
		/// </summary>
		/// <param name="viewerZoom">The viewer zoomlevel</param>
		public void Zoomed(int viewerZoom)
		{
			tileSize = baseTileSize / Mathf.Pow(2, viewerZoom);

			layerIndex = layerStartIndex + viewerZoom;
			CalculateGridScaling();
			ActivateMapLayer();
            onZoom.Invoke(viewerZoom);
		}

		/// <summary>
		/// Setup the minimap layers
		/// </summary>
		private void ActivateMapLayer()
		{
			RemoveOtherLayers();

			Dictionary<Vector2, Tile> tileList;
			if(!tileLayers.ContainsKey(layerIndex))
			{
				tileList = new Dictionary<Vector2, Tile>();
				tileLayers.Add(layerIndex, tileList);
			}
		}

		/// <summary>
		/// Determine the minimap top left origin
		/// </summary>
		private void DetermineTopLeftOrigin()
		{
			switch(minimapConfig.TileMatrixSet.minimapOriginAlignment)
			{
				case TileMatrixSet.OriginAlignment.BottomLeft:
					minimapTopLeft.x = minimapConfig.TileMatrixSet.Origin.x;
					minimapTopLeft.y = minimapConfig.TileMatrixSet.Origin.y + mapSizeInMeters;
					break;
				default:
					minimapTopLeft.x = minimapConfig.TileMatrixSet.Origin.x;
					minimapTopLeft.y = minimapConfig.TileMatrixSet.Origin.y;
					break;
			}
		}

		/// <summary>
		/// Calculate the minimap grid scaling
		/// </summary>
		private void CalculateGridScaling()
		{
			divide = Mathf.Pow(2, layerIndex);
			tileSizeInMeters = mapSizeInMeters / divide;

			//The tile 0,0 its top left does not align with our region top left. So here we determine the offset.
			layerTilesOffset = new Vector2(
				((float)bottomLeft.x - (float)minimapTopLeft.x) / (float)tileSizeInMeters,
				((float)minimapTopLeft.y - (float)topRight.y) / (float)tileSizeInMeters
			);

			//Based on tile numbering type
			tileOffset.x = Mathf.Floor(layerTilesOffset.x);
			tileOffset.y = Mathf.Floor(layerTilesOffset.y);

			//Store the remaining value to offset layer
			layerTilesOffset.x -= tileOffset.x;
			layerTilesOffset.y -= tileOffset.y;

			//Calculate the amount of tiles needed for our app bounding box
			boundsTiles.x = Mathf.CeilToInt(boundsInMeters.x / (float)tileSizeInMeters);
			boundsTiles.y = Mathf.CeilToInt(boundsInMeters.y / (float)tileSizeInMeters);
		}

		/// <summary>
		/// Move the pointer
		/// </summary>
		private void MovePointer()
		{
			fov.SetAsLastSibling(); //Fov is on top of map
			pointer.SetAsLastSibling(); //Pointer is on top of fov

			PositionObjectOnMap(pointer, CoordinateConverter.UnitytoRD(Camera.main.transform.position));

			if(CenterPointerInView)
			{
				this.transform.localPosition = -pointer.localPosition * rectTransform.localScale.x + (Vector3)rectTransformMinimapUI.sizeDelta * 0.5f;
			}
		}

		/// <summary>
		/// Remove other layers that arent on layerIndex
		/// </summary>
		private void RemoveOtherLayers()
		{
			List<int> mapTileKeys = new List<int>(tileLayers.Keys);
			foreach(int layerKey in mapTileKeys)
			{
				//Remove all layers behind top layer except the first, and the one right below our layer
				if((layerKey < layerIndex - 1 && layerKey != layerStartIndex) || layerKey > layerIndex)
				{
					foreach(var tile in tileLayers[layerKey])
					{
						Destroy(tile.Value.gameObject);
					}
					tileLayers.Remove(layerKey);
				}
			}
		}

		/// <summary>
		/// Update the layer tiles based on current layerIndex
		/// </summary>
		/// <param name="tileList"></param>
		private void UpdateLayerTiles(Dictionary<Vector2, Tile> tileList)
		{
			for(int x = 0; x <= boundsTiles.x; x++)
			{
				for(int y = 0; y <= boundsTiles.y; y++)
				{
					Vector2 tileKey;

					//Tile position within this container
					float xPosition = (x * tileSize) - (layerTilesOffset.x * tileSize);
					float yPosition = -((y * tileSize) - (layerTilesOffset.y * tileSize));

					//Origin alignment determines the way we count our grid
					switch(minimapConfig.TileMatrixSet.minimapOriginAlignment)
					{
						case TileMatrixSet.OriginAlignment.BottomLeft:
							tileKey = new Vector2(x + tileOffset.x, (float)(divide - 1) - (y + tileOffset.y));
							break;
						case TileMatrixSet.OriginAlignment.TopLeft:
						default:
							tileKey = new Vector2(x + tileOffset.x, y + tileOffset.y);
							break;
					}

					//Tile position to check if they are in viewer
					float compareXPosition = xPosition * rectTransform.localScale.x + rectTransform.transform.localPosition.x;
					float compareYPosition = yPosition * rectTransform.localScale.x + rectTransform.transform.localPosition.y;

					//Is this tile within the viewer rectangle?
					bool xWithinView = (compareXPosition + baseTileSize > 0 && compareXPosition < rectTransformMinimapUI.sizeDelta.x);
					bool yWithinView = (compareYPosition > 0 && compareYPosition - baseTileSize < rectTransformMinimapUI.sizeDelta.y);

					if(xWithinView && yWithinView)
					{
						if(!tileList.ContainsKey(tileKey))
						{
							var newTileObject = new GameObject();
							var mapTile = newTileObject.AddComponent<Tile>();
							mapTile.Initialize(this.transform, layerIndex, tileSize, xPosition, yPosition, tileKey, minimapConfig);

							tileList.Add(tileKey, mapTile);
						}
					}
					else if(tileList.ContainsKey(tileKey))
					{
						Destroy(tileList[tileKey].gameObject);
						tileList.Remove(tileKey);
					}
				}
			}
		}
	}
}
