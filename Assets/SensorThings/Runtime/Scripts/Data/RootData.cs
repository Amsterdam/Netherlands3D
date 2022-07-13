namespace Netherlands3D.SensorThings
{
    public class RootData
    {
        public Value[] value { get; set; }
        public class Value
        {
            public string name { get; set; }
            public string url { get; set; }
        }
    }
}