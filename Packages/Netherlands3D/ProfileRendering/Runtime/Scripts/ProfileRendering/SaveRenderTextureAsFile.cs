using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SaveRenderTextureAsFile : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void DownloadFile(string callbackGameObjectName, string callbackMethodName, string fileName, byte[] array, int byteLength);

    [SerializeField] private RenderTexture renderTexure;

    public void DownloadAsPNG()
    {
        RenderTexture.active = renderTexure;
        Texture2D texture = new Texture2D(renderTexure.width, renderTexure.height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, renderTexure.width, renderTexure.height), 0, 0);
        RenderTexture.active = null;

        byte[] bytes;
        bytes = texture.EncodeToPNG();

    #if !UNITY_EDITOR && UNITY_WEBGL
        DownloadFile(this.gameObject.name, nameof(FileSaved), "Profile.png", bytes, bytes.Length);
    #elif UNITY_EDITOR
        string path = EditorUtility.SaveFilePanel("Save profile PNG", "", "Profile.png","png");
        if (path.Length != 0)
        {
            File.WriteAllBytes(path, bytes);
        }
    #endif
    }

    public void FileSaved()
    {
        Debug.Log("PNG saved");
    }
}
