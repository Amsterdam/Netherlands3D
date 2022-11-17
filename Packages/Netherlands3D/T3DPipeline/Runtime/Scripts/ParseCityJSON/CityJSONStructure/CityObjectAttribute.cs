using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace Netherlands3D.T3DPipeline
{
    /// <summary>
    /// Custom attribute class. A CityObject can have attributes with a key and a value.
    /// </summary>
    public class CityObjectAttribute
    {
        public CityObject ParentCityObject { get; protected set; }
        public string Key { get; protected set; }
        public JSONNode Value { get; protected set; }

        public CityObjectAttribute(CityObject parentCityObject, string key)
        {
            ParentCityObject = parentCityObject;
            Key = key;
        }

        public CityObjectAttribute(CityObject parentCityObject, string key, JSONNode attributeNode)
        {
            ParentCityObject = parentCityObject;
            Key = key;
            Value = attributeNode;
        }

        public virtual JSONNode GetJSONValue()
        {
            return Value;
        }

        public static List<CityObjectAttribute> ParseAttributesNode(CityObject parentObject, JSONNode attributeNode)
        {
            var list = new List<CityObjectAttribute>();
            foreach(var node in attributeNode)
            {
                var attribute = new CityObjectAttribute(parentObject, node.Key, node.Value);
                //attribute.FromJSONNode(node.Key, node.Value);
                list.Add(attribute);
            }
            return list;
        }
    }
}
