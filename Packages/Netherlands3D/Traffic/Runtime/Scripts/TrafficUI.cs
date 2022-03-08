using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Netherlands3D.Events;
using TMPro;

namespace Netherlands3D.Traffic
{
    /// <summary>
    /// Handles the traffic UI elements
    /// </summary>
    public class TrafficUI : MonoBehaviour
    {
        /// <summary>
        /// If the inputfield is selected by user
        /// </summary>
        public bool SimulationTimeInputFieldSelected { get; set; }
        /// <summary>
        /// If the inputfield is selected by user
        /// </summary>
        public bool SimulationSpeedInputFieldSelected { get; set; }

        [Header("Scriptable Objects")]
        [Tooltip("The scriptable objects for an entity")]
        public EntityScriptableObjects entitySO;
        [Tooltip("The database containing the traffic data")]
        [SerializeField] private DataDatabase dataDatabase;

        [Header("Components")]
        [Tooltip("The camera gameobject with the entitycamera attached")]
        [SerializeField] private GameObject prefabEntityCamera;

        [Header("UI Components")]
        [SerializeField] private TMP_InputField simulationTimeInputField;
        [SerializeField] private TMP_InputField simulationSpeedInputField;
        [SerializeField] private Slider sliderSimulationTime;
        [SerializeField] private Slider sliderSimulationSpeed;

        private EntityCamera entityCamera;

        private void OnEnable()
        {
            dataDatabase.OnAddData.AddListener(OnAddData);
            entitySO.eventSimulationSpeedChanged.started.AddListener(OnSimulationSpeedChange);
        }

        private void OnDisable()
        {
            dataDatabase.OnAddData.RemoveListener(OnAddData);
            entitySO.eventSimulationSpeedChanged.started.RemoveListener(OnSimulationSpeedChange);
        }

        private void Start()
        {
            if(entityCamera == null)
            {
                entityCamera = Instantiate(prefabEntityCamera).GetComponent<EntityCamera>();
            }
        }

        private void Update()
        {
            UpdateVisualSimulationTime();
        }

        /// <summary>
        /// Clear all data
        /// </summary>
        public void Clear()
        {
            dataDatabase.Clear();
        }

        /// <summary>
        /// Follow an random entity
        /// </summary>
        public void FollowRandomEntity()
        {
            if(entityCamera != null)
            {
                Visualizer v = FindObjectOfType<Visualizer>();
                if(v == null) return;
                if(v.transform.childCount == 0) return;
                entityCamera.SetTarget(v.transform.GetChild(Random.Range(0, v.transform.childCount)));
            }
        }

        /// <summary>
        /// Update the simulation time input field value
        /// </summary>
        public void UpdateVisualSimulationTime()
        {
            if(!SimulationTimeInputFieldSelected) simulationTimeInputField.text = entitySO.simulationTime.Value.ToString("0");
            sliderSimulationTime.SetValueWithoutNotify(entitySO.simulationTime.Value);
        }

        /// <summary>
        /// Tell the visualizer to play
        /// </summary>
        public void Play()
        {
            entitySO.simulationState.Value = 1;
        }

        /// <summary>
        /// Tell the visualizer to pause
        /// </summary>
        public void Pause()
        {
            entitySO.simulationState.Value = 0;
        }

        /// <summary>
        /// Tell the visualizer to rewind
        /// </summary>
        public void Rewind()
        {
            entitySO.simulationState.Value = -1;
        }

        /// <summary>
        /// Reset all data
        /// </summary>
        public void ResetPlay()
        {
            entitySO.simulationState.Value = -2;
        }

        /// <summary>
        /// Callback when data gets added
        /// </summary>
        /// <param name="newData"></param>
        public void OnAddData(List<int> newData)
        {
            // Update the max value of the slider
            sliderSimulationTime.maxValue = dataDatabase.MaxSimulationTime;
        }

        /// <summary>
        /// When the simulation time is changed with UI
        /// </summary>
        public void OnSimulationTimeSliderChanged()
        {
            entitySO.simulationTime.Value = sliderSimulationTime.value;
        }

        /// <summary>
        /// When the simulation speed is changed with UI
        /// </summary>
        public void OnSimulationSpeedSliderChanged()
        {
            entitySO.simulationSpeed.Value = sliderSimulationSpeed.value;
        }

        /// <summary>
        /// When the inputfield text is changed
        /// </summary>
        public void OnSimulationTimeInputFieldChanged()
        {
            if(float.TryParse(simulationTimeInputField.text, out float value))
            {
                entitySO.simulationTime.Value = value;
            }
        }

        /// <summary>
        /// When the inputfield text is changed
        /// </summary>
        public void OnSimulationSpeedInputFieldChanged()
        {
            if(float.TryParse(simulationSpeedInputField.text, out float value))
            {
                entitySO.simulationSpeed.Value = Mathf.Clamp(value, 0.01f, 20);
            }
        }

        /// <summary>
        /// Callback when the simulation speed changes
        /// </summary>
        /// <param name="value"></param>
        private void OnSimulationSpeedChange(float value)
        {
            sliderSimulationSpeed.SetValueWithoutNotify(value);
            simulationSpeedInputField.text = value.ToString();
        }
    }
}