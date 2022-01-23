using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
using UnityEngine.UI;

public class ReadSelectedURL : MonoBehaviour
{

    // Start is called before the first frame update
    public StringEvent urlEvent;

    private void Start()
    {
        urlEvent.started.AddListener(show);
    }

    public void show(string url)
    {
        Debug.Log(url);
        GetComponent<Text>().text = url;
    }


}
