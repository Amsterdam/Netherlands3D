using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Xml;
using System.Data;

public static class WMSFormatter 
{

    public static void DeserializeToWMS(ref XmlReader reader)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(WMSStyle));

        //try
        //{
        //    WMSStyle deserializedStyle = (WMSStyle)serializer.Deserialize(reader);
        //    Debug.Log(deserializedStyle.Name);
        //    Debug.Log(deserializedStyle.Title);
        //    Debug.Log(deserializedStyle.Width);
        //    Debug.Log(deserializedStyle.Height);
        //    Debug.Log(deserializedStyle.Format);
        //}
        //catch(System.InvalidOperationException ioe)
        //{
        //    Debug.Log(ioe.Message);
        //}
        //Debug.Log(deserializedStyle.XmlLink);
        //Debug.Log(deserializedStyle.XlinkType);
        //Debug.Log(deserializedStyle.XlinkHRef);

    }




}
