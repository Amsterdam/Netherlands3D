using System;
using System.Collections.Specialized;
using System.Linq;

namespace Netherlands3D.Web
{
    public static class UriExtensionMethods
    {
        public static void AddQueryParameter(this UriBuilder uriBuilder, string key, string value)
        {
            var query = uriBuilder.Query;

            uriBuilder.Query = AddQueryParameterToQueryString(key, value, query);
        }

        public static void AddQueryParameter(this UriBuilder uriBuilder, UriQueryParameter parameter)
        {
            AddQueryParameter(uriBuilder, parameter.Key, parameter.Value);
        }

        public static void RemoveQueryParameter(this UriBuilder uriBuilder, string key)
        {
            var query = uriBuilder.Query;

            var nameValueCollection = new NameValueCollection();
            QueryStringAsNameValueCollection(query, nameValueCollection);

            //Remove if exists
            if (nameValueCollection.AllKeys.Contains(key))
                nameValueCollection.Remove(key);

            uriBuilder.Query = nameValueCollection.ToQueryString();
        }

        private static string ToQueryString(this NameValueCollection nameValueCollection)
        {
            var queryString = string.Join("&", nameValueCollection.AllKeys
                .Select(key => $"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(nameValueCollection[key])}"));

            return queryString;
        }

        /// <summary>
        /// Attempts to parse the query string and append the found elements as to the given NameValueCollection.
        /// </summary>
        public static void TryParseQueryString(this UriBuilder uriBuilder, NameValueCollection nameValueCollection)
        {
            QueryStringAsNameValueCollection(uriBuilder.Query, nameValueCollection);
        }

        /// <summary>
        /// Attempts to parse the query string and append the found elements as to the given NameValueCollection.
        /// </summary>
        public static void TryParseQueryString(this Uri uri, NameValueCollection nameValueCollection)
        {
            QueryStringAsNameValueCollection(uri.Query, nameValueCollection);
        }

        private static string AddQueryParameterToQueryString(string key, string value, string query)
        {
            var encodedKey = Uri.EscapeDataString(key);
            var encodedValue = Uri.EscapeDataString(value);
            var keyValuePair = $"{encodedKey}={encodedValue}";

            var newQueryString = keyValuePair;
            if (string.IsNullOrEmpty(query) == false)
            {
                newQueryString = $"{query.TrimStart('?')}&{newQueryString}";
            }

            return newQueryString;
        }

        /// <see href="https://gist.github.com/ranqn/d966423305ce70cbc320f319d9485fa2" />
        private static void QueryStringAsNameValueCollection(string query, NameValueCollection result)
        {
            if (string.IsNullOrEmpty(query)) return;

            var decodedLength = query.Length;
            var namePos = 0;
            var first = true;

            while (namePos <= decodedLength)
            {
                int valuePos = -1, valueEnd = -1;
                for (var q = namePos; q < decodedLength; q++)
                {
                    if ((valuePos == -1) && (query[q] == '='))
                    {
                        valuePos = q + 1;
                        continue;
                    }

                    if (query[q] != '&') continue;

                    valueEnd = q;
                    break;
                }

                if (first)
                {
                    first = false;
                    if (query[namePos] == '?')
                        namePos++;
                }

                string name;
                if (valuePos == -1)
                {
                    name = null;
                    valuePos = namePos;
                }
                else
                {
                    name = Uri.UnescapeDataString(query.Substring(namePos, valuePos - namePos - 1));
                }

                if (valueEnd < 0)
                {
                    namePos = -1;
                    valueEnd = query.Length;
                }
                else
                {
                    namePos = valueEnd + 1;
                }

                var value = Uri.UnescapeDataString(query.Substring(valuePos, valueEnd - valuePos));

                result.Add(name, value);
                if (namePos == -1)
                    break;
            }
        }
    }
}
