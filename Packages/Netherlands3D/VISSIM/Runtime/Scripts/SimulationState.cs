using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.VISSIM
{
    /// <summary>
    /// The state the simulationTime is in and how it gets updated
    /// </summary>
    public enum SimulationState
    {
        [Tooltip("The simulation time updates every frame")]
        play,
        [Tooltip("The simulation time doesn't change")]
        paused,
        [Tooltip("The simulation time is updated every frame but counts to 0 and then stops")]
        reversed,
        [Tooltip("Reset the simulation time to 0 and then gets set to playing")]
        reset
    }
}
