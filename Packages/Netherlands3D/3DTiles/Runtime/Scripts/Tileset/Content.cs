using GLTFast;
using Netherlands3D.B3DM;
using Netherlands3D.Core;
using SimpleJSON;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Netherlands3D.Tiles3D
{
    [System.Serializable]
    public class Content : MonoBehaviour, IDisposable
    {
        public string uri = "";

        private Coroutine runningContentRequest;

        [SerializeField] private Tile parentTile;
        public Tile ParentTile { get => parentTile; set => parentTile = value; }

        public UnityEvent onDoneDownloading = new();

        

        private GltfImport gltf;

        public enum ContentLoadState
        {
            NOTLOADING,
            DOWNLOADING,
            DOWNLOADED,
        }
        private ContentLoadState state = ContentLoadState.NOTLOADING;
        public ContentLoadState State
        {
            get => state;
            set
            {
                state = value;
            }
        }
#if UNITY_EDITOR
        /// <summary>
        /// Draw wire cube in editor with bounds and color coded state
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (ParentTile == null) return;

            Color color = Color.white;
            switch (State)
            {
                case ContentLoadState.NOTLOADING:
                    color = Color.red;
                    break;
                case ContentLoadState.DOWNLOADING:
                    color = Color.yellow;
                    break;
                case ContentLoadState.DOWNLOADED:
                    color = Color.green;
                    break;
                default:
                    break;
            }

            Gizmos.color = color;
            var parentTileBounds = ParentTile.ContentBounds;
            Gizmos.DrawWireCube(parentTileBounds.center, parentTileBounds.size);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(parentTileBounds.center, parentTileBounds.center + (ParentTile.priority * Vector3.up));
        }
#endif

        /// <summary>
        /// Load the content from an url
        /// </summary>
        public void Load()
        {
            if (State == ContentLoadState.DOWNLOADING || State == ContentLoadState.DOWNLOADED)
                return;

            State = ContentLoadState.DOWNLOADING;
            if (parentTile.parent != null)
            {
                parentTile.parent.IncrementLoadingChildren();
                
            }
            foreach (var child in parentTile.children)
            {
                child.IncrementLoadingParents();
            }
            runningContentRequest = StartCoroutine(
                ImportB3DMGltf.ImportBinFromURL(uri, GotGltfContent)
            );
        }

        /// <summary>
        /// After parsing gltf content spawn gltf scenes
        /// </summary>
        private async void GotGltfContent(ParsedGltf parsedGltf)
        {
            if (this == null)
            {
                if (parentTile.parent!=null)
                {
                    parentTile.parent.DecrementLoadingChildren();
                    
                }
                foreach (var child in parentTile.children)
                {
                    child.DecrementLoadingParents();
                }
                return;
                
            }

                if (parentTile.parent != null)
                {
                    
                    parentTile.parent.IncrementLoadedChildren();
                    parentTile.parent.DecrementLoadingChildren();
                }
            foreach (var child in parentTile.children)
            {
                child.IncrementLoadedParents();
                child.DecrementLoadingParents();
            }
            State = ContentLoadState.DOWNLOADED;

            var gltf = parsedGltf.gltfImport;
            if (gltf != null)
            {
                this.gltf = gltf;
                var scenes = gltf.SceneCount;
                for (int i = 0; i < scenes; i++)
                {
                    await gltf.InstantiateSceneAsync(transform, i);
                    var scene = transform.GetChild(0).transform;
                    var scenePosition = scene.localPosition;
                    scene.localPosition = Vector3.zero;
                    transform.localPosition = scenePosition;

                    if (parsedGltf.rtcCenter != null)
                    {
                        var unityFromEcef = CoordConvert.ECEFToUnity(new Vector3ECEF(parsedGltf.rtcCenter[0], parsedGltf.rtcCenter[1], parsedGltf.rtcCenter[2]));
                        transform.localPosition = unityFromEcef;
                        transform.SetParent(null);
                    }
                }
                this.gameObject.name = uri;
                this.gameObject.AddComponent<MovingOriginFollower>();
            }

            onDoneDownloading.Invoke();
        }

        /// <summary>
        /// Clean up coroutines and content gameobjects
        /// </summary>
        public void Dispose()
        {
            onDoneDownloading.RemoveAllListeners();

            //Direct abort of downloads
            if (State == ContentLoadState.DOWNLOADING && runningContentRequest != null)
            {
                if (parentTile.parent != null)
                {
                    parentTile.parent.DecrementLoadingChildren();
                }
                foreach (var child in parentTile.children)
                {
                    child.DecrementLoadingParents();
                }

                StopCoroutine(runningContentRequest);
                return;
            }
            State = ContentLoadState.NOTLOADING;

            if (gltf != null)
            {
                gltf.Dispose();

                    if (parentTile.parent != null)
                    {
                        parentTile.parent.DecrementLoadedChildren();
                    }
                    foreach (var child in parentTile.children)
                    {
                        child.DecrementLoadedParents();
                    }


                
            }
            Destroy(this.gameObject);
        }
    }
}
