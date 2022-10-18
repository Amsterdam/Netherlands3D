using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace Netherlands3D.T3DPipeline
{
    public class CityObjectAttribute
    {

        public string Key { get; protected set; }
        public JSONNode Value { get; protected set; }

        public CityObjectAttribute(string key)
        {
            Key = key;
        }

        public CityObjectAttribute(string key, JSONNode attributeNode)
        {
            Key = key;
            Value = attributeNode;
        }

        public virtual JSONNode GetJSONValue()
        {
            return Value;
        }

        public static List<CityObjectAttribute> ParseAttributesNode(JSONNode attributeNode)
        {
            var list = new List<CityObjectAttribute>();
            foreach(var node in attributeNode)
            {
                var attribute = new CityObjectAttribute(node.Key, node.Value);
                //attribute.FromJSONNode(node.Key, node.Value);
                list.Add(attribute);
            }
            return list;
        }
    }
}
