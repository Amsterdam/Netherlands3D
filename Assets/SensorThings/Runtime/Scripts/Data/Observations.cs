using System;

namespace Netherlands3D.SensorThings
{
    public class Observations
    {
        public string iotnextLink { get; set; }
        public Value[] value { get; set; }
        public class Value
        {
            public int iotid { get; set; }
            public string iotselfLink { get; set; }
            public string phenomenonTime { get; set; }
            public float result { get; set; }
            public string DatastreamiotnavigationLink { get; set; }
            public string FeatureOfInterestiotnavigationLink { get; set; }
            public object resultTime { get; set; }
        }
    }
}