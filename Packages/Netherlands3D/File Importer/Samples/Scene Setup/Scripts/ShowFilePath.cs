using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
using TMPro;

namespace Netherlands3D.FileImporter.Samples
{
    public class ShowFilePath : MonoBehaviour
    {
        public StringEvent eventFileLoaderFileImported;
        public TextMeshProUGUI text;

        // Start is called before the first frame update
        void Start()
        {
            eventFileLoaderFileImported.started.AddListener(x => text.text = x);
        }
    }
}