using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Netherlands3D.Core.Tiles
{
    /// <summary>
    /// Tile prioritiser optimised for WebGL where we cant use threading.
    /// Modern browsers like Chrome limits parralel downloads from host to 6 per tab.
    /// Threading is not supported for WebGL, so this prioritiser spreads out actions to reduce framedrop spikes.
    /// </summary>
    public class WebTilePrioritiser : TilePrioritiser
    {
        [SerializeField] private int maxSimultaneousDownloads = 6;
        [SerializeField] AnimationCurve screenCenterWeight;

        private Vector2 viewCenter = new Vector2(0.5f, 0.5f);

        private List<Tile> prioritisedTiles = new List<Tile>();

        public override void LoadTileContent(Tile tile)
        {
            tile.content.stateChanged.AddListener(CalculatePriorities);

            prioritisedTiles.Add(tile);

            CalculatePriorities();
            // Add weights to current list of tiles
            // Center of screen 10* falloff curve
            // check the current active downloads
            // max one load per frame
            // some need to be interupted, cleared for this one?
            // center of screen priority
        }

        public override void CalculatePriorities()
        {
            foreach(var tile in prioritisedTiles)
            {
                var priorityScore = 0.0f;
                priorityScore += InViewCenterScore(tile.Bounds.center,10);
                tile.priority = priorityScore;
            }

            prioritisedTiles = prioritisedTiles.OrderBy(obj => obj.priority).ToList();

            //
        }

        /// <summary>
        /// Return a score based on position in center of view using falloff curve
        /// </summary>
        /// <param name="maxScore">Max score in screen center</param>
        /// <returns></returns>
        public float InViewCenterScore(Vector3 position, int maxScore)
        {
            var position2D = Camera.main.WorldToViewportPoint(position);
            var distance = Vector2.Distance(position2D, viewCenter);

            return maxScore * screenCenterWeight.Evaluate(1.0f - distance);
        }

        public override void RemoveTileContent(Tile tile)
        {
            //Clean up as fast as possible, directly dispose
            if (prioritisedTiles.Contains(tile))
                prioritisedTiles.Remove(tile);

            tile.Dispose();
        }
    }
}