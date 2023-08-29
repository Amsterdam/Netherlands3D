using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Tiles3D
{

    
    /// <summary>
    /// Base class for adding/removing tiles to a prioritised list.
    /// Derived classes can contain specific logic to determine the priority based on platform.
    /// </summary>
    public abstract class TilePrioritiser : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern bool isMobile();

        [Header("SSE Screen height limitations (0 is disabled)")]
        public int maxScreenHeightInPixels = 0;
        public int maxScreenHeightInPixelsMobile = 0;

        private bool mobileMode = false;
        public bool MobileMode { get => mobileMode; set => mobileMode = value; }
        public int MaxScreenHeightInPixels {
            get
            {
                return (mobileMode) ? maxScreenHeightInPixelsMobile: maxScreenHeightInPixels;
            }
        }

        public UnityEvent<bool> OnMobileModeEnabled;
        


        private void Awake()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            MobileMode = isMobile();
#endif
            OnMobileModeEnabled.Invoke(MobileMode);
        }

        public void SetMaxScreenHeightInPixels(float pixels)
        {
            maxScreenHeightInPixels = Mathf.RoundToInt(pixels);
        }
        public void SetMaxScreenHeightInPixelsMobile(float pixels)
        {
            maxScreenHeightInPixelsMobile = Mathf.RoundToInt(pixels);
        }

        public abstract void CalculatePriorities();
        public abstract void RequestUpdate(Tile tile);
        public abstract void RequestDispose(Tile tile, bool immediately=false);
        public abstract void SetCamera(Camera currentMainCamera);
    }
}
