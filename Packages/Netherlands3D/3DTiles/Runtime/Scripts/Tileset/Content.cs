using GLTFast;
using Netherlands3D.B3DM;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Netherlands3D.Tiles3D
{
    [System.Serializable]
    public class Content : MonoBehaviour, IDisposable
    {
        public string uri = "";

        public GameObject contentGameObject;

        private UnityWebRequest runningWebRequest;
        private Coroutine runningContentRequest;

        private Tile parentTile;
        public Tile ParentTile { get => parentTile; set => parentTile = value; }

        public UnityEvent doneDownloading = new UnityEvent();

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
            State = ContentLoadState.DOWNLOADING;
            runningContentRequest = StartCoroutine(
                ImportB3DMGltf.ImportBinFromURL(uri, GotGltfContent)
            );
        }

        /// <summary>
        /// After parsing gltf content spawn gltf scenes
        /// </summary>
        private async void GotGltfContent(GltfImport gltf)
        {
            if (this == null) return;

            State = ContentLoadState.DOWNLOADED;
            if (gltf != null)
            {
                this.gltf = gltf;

                var scenes = gltf.SceneCount;
                var gameObject = new GameObject($"{parentTile.X},{parentTile.Y},{parentTile.Z} gltf scenes:{scenes}");
                for (int i = 0; i < scenes; i++)
                {
                    await gltf.InstantiateSceneAsync(gameObject.transform, i);
                }

                this.contentGameObject = gameObject;
                this.contentGameObject.transform.SetParent(this.gameObject.transform, true);
                this.contentGameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }

            doneDownloading.Invoke();
        }

        /// <summary>
        /// Clean up coroutines and content gameobjects
        /// </summary>
        public void Dispose()
        {
            doneDownloading.RemoveAllListeners();

            //Direct abort of downloads
            if (State == ContentLoadState.DOWNLOADING)
            {
                StopCoroutine(runningContentRequest);
            }
            State = ContentLoadState.NOTLOADING;

            if (contentGameObject)
            {
                gltf.Dispose();
                Destroy(contentGameObject);
            }
            Destroy(this);
        }
    }
}