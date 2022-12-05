using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFS : IWebService
{
    public string Version { get; private set; }

    public string BaseUrl => throw new System.NotImplementedException();

    public WFS(string version)
    {
        Version = version;
    }

    public string GetCapabilities()
    {
        throw new System.NotImplementedException();
    }
}
