using Netherlands3D.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netherlands3D.Minimap
{
    [AddComponentMenu("Netherlands3D/Minimap/WMTS")]
    [HelpURL("https://portal.opengeospatial.org/files/?artifact_id=35326")] // Info about WMTS
    public class WMTS : MonoBehaviour
    {
        /// <summary>
        /// Center the pointer in the view
        /// </summary>
        public bool CenterPointerInView
        {
            get => centerPointerInView;
            set
            {
                centerPointerInView = value;
            }
        }
        /// <summary>
        /// The current layer index of the minimap (from which layer images need to be loaded)
        /// </summary>
        public int LayerIndex
        {
            get { return layerIndex; }
            set
            {
                layerIndex = value;
                CalculateGridScaling();
                RemoveNonIndexLayers();
                ActivateLayerIndex();
            }
        }

        [Header("Components")]
        [Tooltip("The configuration of the minimap")]
        [SerializeField] private Configuration config;
        [Tooltip("The rect transform of the FOV")]
        [SerializeField] private RectTransform rectTransformFOV;
        [Tooltip("The rect transform of pointer")]
        [SerializeField] private RectTransform rectTransformPointer;

        /// <summary>
        /// Center the pointer in the view
        /// </summary>
        /// <see cref="CenterPointerInView"/>
        private bool centerPointerInView;
        /// <summary>
        /// The current layer index of the minimap (from which layer images need to be loaded)
        /// </summary>
        private int layerIndex;
        /// <summary>
        /// The start number at which the layerIndex starts;
        /// </summary>
        private int layerStartIndex = 5;

        /// <summary>
        /// 
        /// </summary>
        private float startMeterInPixels;
        /// <summary>
        /// The size of the tiles
        /// </summary>
        private float tileSize;
        /// <summary>
        /// 
        /// </summary>
        private double divide;
        /// <summary>
        /// The map size in meters
        /// </summary>
        private double mapSizeMeters;
        /// <summary>
        /// The tile size in meters
        /// </summary>
        private double tileSizeMeters;
        /// <summary>
        /// The layers tile offset
        /// </summary>
        private Vector2 layerTilesOffset;
        /// <summary>
        /// The offset of a tile
        /// </summary>
        private Vector2 tileOffset;
        /// <summary>
        /// The amount of tiles in the bounding box
        /// </summary>
        private Vector2 boundingBoxTiles;
        /// <summary>
        /// The amount of meters of the bounding box
        /// </summary>
        private Vector2 boundingBoxMeters;
        /// <summary>
        /// The minimap top left x/y position
        /// </summary>
        private Vector2RD minimapTopLeft;
        /// <summary>
        /// The rect transform of the gameobject the wmts is attached on
        /// </summary>
        private RectTransform rectTransform;
        /// <summary>
        /// The rect transform of the minimap UI
        /// </summary>
        private RectTransform rectTransformUI;

        /// <summary>
        /// Contains the tiles from each layer
        /// </summary>
        private Dictionary<int, Dictionary<Vector2, Tile>> tileLayers = new Dictionary<int, Dictionary<Vector2, Tile>>();

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            rectTransformUI = GetComponentInParent<RectTransform>();
        }

        // Start is called before the first frame update
        void Start()
        {
            layerIndex = layerStartIndex;
            tileSize = config.tileSize;
            mapSizeMeters = tileSize * config.pixelsInMeter * config.scaleDenominator;
            boundingBoxMeters = new Vector2((float)config.topRight.x - (float)config.bottomLeft.x, (float)config.topRight.y - (float)config.bottomLeft.y);

            SetupMinimapTopLeft();
            CalculateGridScaling();
            ActivateLayerIndex();

            startMeterInPixels = (float)tileSizeMeters / tileSize;
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        /// <summary>
        /// User clicked on MinimapUI
        /// </summary>
        /// <param name="eventData"></param>
        public void ClickedMap(PointerEventData eventData)
        {
            // The point clicked on the map in local coordinates
            Vector3 localClickPosition = transform.InverseTransformPoint(eventData.position);

            // Distance in meters from the top left corner of the map
            Vector2 meterDistance = localClickPosition * startMeterInPixels;
            Vector3 coordinateRD = CoordConvert.RDtoUnity(new Vector3RD((float)config.bottomLeft.x + meterDistance.x, (float)config.topRight.y + meterDistance.y, Camera.main.transform.position.y));
            Camera.main.transform.position = coordinateRD;
        }

        /// <summary>
        /// Position a rect transform on the map
        /// </summary>
        /// <param name="target"></param>
        /// <param name="targetPosition"></param>
        public void PositionOnMap(RectTransform target, Vector3RD targetPosition)
        {
            target.transform.localScale = Vector3.one / rectTransform.localScale.x;
            target.transform.localPosition = DeterminePositionOnMap(targetPosition);
        }

        /// <summary>
        /// Update the WMTS tiles, normally called from MinimapUI OnDrag
        /// </summary>
        public void UpdateTiles()
        {
            Debug.Log("update t");
            Clamp();

            Vector2 position;
            Vector2 tileKey;

            // Update the x / y tile 2D grid
            for(int x = 0; x < boundingBoxTiles.x; x++)
            {
                for(int y = 0; y < boundingBoxTiles.y; y++)
                {
                    // Tile position within this container
                    position = new Vector2((x * tileSize) - (layerTilesOffset.x * tileSize), -((y * tileSize) - (layerTilesOffset.y * tileSize)));

                    // Origin alignment determines the way to count the grid
                    tileKey = config.alignment switch
                    {
                        Configuration.OriginAlignment.bottomLeft => new Vector2(x + tileOffset.x, (float)(divide - 1) - (y + tileOffset.y)),
                        Configuration.OriginAlignment.topLeft => new Vector2(x + tileOffset.x, y + tileOffset.y),
                        _ => throw new System.Exception("Invalid enum value")
                    };

                    // Check if the tile position is viewable
                    Vector2 comparePosition = new Vector2(position.x * rectTransform.localScale.x + rectTransform.position.x, position.y * rectTransform.localScale.x + rectTransform.localPosition.y); // note: different
                    
                    bool xWithinView = comparePosition.x + config.tileSize > 0 && comparePosition.x < rectTransformUI.sizeDelta.x;
                    bool yWithinView = comparePosition.y > 0 && comparePosition.y - config.tileSize < rectTransformUI.sizeDelta.y;

                    if(xWithinView && yWithinView)
                    {
                        if(!tileLayers[layerIndex].ContainsKey(tileKey))
                        {
                            // Add a new tile
                            GameObject a = new GameObject();
                            Tile tile = a.AddComponent<Tile>();
                            tile.Initialize(layerIndex, tileSize, position, tileKey, config);
                            tile.transform.SetParent(transform);
                            tileLayers[layerIndex].Add(tileKey, tile);
                        }
                    }
                    else if(tileLayers[layerIndex].ContainsKey(tileKey))
                    {
                        // Remove tile
                        Destroy(tileLayers[layerIndex][tileKey].gameObject);
                        tileLayers[layerIndex].Remove(tileKey);
                    }
                }
            }

            MovePointer();
        }

        /// <summary>
        /// Zoom in or out
        /// </summary>
        /// <param name="zoom">The zoom level to be added</param>
        public void Zoom(int zoom)
        {
            tileSize = config.tileSize / Mathf.Pow(2, zoom);

            LayerIndex = layerStartIndex + zoom;
        }

        /// <summary>
        /// Activates the current layer index tiles
        /// </summary>
        private void ActivateLayerIndex()
        {
            // Check if the tileLayers already contains the current layerIndex, else add it
            if(!tileLayers.ContainsKey(layerIndex)) tileLayers.Add(layerIndex, new Dictionary<Vector2, Tile>());
        }

        /// <summary>
        /// Calculate the grid scaling of the minimap
        /// </summary>
        private void CalculateGridScaling()
        {
            divide = Mathf.Pow(2, layerIndex);
            tileSizeMeters = mapSizeMeters / divide;

            // The tile (0,0) top left does not align with this region top left, so we calculate the offset
            layerTilesOffset = new Vector2((float)config.bottomLeft.x - (float)minimapTopLeft.x, (float)minimapTopLeft.y - (float)config.topRight.y) / (float)tileSizeMeters;

            // Based on tile numbering type (??? old comment)
            tileOffset = new Vector2(Mathf.Floor(layerTilesOffset.x), Mathf.Floor(layerTilesOffset.y));

            // Store the remaining value to offset layer (??? old comment)
            layerTilesOffset -= tileOffset;

            // Calculate the amount of tiles needed for the bounding box
            boundingBoxTiles = new Vector2(Mathf.CeilToInt(boundingBoxMeters.x / (float)tileSizeMeters), Mathf.CeilToInt(boundingBoxMeters.y / (float)tileSizeMeters));
        }

        /// <summary>
        /// Clamp the minimap
        /// </summary>
        private void Clamp()
        {
            Vector2 maxPosUnits = new Vector2(-(boundingBoxMeters.x / startMeterInPixels), (boundingBoxMeters.y / startMeterInPixels)) * transform.localScale.x;

            transform.localPosition = new Vector3(Mathf.Clamp(transform.localPosition.x, maxPosUnits.x + rectTransformUI.sizeDelta.x, 0), Mathf.Clamp(transform.localPosition.y, rectTransformUI.sizeDelta.y, maxPosUnits.y), 0);
        }

        /// <summary>
        /// Determine the RD position on the map
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private Vector3 DeterminePositionOnMap(Vector3RD position)
        {
            Vector2RD meters = new Vector2RD(position.x - (float)config.bottomLeft.x, position.y - (float)config.topRight.y);
            Vector2RD pixels = new Vector2RD(meters.x / startMeterInPixels, meters.y / startMeterInPixels);

            return new Vector3((float)pixels.x, (float)pixels.y);
        }

        /// <summary>
        /// Move the pointer to correct position
        /// </summary>
        private void MovePointer()
        {
            rectTransformFOV.SetAsLastSibling();
            rectTransformPointer.SetAsLastSibling();

            PositionOnMap(rectTransformPointer, CoordConvert.UnitytoRD(Camera.main.transform.position));

            if(CenterPointerInView) transform.localPosition = -rectTransformPointer.localPosition * rectTransform.localScale.x + (Vector3)rectTransformUI.sizeDelta * 0.5f;
        }

        /// <summary>
        /// Removes layers that arent from the current layerIndex
        /// </summary>
        private void RemoveNonIndexLayers()
        {
            // Item being the tileLayer key index
            foreach(int item in tileLayers.Keys)
            {
                // Remove all layers behind the top layer except the first & one below layerIndex
                if(item < layerIndex - 1 && item != layerStartIndex || item > layerIndex)
                {
                    // Destroy all gameobjects from the layer
                    foreach(var tile in tileLayers[item]) Destroy(tile.Value.gameObject);
                    // Remove the layer
                    tileLayers.Remove(item);
                }
            }
        }

        /// <summary>
        /// Setup the minimapTopLeft value based on the alignment
        /// </summary>
        private void SetupMinimapTopLeft()
        {
            minimapTopLeft = config.alignment switch
            {
                Configuration.OriginAlignment.topLeft => config.minimapTopLeft,
                Configuration.OriginAlignment.bottomLeft => new Vector2RD(config.minimapTopLeft.x, config.minimapTopLeft.y + mapSizeMeters),
                _ => throw new System.Exception("Invalid enum value")
            };
        }
    }
}
