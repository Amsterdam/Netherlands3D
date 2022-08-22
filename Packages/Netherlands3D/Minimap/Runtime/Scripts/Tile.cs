using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Netherlands3D.Minimap
{
    public class Tile : MonoBehaviour
    {
        /// <summary>
        /// The layerIndex of the minimap
        /// </summary>
        private int layerIndex;
        /// <summary>
        /// The key of this tile (position on the minimap)
        /// </summary>
        private Vector2 key;
        /// <summary>
        /// The raw image used for minimap texture
        /// </summary>
        private RawImage rawImage;

        private Configuration config;

        private UnityWebRequest request;

        private void OnDestroy()
        {
            StopAllCoroutines();
            // Cleanup
            if(request != null) request.Dispose();
            Destroy(rawImage.texture);
            rawImage.texture = null;
            Destroy(rawImage);
        }

        public void Initialize(int layerIndex, Vector2 sizeDelta, Vector2 position, Vector2 key, Configuration config)
        {
            this.layerIndex = layerIndex;
            this.key = key;
            this.config = config;
            transform.localPosition = position;

            // Generate raw image
            rawImage = gameObject.AddComponent<RawImage>();
            rawImage.raycastTarget = false;
            rawImage.rectTransform.pivot = new Vector2(0, 1);
            //rawImage.rectTransform.anchorMin = rawImage.rectTransform.anchorMax = new Vector2(0, 1);
            rawImage.rectTransform.localScale = Vector3.one;
            rawImage.rectTransform.sizeDelta = sizeDelta;
            rawImage.enabled = false;
            rawImage.color = new Color(1f, 1f, 1f, 0);

            StartCoroutine(LoadTexture());
        }

        /// <summary>
        /// Loads the texture from the internet based on zoomIndex, & key
        /// </summary>
        /// <returns></returns>
        private IEnumerator LoadTexture()
        {
            string url = config.serviceUrl.Replace("{zoom}", layerIndex.ToString()).Replace("{x}", key.x.ToString()).Replace("{y}", key.y.ToString());

            using(request = UnityWebRequestTexture.GetTexture(url, true))
            {
                yield return request.SendWebRequest();

                if(request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning("[Minimap] Could not find minimap tile: " + key.ToString());
                }
                else
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(request);
                    texture.wrapMode = TextureWrapMode.Clamp;
                    rawImage.texture = texture;
                    rawImage.enabled = true;
                    StartCoroutine(RawImageFadeIn());
                }
            }
        }

        private IEnumerator RawImageFadeIn()
        {
            while(rawImage.color.a < 1f)
            {
                rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, rawImage.color.a + 3 * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
