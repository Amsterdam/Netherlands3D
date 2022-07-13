public class Things
{
    public string iotnextLink { get; set; }
    public Value[] value { get; set; }


    public class Value
    {
        public int iotid { get; set; }
        public string iotselfLink { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public Properties properties { get; set; }
        public string LocationsiotnavigationLink { get; set; }
        public string DatastreamsiotnavigationLink { get; set; }
        public string HistoricalLocationsiotnavigationLink { get; set; }
    }

    public class Properties
    {
        public string codegemeente { get; set; }
        public string knmicode { get; set; }
        public string nh3closecode { get; set; }
        public string nh3regiocode { get; set; }
        public object nh3stadcode { get; set; }
        public string no2closecode { get; set; }
        public string no2regiocode { get; set; }
        public string no2stadcode { get; set; }
        public string owner { get; set; }
        public string pm10closecode { get; set; }
        public string pm10regiocode { get; set; }
        public string pm10stadcode { get; set; }
        public string pm25closecode { get; set; }
        public string pm25regiocode { get; set; }
        public string pm25stadcode { get; set; }
        public string project { get; set; }
    }

}
