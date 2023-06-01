using System.Linq;
using Netherlands3D.Events;
using Netherlands3D.T3DPipeline;
using UnityEngine;

namespace Netherlands3D.Windmills
{
    [RequireComponent(typeof(Windmill))]
    public class VisualizeWindmillUsingCityJson : MonoBehaviour
    {
        private const string AXIS_HEIGHT_KEY = "asHoogte";
        private const string ROTOR_DIAMETER_KEY = "rotorDiameter";
        private const string STATUS_KEY = "status";

        [SerializeField] private GameObjectEvent onCityObjectVisualized;
        private Windmill windmill;

        private void Awake()
        {
            windmill = GetComponent<Windmill>();
        }

        private void OnEnable()
        {
            onCityObjectVisualized.AddListenerStarted(OnVisualize);
        }

        private void OnDisable()
        {
            onCityObjectVisualized.RemoveListenerStarted(OnVisualize);
        }

        private void OnVisualize(GameObject obj)
        {
            if (obj != transform.parent.gameObject) return;

            var cityObject = obj.GetComponent<CityObject>();

            windmill.AxisHeight = cityObject.Attributes.First(attribute => attribute.Key == AXIS_HEIGHT_KEY).Value;
            windmill.RotorDiameter = cityObject.Attributes.First(attribute => attribute.Key == ROTOR_DIAMETER_KEY).Value;
            windmill.Status = ParseStatus(cityObject.Attributes.First(attribute => attribute.Key == STATUS_KEY).Value);
        }

        private static WindmillStatus ParseStatus(string statusString)
        {
            return statusString.ToLower() switch
            {
                "in bedrijf" => WindmillStatus.Active,
                "toekomstig" => WindmillStatus.Planned,
                "gesaneerd" => WindmillStatus.Removed,
                _ => WindmillStatus.Unknown
            };
        }
    }
}
