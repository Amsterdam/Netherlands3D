using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWebService
{
    public string BaseUrl { get; }
    string GetCapabilities();
}

public interface IWSMappable
{
    public string BaseUrl { get; }
    string GetMapRequest();
}
