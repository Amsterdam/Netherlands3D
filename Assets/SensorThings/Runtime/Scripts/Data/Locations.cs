namespace Netherlands3D.SensorThings
{
    public class Locations
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
            public Location location { get; set; }
            public string ThingsiotnavigationLink { get; set; }
            public string HistoricalLocationsiotnavigationLink { get; set; }
        }

        public class Location
        {
            public float[] coordinates { get; set; }
            public string type { get; set; }
        }

    }
}