using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;

namespace Netherlands3D.Traffic
{
    /// <summary>
    /// Simulation Scriptable Objects. Collection of scriptable objects needed for simulation
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "Simulation Scriptable Objects", menuName = "ScriptableObjects/Traffic/Simulation Scriptable Objects", order = 1)]
    public class SSO : ScriptableObject
    {
        [Tooltip("If the entity should update itself in realtime on enviroment")]
        public BoolEvent eventUpdateRealtime;
        [Tooltip("Event that gets triggerd if the simulation time changes")]
        public FloatEvent eventSimulationTimeChanged;
        [Tooltip("Event that gets triggerd if the simulation speed changes")]
        public FloatEvent eventSimulationSpeedChanged;
        [Tooltip("Event that gets triggerd if the simulation state changes")]
        public IntEvent eventSimulationStateChanged;
        [Tooltip("The current VISSIM simulation time starting from 0 - infinity")]
        public FloatVariable simulationTime;
        [Tooltip("The speed multiplier to how fast the simulation is running. 1 = normal")]
        public FloatVariable simulationSpeed;
        [Tooltip("The state of the simulation time and how it gets updated. 0 = paused, 1 = play, -1 = reversed, -2 = reset")]
        public IntVariable simulationState;

        public SSO(FloatEvent eventSimulationTimeChanged, FloatEvent eventSimulationSpeedChanged, IntEvent eventSimulationStateChanged, FloatVariable simulationTime, FloatVariable simulationSpeed, IntVariable simulationState)
        {
            this.eventSimulationTimeChanged = eventSimulationTimeChanged;
            this.eventSimulationSpeedChanged = eventSimulationSpeedChanged;
            this.eventSimulationStateChanged = eventSimulationStateChanged;
            this.simulationTime = simulationTime;
            this.simulationSpeed = simulationSpeed;
            this.simulationState = simulationState;
        }
    }
}
