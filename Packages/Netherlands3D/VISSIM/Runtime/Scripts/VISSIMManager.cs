using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
using Netherlands3D.TileSystem;
using System.Globalization;

/* This namespace is maintained by: https://github.com/MrSliddes */
namespace Netherlands3D.VISSIM
{
    /// <summary>
    /// The main script that handles everything connected to VISSIM
    /// </summary>
    public class VISSIMManager : MonoBehaviour
    {
        public static VISSIMManager Instance { get { return instance; } private set { } }
        /// <summary>
        /// If the Datas list has reached its max count
        /// </summary>
        public static bool DatasReachedMaxCount { get { return MaxDatasCount > 0 && Datas.Count >= MaxDatasCount; } }
        /// <summary>
        /// Show the Debug.Log() messages from VISSIM
        /// </summary>
        public static bool ShowDebugLog { get { return Instance.showDebugLog; } set { Instance.showDebugLog = value; } }
        /// <summary>
        /// When selecting an entity its data coordinates are drawn with gizmos
        /// </summary>
        public static bool VisualizeGizmosDataPoints { get { return Instance.visualizeGizmosDataPoints; } set { Instance.visualizeGizmosDataPoints = value; } }
        /// <summary>
        /// The max limit the list Datas can be
        /// </summary>
        /// <remarks>-1 = max</remarks>
        public static int MaxDatasCount = -1;
        /// <summary>
        /// The current VISSIM simulation time starting from 0 - infinity
        /// </summary>
        public static float SimulationTime
        {
            get { return Instance.simulationTime; }
            set
            {
                if(value < 0) value = 0;
                Instance.simulationTime = value;
                Instance.previousSimulationTime = value;
                OnSimulationTimeChanged?.Invoke(value);
            }
        }
        /// <summary>
        /// The state of the simulation time and how it gets updated
        /// </summary>
        public static SimulationState SimulationState 
        { 
            get { return Instance.simulationState; }
            set
            {
                Instance.simulationState = value;
                Instance.previousSimulationState = value;
                OnSimulationStateChanged?.Invoke(value);
            }
        }
        /// <summary>
        /// The parent transform of the visualizer script
        /// </summary>
        public static Transform VisualizerParentTransform { get { return Instance.visualizerParentTransform; } }
        /// <summary>
        /// The binary mesh layer of the tile system
        /// </summary>
        public static BinaryMeshLayer BinaryMeshLayer { get { return Instance.binaryMeshLayer; } }
        /// <summary>
        /// Callback that gets called when Data is added to Datas
        /// </summary>
        public static DelegateAddData OnAddData;
        /// <summary>
        /// Callback when the simulation time is changed
        /// </summary>
        public static DelegateSimulationTimeChanged OnSimulationTimeChanged;
        /// <summary>
        /// Callback when the simulation state is changed
        /// </summary>
        public static DelegateSimulationStateChanged OnSimulationStateChanged;
        /// <summary>
        /// The Visualizer script that visualizes the VISSIM data
        /// </summary>
        public static Visualizer Visualizer { get { return Instance.visualizer; } }
        /// <summary>
        /// All VISSIM data
        /// </summary>
        public static Dictionary<int, Data> Datas { get { return Instance.datas; } private set { } }
        /// <summary>
        /// A list containing all entities that do not have a corresponding id from entitiesDatas for debugging purposes
        /// </summary>
        public static ref List<int> MissingEntityIDs { get { return ref Instance.missingEntityIDs; } } //TODO is ref needed here?
        /// <summary>
        /// Contains all available enities ID's (cars/busses/bikes etc.) with corresponding prefab to spawn
        /// </summary>
        public static ref Dictionary<int, GameObject> AvailableEntitiesData { get { return ref Instance.availableEntitiesData; } }

        private static VISSIMManager instance;

        /// <summary>
        /// All VISSIM Data, <ID, Data>
        /// </summary>
        public Dictionary<int, Data> datas = new Dictionary<int, Data>();
        /// <summary>
        /// A list containing all entities that do not have a corresponding id from entitiesDatas for debugging purposes
        /// </summary>
        private List<int> missingEntityIDs = new List<int>();
        /// <summary>
        /// Contains all available enities ID's (cars/busses/bikes etc.) with corresponding prefab to spawn
        /// </summary>
        public Dictionary<int, GameObject> availableEntitiesData = new Dictionary<int, GameObject>();

        [Header("Values")]
        [Tooltip("Show the Debug.Log() messages from VISSIM")]
        [SerializeField] private bool showDebugLog = true;
        [Tooltip("Visualize the VISSIM Data")]
        [SerializeField] private bool visualizeData = true;
        [Tooltip("When selecting an entity its data coordinates are drawn with gizmos")]
        [SerializeField] private bool visualizeGizmosDataPoints = true;
        [Tooltip("The state of the simulation time and how it gets updated")]
        [SerializeField] private SimulationState simulationState;
        [Tooltip("The current VISSIM simulation time starting from 0 - infinity")]
        [SerializeField] private float simulationTime = 0;

        [Header("Entity Data")]
        [Tooltip("List containing every available entity data (Scriptable Objects)")]
        public List<EntityData> entitiesDatas = new List<EntityData>();

        [Header("Components")]
        [Tooltip("Event that fires when files are imported")]
        [SerializeField] private StringEvent eventFilesImported;
        [Tooltip("Event that fires when the database needs to be cleared")]
        [SerializeField] private BoolEvent eventClearDatabase;
        [Tooltip("The binary mesh layer of the tile system")]
        [SerializeField] private BinaryMeshLayer binaryMeshLayer;

        /// <summary>
        /// Delegate that gets called when data is added to Datas
        /// </summary>
        /// <remarks>
        /// newDataKeys contains the keys of 'Datas' that have been added/updated
        /// </remarks>
        public delegate void DelegateAddData(List<int> newDataKeys);
        /// <summary>
        /// Delegate that gets called when the simulation time is changed
        /// </summary>
        public delegate void DelegateSimulationTimeChanged(float newTime);
        /// <summary>
        /// Delegate that gets called if the Simulation State is changed
        /// </summary>
        /// <param name="newState"></param>
        public delegate void DelegateSimulationStateChanged(SimulationState newState);

        /// <summary>
        /// Keeps track of data had already been added or not, if not reset simulationTime
        /// </summary>
        private bool firstDataAdded = true;
        /// <summary>
        /// Keeps track of the previous simulation time incase it gets changed via inspector by user
        /// </summary>
        private float previousSimulationTime;
        /// <summary>
        /// Keeps track of the previous simulation State incase it gets changed via inspector
        /// </summary>
        private SimulationState previousSimulationState;
        /// <summary>
        /// The parent transform of the visualizer script
        /// </summary>
        private Transform visualizerParentTransform;
        /// <summary>
        /// The VISSIM visualizer to show cars etc.
        /// </summary>
        private Visualizer visualizer;

        private void OnEnable()
        {
            OnAddData += OnAddDataCallback;
            OnSimulationTimeChanged += SimulationTimeChanged;
            eventFilesImported.started.AddListener(FileLoader.Load);
        }

        private void OnDisable()
        {
            OnAddData -= OnAddDataCallback;
            OnSimulationTimeChanged -= SimulationTimeChanged;
            eventFilesImported.started.RemoveListener(FileLoader.Load);
        }

        private void Awake()
        {
            // Set instance
            if(instance != null)
            {
                Debug.LogError("[VISSIM] An instance of VISSIMManager is already created! Make sure that you only have 1 instance of VISSIMManager.");
                Destroy(gameObject);
                return;
            }
            instance = this;

            // Set
            visualizerParentTransform = new GameObject("Visualizer Parent").GetComponent<Transform>();
            visualizerParentTransform.SetParent(transform);
            visualizer = new Visualizer();
            SimulationState = SimulationState.paused;
        }

        // Start is called before the first frame update
        void Start()
        {
            LoadDefaultData();
        }

        private void Update()
        {
            UpdateSimulationTime();
        }

        /// <summary>
        /// Add VISSIM data to VISSIMManager.datas
        /// </summary>
        /// <param name="data">The Data class to add</param>
        public static void AddData(Data data)
        {
            // Check if allowed to add
            if(DatasReachedMaxCount) return;

            Datas.Add(data.id, data);

            OnAddData?.Invoke(new List<int>() { data.id });
        }

        /// <summary>
        /// Add VISSIM data to VISSIMManager.datas
        /// </summary>
        /// <param name="datas">The Data list to add</param>
        public static void AddData(Dictionary<int, Data> newDatas)
        {
            // Keep track of what datas keys have been added/updated
            List<int> dataKeysUpdated = new List<int>();

            foreach(var data in newDatas)
            {
                if(DatasReachedMaxCount) break;

                // Check if key is already present
                if(Datas.ContainsKey(data.Key))
                {
                    // Key already present, update it
                    Datas[data.Key].AddCoordinates(data.Value.coordinates);
                }
                else
                {
                    // Add new key
                    Datas.Add(data.Key, data.Value);
                }
                dataKeysUpdated.Add(data.Key);
            }

            OnAddData?.Invoke(dataKeysUpdated);
        }

        /// <summary>
        /// Clears all VISSIM data
        /// </summary>
        public static void Clear()
        {

        }

        /// <summary>
        /// Invoke the string event Files Imported
        /// </summary>
        /// <param name="file">The file contents to import</param>
        public static void InvokeEventFilesImported(string file)
        {
            if(Instance.eventFilesImported == null || Instance.eventFilesImported.started == null)
            {
                Debug.LogError("[VISSIM] Event files imported not started!");
                return;
            }

            Instance.eventFilesImported.started.Invoke(file);
        }

        /// <summary>
        /// Load the default VISSIM data
        /// </summary>
        public static void LoadDefaultData()
        {
            Instance.missingEntityIDs.Clear();
            Instance.availableEntitiesData.Clear();

            foreach(var item in Instance.entitiesDatas)
            {
                if(Instance.availableEntitiesData.ContainsKey(item.id))
                {
                    Debug.LogError(string.Format("[VISSIM] VISSIM Entity with ID {0} has already been added. Check your entity data for duplicates with same ID", item.id));
                    continue;
                }

                Instance.availableEntitiesData.Add(item.id, item.prefabEntity);
            }
        }

        /// <summary>
        /// Check if the entity type index is available in availableEntitiesData
        /// </summary>
        /// <param name="index"></param>
        public static void CheckEntityTypeIndex(int index)
        {
            if(!Instance.availableEntitiesData.ContainsKey(index) && !Instance.missingEntityIDs.Contains(index))
            {
                Debug.LogWarning(string.Format("[VISSIM] Missing entity id {0}", index));
                Instance.missingEntityIDs.Add(index);
            }
        }

        /// <summary>
        /// Clears all VISSIM data
        /// </summary>
        /// <remarks>Used for Unity button events as static voids cannot be used for that. Invokes Clear()</remarks>
        /// <see cref="Clear()"/>
        public void Clear4Button()
        {
            Clear();
        }

        /// <summary>
        /// Called when new data has been added to datas
        /// </summary>
        /// <param name="newDataKeys">The keys that have been updated from Datas</param>
        private void OnAddDataCallback(List<int> newDataKeys)
        {
            if(ShowDebugLog) Debug.Log(string.Format("[VISSIM] Added {0} data(s)", newDataKeys.Count));
            if(firstDataAdded)
            {
                firstDataAdded = false;
                SimulationState = SimulationState.play;
            }
        }

        /// <summary>
        /// Update the simulation time value
        /// </summary>
        private void UpdateSimulationTime()
        {
            switch(simulationState)
            {
                case SimulationState.play:
                    simulationTime += Time.deltaTime;
                    previousSimulationTime = simulationTime;
                    break;
                case SimulationState.paused:
                    break;
                case SimulationState.reversed:
                    simulationTime -= Time.deltaTime;
                    if(simulationTime < 0) simulationTime = 0;
                    previousSimulationTime = simulationTime;
                    break;
                case SimulationState.reset:
                    simulationTime = 0;
                    previousSimulationTime = 0;
                    simulationState = SimulationState.paused;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Callback when the simulation time gets changed
        /// </summary>
        /// <param name="newTime"></param>
        private void SimulationTimeChanged(float newTime)
        {
            if(showDebugLog) Debug.Log("[VISSIM] SimulationTime changed to " + newTime);
        }

        private void OnValidate()
        {
            // Check if user has changed simulation time in inspector
            if(simulationTime != previousSimulationTime)
            {
                SimulationTime = simulationTime;
            }

            // Check if user has changed simulation state in inspector
            if(simulationState != previousSimulationState)
            {
                SimulationState = simulationState;
            }
        }

    }
}
