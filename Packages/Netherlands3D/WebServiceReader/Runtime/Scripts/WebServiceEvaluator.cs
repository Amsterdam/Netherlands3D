using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class WebServiceEvaluator
{

    public WebServiceType GetWebServiceTypeFromXML(XmlDocument xml)
    {
        XmlElement serviceID = xml.DocumentElement["ows:ServiceIdentification"]["ows:ServiceType"];
        if (serviceID != null && serviceID.InnerText.Contains("WFS"))
        {
            return WebServiceType.WFS;
        }
        XmlElement service = xml.DocumentElement["Service"]["Name"];
        if (service != null && service.InnerText.Contains("WMS"))
        {
            return WebServiceType.WMS;
        }
        return WebServiceType.NONE;
    }

}
