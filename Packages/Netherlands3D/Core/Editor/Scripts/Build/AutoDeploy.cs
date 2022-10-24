using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

public class AutoDeploy
{
    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        var autodeployScriptPath = pathToBuiltProject + "/../autodeploy_netherlands3d.bat";
        if (File.Exists(autodeployScriptPath))
        {
            Debug.Log("<color=#00FF00>Build complete.</color> Starting deploy script (optional)");
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
            startInfo.FileName = autodeployScriptPath;
            startInfo.Arguments = Path.GetFileNameWithoutExtension(pathToBuiltProject);
            process.StartInfo = startInfo;
            process.Start();
        }
    }
}