using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;

public static class WebCommunicator
{
    private static readonly HttpClient client = new();
    public static string GetDataFromURL(string url)
    {
        return client.GetStringAsync(url).Result;
    }


}
