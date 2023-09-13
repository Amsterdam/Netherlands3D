using UnityEngine;

namespace Netherlands3D.Windmills
{
    public class Windmill : MonoBehaviour
    {
        public float RotorDiameter
        {
            get => rotorDiameter;
            set
            {
                rotorDiameter = value;
                Resize();
            }
        }

        public float AxisHeight
        {
            get => axisHeight;
            set
            {
                axisHeight = value;
                Resize();
            }
        }

        public WindmillStatus Status
        {
            get => status;
            set
            {
                status = value;
                Resize();
            }
        }

        [Header("Settings")]
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private WindmillStatus status;
        [SerializeField] private float axisHeight = 120f;
        [SerializeField] private float rotorDiameter = 120f;
        [SerializeField] private float defaultHeight = 120f;
        [SerializeField] private float defaultDiameter = 120f;

        [Header("Models")]
        [SerializeField] private GameObject windmillBase;
        [SerializeField] private GameObject windmillAxis;
        [SerializeField] private GameObject windmillRotorConnection;
        [SerializeField] private GameObject windmillRotor;

        private Vector3 axisBasePosition;

        private void Awake()
        {
            axisBasePosition = windmillAxis.transform.localPosition;
        }

        private void Start()
        {
            // Do initial resize, in case properties were set in inspector
            Resize();
        }

        private void Resize()
        {
            if (axisHeight == 0)
            {
                Debug.LogWarning($"Windmill {name} has no height, using fallback height");
                axisHeight = defaultHeight;
            }

            if (rotorDiameter == 0)
            {
                Debug.LogWarning($"Windmill {name} has no diameter, using fallback diameter");
                rotorDiameter = defaultDiameter;
            }

            var rotorScale = rotorDiameter * 0.5f;
            var baseScale = axisHeight * 0.1f;

            windmillBase.transform.localScale = new Vector3(baseScale, baseScale, axisHeight);
            windmillAxis.transform.localPosition = new Vector3(axisBasePosition.x, axisHeight, axisBasePosition.z);
            windmillAxis.transform.localScale = baseScale * Vector3.one;
            windmillRotor.transform.position = windmillRotorConnection.transform.position;
            windmillRotor.transform.localScale = rotorScale * Vector3.one;
        }

        private void Update()
        {
            var windmillRotorTransform = windmillRotor.transform;
            var windmillRotorPosition = windmillRotorTransform.position;
            Debug.DrawLine(windmillRotorPosition, windmillRotorPosition + windmillRotorTransform.forward);

            windmillRotorTransform.Rotate(Vector3.up, Time.deltaTime * rotationSpeed, Space.Self);
        }
    }
}
