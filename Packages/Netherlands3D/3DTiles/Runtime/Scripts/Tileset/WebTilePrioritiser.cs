using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Netherlands3D.Tiles3D
{
    /// <summary>
    /// Tile prioritiser optimised for WebGL where we cant use threading.
    /// Modern browsers like Chrome limits parralel downloads from host to 6 per tab.
    /// Threading is not supported for WebGL, so this prioritiser spreads out actions to reduce framedrop spikes.
    /// This prioritiser takes center-of-screen into account combined with the 3D Tile SSE to determine tile priotities.
    /// A delayed dispose list 
    /// </summary>
    /// 
    public class WebTilePrioritiser : TilePrioritiser
    { 
        [Header("Web limitations")]
        [SerializeField] private int maxSimultaneousDownloads = 6;

        [Header("Delay tile destroys"),Tooltip("Limit the amount of tiles that can be destroyed on delay")]
        [SerializeField] private int maxTilesInDisposeList = 4;

        [Header("Screen space error priority")]
        [SerializeField] private float screenSpaceErrorScoreMultiplier = 10;

        [Header("Center of screen priority")]
        [SerializeField] private float screenCenterScore = 10;
        [SerializeField] AnimationCurve screenCenterWeight;

        private Vector2 viewCenter = new Vector2(0.5f, 0.5f);

        private List<Tile> delayedDisposeList = new List<Tile>();
        private List<Tile> prioritisedTiles = new List<Tile>();
        public List<Tile> PrioritisedTiles { get => prioritisedTiles; private set => prioritisedTiles = value; }

        private bool requirePriorityCheck = false;
        public bool showPriorityNumbers = false;

        [SerializeField]
        private int downloadAvailable = 0;

        private Camera currentCamera;

        private bool pauseNewDownloads = false;

        public void PauseDownloads(bool paused)
        {
            pauseNewDownloads = paused;
        }

        /// <summary>
        /// If a tile completed loading, recalcule priorities
        /// </summary>
        private void TileCompletedLoading()
        {
            requirePriorityCheck = true;
        }

        /// <summary>
        /// Request update for this tile by adding it to the prioritised tile list.
        /// Highest priority will be loaded first.
        /// </summary>
        public override void RequestUpdate(Tile tile)
        {
            requirePriorityCheck = true;
            tile.requestedUpdate = true;

            PrioritisedTiles.Add(tile);
        }

        /// <summary>
        /// Add this tile to the dispose list.
        /// Using this list we can keep parent tiles visible untill their loading children are done.
        /// </summary>
        public override void RequestDispose(Tile tile, bool immediately=false)
        {
            PrioritisedTiles.Remove(tile);
            requirePriorityCheck = true;

            bool anyChildLoading = false;
            tile.requestedDispose = true;
            tile.childrenCountDelayingDispose = 0;

            if (tile.CountLoadingChildren()+tile.CountLoadedChildren()>0) // there are active children
            {
                if (tile.CountLoadingChildren() > 0)
                {
                    anyChildLoading = true;
                }
            }

            if(anyChildLoading && immediately==false)
            {
                delayedDisposeList.Add(tile);
            }
            else
            {
                Dispose(tile);
            }

        }

        /// <summary>
        /// Check the list of tiles where the dispose was delayed
        /// </summary>
        private void CheckDelayedDispose()
        {
            if (delayedDisposeList.Count > 0)
            {
                for (int i = delayedDisposeList.Count - 1; i >= 0; i--)
                {
                    var tile = delayedDisposeList[i];
                    int loadingchildcount = tile.CountLoadingChildren();

                    foreach (var child in tile.children)
                    {
                        if (loadingchildcount==0)
                        {
                            Dispose(tile);
                            delayedDisposeList.RemoveAt(i);
                        }
                    }

                    
                }
            }
        }
        

        /// <summary>
        /// Directly dispose this tile content
        /// </summary>
        public void Dispose(Tile tile)
        {
            
           
            tile.Dispose();
            tile.requestedUpdate = false;
            tile.requestedDispose = false;
        }

        private void LateUpdate()
        {
            if(requirePriorityCheck)
            {
                CalculatePriorities();
            }

            //Check delayed tile expose status
            CheckDelayedDispose();
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
                priorityScore += sseScore(tile);
                int loadedChildren = tile.CountLoadedChildren();
                int loadedParents = tile.CountLoadedParents();
                if (loadedParents<1) // no parents loaded
                {
                    priorityScore *= 10;
                }
                tile.priority = (int)priorityScore;
            }

            PrioritisedTiles.Sort((obj1, obj2) => obj2.priority.CompareTo(obj1.priority));
            Apply();
        }

        /// <summary>
        /// Apply new priority changes to the tiles
        /// and start new downloads for the highest priority tiles if there is a download slot available.
        /// </summary>
        private void Apply()
        {
            var downloading = PrioritisedTiles.Count(tile => tile.content.State == Content.ContentLoadState.DOWNLOADING);
            downloadAvailable = maxSimultaneousDownloads - downloading;

            //Start a new download first the highest priority if a slot is available
            for (int i = 0; i < PrioritisedTiles.Count; i++)
            {
                if (downloadAvailable <= 0) break;

                var tile = PrioritisedTiles[i];
                if (!pauseNewDownloads && tile.content && tile.content.State == Content.ContentLoadState.NOTLOADING)
                {
                    downloadAvailable--;
                    tile.content.Load();
                    tile.content.onDoneDownloading.AddListener(TileCompletedLoading);
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


        public float sseScore(Tile tile)
        {
            float result = 0;
            result = tile.screenSpaceError * 100;
            return result;
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
