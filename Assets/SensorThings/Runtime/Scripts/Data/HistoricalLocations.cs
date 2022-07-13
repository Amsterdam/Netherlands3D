namespace Netherlands3D.SensorThings
{
    public class HistoricalLocations
    {
        public string iotnextLink { get; set; }
        public Value[] value { get; set; }

        public class Value
        {
            public int iotid { get; set; }
            public string iotselfLink { get; set; }
            public string time { get; set; }
            public string ThingiotnavigationLink { get; set; }
            public string LocationsiotnavigationLink { get; set; }
        }
    }
}