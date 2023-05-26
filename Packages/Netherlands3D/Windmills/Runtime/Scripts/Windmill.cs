using System.Linq;
using Netherlands3D.Events;
using Netherlands3D.T3DPipeline;
using UnityEngine;

namespace Netherlands3D.Windmills
{
    public class Windmill : MonoBehaviour
    {
        private const string AXIS_HEIGHT_KEY = "asHoogte";
        private const string ROTOR_DIAMETER_KEY = "rotorDiameter";
        private const string STATUS_KEY = "status";

        public float RotorDiameter { get; private set; }
        public float AxisHeight { get; private set; }
        public WindmillStatus Status { get; private set; }


        [SerializeField] private GameObjectEvent onCityObjectVisualized;

        [SerializeField] private GameObject windmillBase;
        [SerializeField] private GameObject windmillAxis;
        private Vector3 axisBasePosition;
        [SerializeField] private GameObject windmillRotorConnection;
        [SerializeField] private GameObject windmillRotor;
        [SerializeField] private float rotationSpeed = 10f;

        float fallbackHeight = 120f;
        float fallbackDiameter = 120f;

        private void Awake()
        {
            axisBasePosition = windmillAxis.transform.localPosition;
        }

        private void OnEnable()
        {
            onCityObjectVisualized.AddListenerStarted(Initialize);
        }

        public void Initialize(GameObject obj)
        {
            if (obj != transform.parent.gameObject)
                return;

            var cityObject = obj.GetComponent<CityObject>();

            AxisHeight = cityObject.Attributes.First(attribute => attribute.Key == AXIS_HEIGHT_KEY).Value;
            RotorDiameter = cityObject.Attributes.First(attribute => attribute.Key == ROTOR_DIAMETER_KEY).Value;
            Status = ParseStatus(cityObject.Attributes.First(attribute => attribute.Key == STATUS_KEY).Value);

            if (Status == WindmillStatus.Planned)
            {
                if (AxisHeight == 0 || RotorDiameter == 0)
                {
                    print("Windmill " + cityObject.Id +
                          " has no height or diameter, using fallback height and diameter");
                    AxisHeight = fallbackHeight;
                    RotorDiameter = fallbackDiameter;
                }
            }

            windmillBase.transform.localScale = new Vector3(AxisHeight / 10, AxisHeight / 10, AxisHeight);
            windmillAxis.transform.localPosition = new Vector3(axisBasePosition.x, AxisHeight, axisBasePosition.z);
            windmillAxis.transform.localScale = AxisHeight * 0.1f * Vector3.one;
            windmillRotor.transform.localScale = new Vector3(RotorDiameter / 2, RotorDiameter / 2, RotorDiameter / 2);
            windmillRotor.transform.position = windmillRotorConnection.transform.position;
        }

        private static WindmillStatus ParseStatus(string statusString)
        {
            if (statusString.ToLower() == "in bedrijf")
                return WindmillStatus.Active;
            if (statusString.ToLower() == "toekomstig")
                return WindmillStatus.Planned;
            if (statusString.ToLower() == "gesaneerd")
                return WindmillStatus.Removed;

            return WindmillStatus.Unknown;
        }

        private void Update()
        {
            Debug.DrawLine(windmillRotor.transform.position,
                windmillRotor.transform.position + windmillRotor.transform.forward);
            windmillRotor.transform.Rotate(Vector3.up, Time.deltaTime * rotationSpeed, Space.Self);
        }
    }
}
