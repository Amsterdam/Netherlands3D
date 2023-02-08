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
        [Header("Web limitations")]
        [SerializeField] private int maxSimultaneousDownloads = 6;

        [Header("Screen space error priority")]
        [SerializeField] private float screenSpaceErrorScoreMultiplier = 10;

        [Header("Center of screen priority")]
        [SerializeField] private float screenCenterScore = 10;
        [SerializeField] AnimationCurve screenCenterWeight;

        private Vector2 viewCenter = new Vector2(0.5f, 0.5f);

        [SerializeField] private List<Tile> prioritisedTiles = new List<Tile>();
        public List<Tile> PrioritisedTiles { get => prioritisedTiles; private set => prioritisedTiles = value; }

        private Camera currentCamera;

        private bool requirePriorityCheck = false;
        public bool showPriorityNumbers = false;

        /// <summary>
        /// If a tile completed loading, recalcule priorities
        /// </summary>
        private void TileCompletedLoading()
        {
            requirePriorityCheck = true;
        }

        public override void RequestUpdate(Tile tile)
        {
            requirePriorityCheck = true;

            tile.requestedUpdate = true;
            PrioritisedTiles.Add(tile);
        }

        public override void RequestDispose(Tile tile)
        {
            PrioritisedTiles.Remove(tile);

            requirePriorityCheck = true;

            tile.Dispose();
            tile.requestedUpdate = false;
        }

        private void LateUpdate()
        {
            if(requirePriorityCheck)
            {
                CalculatePriorities();
            }
        }

        /// <summary>
        /// Calculates the priority list for the added tiles
        /// </summary>
        public override void CalculatePriorities()
        {
            foreach (var tile in PrioritisedTiles)
            {
                var priorityScore = 0.0f;
                priorityScore += DistanceScore(tile);
                priorityScore += InViewCenterScore(tile.ContentBounds.center, screenCenterScore);

                tile.priority = (int)priorityScore;
            }

            PrioritisedTiles.Sort((obj1, obj2) => obj2.priority.CompareTo(obj1.priority));
            Apply();
        }

        /// <summary>
        /// Apply new priority changes to the tiles
        /// by interupting tiles that fall outside the max downloads and
        /// did not start loading yet.
        /// </summary>
        private void Apply()
        {
            int downloadAvailable = maxSimultaneousDownloads;

            //Starting from lowest priority, abort any running downloads to make room for top of priority list
            for (int i = 0; i < PrioritisedTiles.Count; i++)
            {
                var tile = PrioritisedTiles[i];
                if (downloadAvailable > 0 && tile.content && tile.content.State == Content.ContentLoadState.NOTLOADING)
                {
                    downloadAvailable--;
                    tile.content.Load();
                    tile.content.doneDownloading.AddListener(TileCompletedLoading);
                }
            }
        }

        /// <summary>
        /// Return a score based on world position in center of view using falloff curve
        /// </summary>
        /// <param name="maxScore">Max score in screen center</param>
        /// <returns></returns>
        public float InViewCenterScore(Vector3 position, float maxScore)
        {
            var position2D = Camera.main.WorldToViewportPoint(position);
            var distance = Vector2.Distance(position2D, viewCenter);

            return maxScore * screenCenterWeight.Evaluate(1.0f - distance);
        }

        /// <summary>
        /// Return higher score for closer position to current target camera
        /// </summary>
        /// <param name="position">World position to compare distance to camera</param>
        /// <param name="minDistance">The distance where score is maximum</param>
        /// <param name="maxDistance">The distance where score becomes 0</param>
        /// <param name="maxScore">Max score for closest object</param>
        /// <returns></returns>
        public float DistanceScore(Tile tile)
        {
            return tile.screenSpaceError * screenSpaceErrorScoreMultiplier;
        }

        public override void SetCamera(Camera currentMainCamera)
        {
            currentCamera = currentMainCamera;
        }
    }
}