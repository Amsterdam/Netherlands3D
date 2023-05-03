#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Netherlands3D.Authentication
{
    public static class WebGLAssetInstaller
    {
        [MenuItem("Netherlands3D/Authentication/Import WebGL Assets")]
        static void Copy()
        {
            var templatesFolder = $"{Application.dataPath}/WebGLTemplates/";
            if (!Directory.Exists(templatesFolder) || Directory.GetDirectories(templatesFolder).Length == 0)
            {
                EditorUtility.DisplayDialog(
                    "Error: WebGL template(s) not found",
                    "To import the WebGL Assets for the Authentication package, you need to have a custom WebGL template present first",
                    "Ok"
                );
                return;
            }

            var packageFolder = "Packages/nl.netherlands3d.authentication/WebGLTemplates/";
            foreach (var templateDirectory in Directory.GetDirectories(templatesFolder))
            {
                var oAuthPath = $"{templateDirectory}/oauth/";
                if (!Directory.Exists(oAuthPath))
                {
                    Directory.CreateDirectory(oAuthPath);
                }

                var source = $"{packageFolder}/oauth/callback.html";
                var destination = $"{oAuthPath}/callback.html";
                File.Copy(source, destination, true);
                Debug.Log($"Successfully copied {source} to {destination}");
            }
        }
    }
}

#endif
