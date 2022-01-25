using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Netherlands3D.Geoservice
{

    public abstract class ImageGeoservice
    {
        public abstract bool UrlIsValid(string url);

        public abstract string getType();

        public abstract bool readCapabilities(ServerData serverData, string xmlstring);
    }

    //public class wmts : ImageGeoservice
    //{
    //    public override string getType()
    //    {
    //        return "WMTS";
    //    }

    //    public override void readCapabilities(ServerData serverData, string xmlstring)
    //    {
    //        Debug.Log("wmts inlezen");
    //    }

    //    public override bool UrlIsValid(string url)
    //    {

    //        if (url.Contains("http") == false)
    //        {
    //            return false;
    //        }

    //        if (url.Contains("request=GetCapabilities") == false)
    //        {
    //            return false;
    //        }
    //        if (url.Contains("service=wmts") == false && url.Contains("service=WMTS")==false)
    //        {
    //            return false;
    //        }

    //        return true;

    //    }
    //}
}
