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

        [SerializeField] private float distanceScore = 10;
        [SerializeField] private float screenCenterScore = 10;
        [SerializeField] AnimationCurve screenCenterWeight;

        private Vector2 viewCenter = new Vector2(0.5f, 0.5f);

        private List<Tile> prioritisedTiles = new List<Tile>();
        public List<Tile> PrioritisedTiles { get => prioritisedTiles; private set => prioritisedTiles = value; }

        private Camera currentCamera;


        public override void Add(Tile tile)
        {
            tile.content.stateChanged.AddListener(CalculatePriorities);

            PrioritisedTiles.Add(tile);

            CalculatePriorities();
        }

        public override void Remove(Tile tile)
        {
            //Clean up as fast as possible, directly dispose
            if (PrioritisedTiles.Contains(tile))
                PrioritisedTiles.Remove(tile);

            CalculatePriorities();

            tile.Dispose();
        }

        public override void CalculatePriorities()
        {
            foreach(var tile in PrioritisedTiles)
            {
                var priorityScore = 0.0f;
                priorityScore += DistanceScore(tile.Bounds.center, 100, 5000, distanceScore);
                priorityScore += InViewCenterScore(tile.Bounds.center, screenCenterScore);

                tile.priority = priorityScore;
            }

            PrioritisedTiles = PrioritisedTiles.OrderBy(obj => obj.priority).ToList();

            Prioritise();
        }

        /// <summary>
        /// Apply new priority changes to the tiles
        /// by interupting tiles that fall outside the max downloads and
        /// did not start loading yet.
        /// </summary>
        private void Prioritise()
        {
            int downloadAvailable = maxSimultaneousDownloads;
            int interuptToMakeRoom = PrioritisedTiles.Count - maxSimultaneousDownloads;

            //Starting from lowest priority, abort any running downloads to make room for top of priority list
            for (int i = PrioritisedTiles.Count - 1; i >= 0; i--)
            {
                var tile = PrioritisedTiles[i];
                if (interuptToMakeRoom > 0 && tile.content.State == Content.ContentLoadState.LOADING)
                {
                    interuptToMakeRoom--;
                    tile.content.Dispose();
                }
                else if(tile.content.State == Content.ContentLoadState.NOTLOADED)
                {
                    downloadAvailable--;
                    tile.content.Load();
                }
                else if (tile.content.State == Content.ContentLoadState.READY)
                {
                    //Keep already loaded tiles
                    //This is the place to introduce max tiles in memory limits clean up
                }
            }

            for (int i = 0; i < PrioritisedTiles.Count; i++)
            {
                var tile = PrioritisedTiles[i];
                if(tile.content.State == Content.ContentLoadState.NOTLOADED)
                {
                    downloadAvailable--;
                    tile.content.Load();
                }
                else if(tile.content.State == Content.ContentLoadState.READY)
                {

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
        public float DistanceScore(Vector3 position, float minDistance, float maxDistance, float maxScore)
        {
            var distance = Vector3.Distance(currentCamera.transform.position, position);
            var distanceScore = maxScore - (Mathf.InverseLerp(distance, minDistance, maxDistance) * maxScore);

            return distanceScore;
        }

        public override void SetCamera(Camera currentMainCamera)
        {
            currentCamera = currentMainCamera;
        }
    }
}