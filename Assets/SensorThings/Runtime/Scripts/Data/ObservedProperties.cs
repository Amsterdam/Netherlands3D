namespace Netherlands3D.SensorThings
{
    public class ObservedProperties
    {
        public Value[] value { get; set; }

        public class Value
        {
            public int iotid { get; set; }
            public string iotselfLink { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string definition { get; set; }
            public string DatastreamsiotnavigationLink { get; set; }
        }
    }
}