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
        [Tooltip("The scriptable objects for an entity")]
        public EntityScriptableObjects entitySO;

        [Header("UI Components")]
        [SerializeField] private TMP_InputField simulationTimeInputField;
        [SerializeField] private TMP_InputField simulationSpeedInputField;
        [SerializeField] private Slider sliderSimulationTime;
        [SerializeField] private Slider sliderSimulationSpeed;

        // Start is called before the first frame update
        void Start()
        {
        
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

        }

        /// <summary>
        /// Update the simulation time input field value
        /// </summary>
        public void UpdateVisualSimulationTime()
        {
            simulationTimeInputField.text = entitySO.simulationTime.Value.ToString("0");
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
        /// When the simulation time is changed with UI
        /// </summary>
        public void OnSimulationTimeSliderChanged()
        {

        }

        /// <summary>
        /// When the simulation speed is changed with UI
        /// </summary>
        public void OnSimulationSpeedSliderChanged()
        {

        }

        public void OnSimulationTimeInputFieldChanged()
        {

        }

        public void OnSimulationSpeedInputFieldChanged()
        {

        }
    }
}
