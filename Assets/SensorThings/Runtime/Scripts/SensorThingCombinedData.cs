using Netherlands3D.SensorThings;

public class SensorThingCombinedData
{
    public Datastreams.Value datastream;
    public ObservedProperties.Value observedProperty;
    public Observations observations;

    public bool DataComplete { get => (observedProperty != null && observations != null); }
}

