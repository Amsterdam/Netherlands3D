namespace Netherlands3D.SensorThings
{
    public class Sensors
    {
        public Value[] value { get; set; }
        public class Value
        {
            public int iotid { get; set; }
            public string iotselfLink { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string encodingType { get; set; }
            public string metadata { get; set; }
            public string DatastreamsiotnavigationLink { get; set; }
        }
    }
}