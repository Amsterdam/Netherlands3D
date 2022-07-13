public class Datastreams
{
    public string iotnextLink { get; set; }
    public Value[] value { get; set; }


    public class Value
    {
        public int iotid { get; set; }
        public string iotselfLink { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public Unitofmeasurement unitOfMeasurement { get; set; }
        public string observationType { get; set; }
        public string ThingiotnavigationLink { get; set; }
        public string SensoriotnavigationLink { get; set; }
        public string ObservationsiotnavigationLink { get; set; }
        public string ObservedPropertyiotnavigationLink { get; set; }
    }

    public class Unitofmeasurement
    {
        public string definition { get; set; }
        public string symbol { get; set; }
    }
}
