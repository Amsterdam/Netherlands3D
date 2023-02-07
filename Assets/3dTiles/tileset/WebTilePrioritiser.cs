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

        private int lastFramePrioritiesCalculated = 0;
        public bool showPriorityNumbers = false;

        public override void RequestUpdate(Tile tile)
        {
            tile.requestedUpdate = true;
            tile.content.doneDownloading.AddListener(TileFinishedDownload);

            PrioritisedTiles.Add(tile);

            CalculatePriorities();
            Prioritise();
        }

        private void TileFinishedDownload()
        {
            CalculatePriorities();
            Prioritise();
        }

        public override void RequestDispose(Tile tile)
        {
            //Clean up as fast as possible, directly dispose
            if (tile.requestedUpdate)
                PrioritisedTiles.Remove(tile);

            tile.Dispose();
            tile.requestedUpdate = false;
 
            CalculatePriorities();
            Prioritise();
        }

        /// <summary>
        /// Calculates the priority list for the added tiles
        /// </summary>
        public override void CalculatePriorities()
        {
            if (lastFramePrioritiesCalculated == Time.frameCount) {
                return;
            }
            lastFramePrioritiesCalculated = Time.frameCount;

            foreach (var tile in PrioritisedTiles)
            {
                var priorityScore = 0.0f;
                //priorityScore += DistanceScore(tile);
                priorityScore += InViewCenterScore(tile.Bounds.center, screenCenterScore);

                tile.priority = priorityScore;
            }

            PrioritisedTiles = PrioritisedTiles.OrderByDescending(obj => obj.priority).ToList();
        }

        /// <summary>
        /// Apply new priority changes to the tiles
        /// by interupting tiles that fall outside the max downloads and
        /// did not start loading yet.
        /// </summary>
        private void Prioritise()
        {
            int downloadAvailable = maxSimultaneousDownloads;
            int interuptToMakeRoom = 0;

            //Determine how many downloads need to be interupted to make room for highest priority downloads
            for (int i = 0; i < PrioritisedTiles.Count; i++)
            {
                if (i > maxSimultaneousDownloads) break;

                var tile = PrioritisedTiles[i];
                if (tile.content.State == Content.ContentLoadState.NOTLOADING)
                {
                    interuptToMakeRoom++;
                }
                else if(tile.content.State == Content.ContentLoadState.DOWNLOADING)
                {
                    downloadAvailable--;
                }
            }

            //Starting from lowest priority, abort any running downloads to make room for top of priority list
            for (int i = PrioritisedTiles.Count - 1; i >= 0; i--)
            {
                var tile = PrioritisedTiles[i];
                if (interuptToMakeRoom > 0 && tile.content.State == Content.ContentLoadState.DOWNLOADING)
                {
                    interuptToMakeRoom--;
                    tile.Dispose();
                }
                else if(downloadAvailable > 0 && tile.content && tile.content.State == Content.ContentLoadState.NOTLOADING)
                {
                    downloadAvailable--;
                    tile.content.Load();
                    tile.content.doneDownloading.AddListener(TileCompletedLoading);
                }
            }
        }

        /// <summary>
        /// If a tile completed loading, recalcule priorities
        /// </summary>
        private void TileCompletedLoading()
        {
            CalculatePriorities();
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