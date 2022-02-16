using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System;

namespace Netherlands3D.VISSIM.Tests
{
    /// <summary>
    /// To compare the speed to see what file reading methode is faster
    /// </summary>
    [AddComponentMenu("VISSIM/VISSIM Compare File Reader Speed")]
    public class CompareFileReaderSpeed : MonoBehaviour
    {
        [Tooltip("How many lines are read before stopping the test")]
        [SerializeField] private int maxLinesToRead = 1000;
        [Tooltip("The file to read")]
        [SerializeField] private UnityEngine.Object file;

        private int lineIndex;
        private string filePath;
        private Stopwatch stopwatch = new Stopwatch();

        // Start is called before the first frame update
        void Start()
        {
            if(EditorUtility.DisplayDialog("Execute VISSIM File Reader Speed Test", "You are about to run a VISSIM test which might take a while, run the test?", "Yes", "No"))
            {
                StartTests();
            }
        }

        /*
        Conclusion Tests
        It seems that ReadAllText is better for bigger files, 
        and StreamReader for smaller files
         */
        public void StartTests()
        {
            filePath = AssetDatabase.GetAssetPath(file);
            UnityEngine.Debug.Log("[VISSIM][CFRS] filePath: " + filePath);

            TestFileReadAllText();
            TestStreamReader();
        }

        public void TestFileReadAllText()
        {
            UnityEngine.Debug.LogWarning("[VISSIM][CFRS] Start Test File.ReadAllText...");
            stopwatch.Restart();
            lineIndex = 0;

            string content = File.ReadAllText(filePath);
            string[] lines = content.Split((System.Environment.NewLine + "\n" + "\r").ToCharArray());
            foreach(var line in lines)
            {
                UnityEngine.Debug.Log(line);
                lineIndex++;
                if(lineIndex > maxLinesToRead) break;
            }

            stopwatch.Stop();
            UnityEngine.Debug.LogWarning("[VISSIM][CFRS] File.ReadAllText did it in " + stopwatch.ElapsedMilliseconds + "ms");
        }

        public void TestStreamReader()
        {
            UnityEngine.Debug.LogWarning("[VISSIM][CFRS] Start Test StreamReader...");
            stopwatch.Restart();
            lineIndex = 0;

            try
            {
                using(StreamReader sr = new StreamReader(filePath))
                {
                    string line;
                    // Read and display lines from the file until the end of
                    // the file is reached.
                    while((line = sr.ReadLine()) != null)
                    {
                        UnityEngine.Debug.Log(line);
                        lineIndex++;
                        if(lineIndex > maxLinesToRead) break;
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }

            stopwatch.Stop();
            UnityEngine.Debug.LogWarning("[VISSIM][CFRS] Streamreader did it in " + stopwatch.ElapsedMilliseconds + "ms");
        }
    }
}
