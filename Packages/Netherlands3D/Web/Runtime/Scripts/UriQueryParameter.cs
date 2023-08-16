namespace Netherlands3D.Web
{
    public readonly struct UriQueryParameter
    {
        public readonly string Key;
        public readonly string Value;

        public UriQueryParameter(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Key}={Value}";
        }
    }
}
