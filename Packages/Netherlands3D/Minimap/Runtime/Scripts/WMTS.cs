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

        [Header("Components")]
        [Tooltip("The configuration of the minimap")]
        [SerializeField] private Configuration config;

        /// <summary>
        /// Center the pointer in the view
        /// </summary>
        /// <see cref="CenterPointerInView"/>
        private bool centerPointerInView;
        /// <summary>
        /// The current layer index of the minimap (from which layer images need to be loaded)
        /// </summary>
        private int layerIndex = 5;

        /// <summary>
        /// 
        /// </summary>
        private float startMeterInPixels;
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
        /// Contains the tiles from each layer
        /// </summary>
        private Dictionary<int, Dictionary<Vector2, Tile>> tileLayers = new Dictionary<int, Dictionary<Vector2, Tile>>();

        // Start is called before the first frame update
        void Start()
        {
            mapSizeMeters = config.tileSize * config.pixelsInMeter * config.scaleDenominator;
            boundingBoxMeters = new Vector2((float)config.topRight.x - (float)config.bottomLeft.x, (float)config.topRight.y - (float)config.bottomLeft.y);

            SetupMinimapTopLeft();
            CalculateGridScaling();

            startMeterInPixels = (float)tileSizeMeters / config.tileSize;
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
