using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugReader : MonoBehaviour
{
    public string Url;
    public UrlReader urlReader;

    public void ReadURLInEditor()
    {
        urlReader.GetFromURL(Url);
    }
}
