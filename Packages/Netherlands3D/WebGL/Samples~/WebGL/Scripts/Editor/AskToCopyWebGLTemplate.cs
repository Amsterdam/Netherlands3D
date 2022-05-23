#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public class StartupWebGLTemplate
{
    
    static StartupWebGLTemplate()
    {
        var templatesFolder = Application.dataPath + "/WebGLTemplates/";
        if (!File.Exists(templatesFolder + "Netherlands3D/index.html"))
        {
            var confirmed = EditorUtility.DisplayDialog(
                "Imported WebGL template",
                "Make sure to move the Netherlands3D template from the imported Samples folder to the Assets/WebGLTemplates/ folder.\nThen you can select the template under 'Player Settings/Resolution and Presentation'.",
                "Ok"
            );
        }
    }
    
}
#endif