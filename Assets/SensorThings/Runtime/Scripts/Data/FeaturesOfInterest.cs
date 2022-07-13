namespace Netherlands3D.SensorThings
{
    public class FeaturesOfInterest
    {
        public string iotnextLink { get; set; }
        public Value[] value { get; set; }

        public class Value
        {
            public int iotid { get; set; }
            public string iotselfLink { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string encodingType { get; set; }
            public Feature feature { get; set; }
            public string ObservationsiotnavigationLink { get; set; }
        }

        public class Feature
        {
            public float[] coordinates { get; set; }
            public string type { get; set; }
        }
    }
}