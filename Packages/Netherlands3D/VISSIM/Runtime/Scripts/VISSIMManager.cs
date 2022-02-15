using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
using Netherlands3D.TileSystem;
using System.Globalization;

namespace Netherlands3D.VISSIM
{
    /// <summary>
    /// The main script that handles everything connected to VISSIM
    /// </summary>
    public class VISSIMManager : MonoBehaviour
    {
        public static VISSIMManager Instance { get { return instance; } private set { } }
        /// <summary>
        /// Show the Debug.Log() messages from VISSIM
        /// </summary>
        public static bool ShowDebugLog { get { return Instance.showDebugLog; } set { Instance.showDebugLog = value; } }
        /// <summary>
        /// If the Datas list has reached its max count
        /// </summary>
        public static bool DatasReachedMaxCount { get { return MaxDatasCount > 0 && Datas.Count >= MaxDatasCount; } }
        /// <summary>
        /// The max limit the list Datas can be
        /// </summary>
        /// <remarks>-1 = max</remarks>
        public static int MaxDatasCount = -1;
        /// <summary>
        /// The template for VISSIM
        /// </summary>
        public static string RequiredTemplate { get { return "$VEHICLE:SIMSEC;NO;VEHTYPE;COORDFRONT;COORDREAR;WIDTH"; } }

        public static Transform VisualizerParentTransform { get { return Instance.visualizerParentTransform; } }
        /// <summary>
        /// The binary mesh layer of the tile system
        /// </summary>
        public static BinaryMeshLayer BinaryMeshLayer { get { return Instance.binaryMeshLayer; } }
        /// <summary>
        /// All VISSIM data
        /// </summary>
        public static ref List<Data> Datas { get { return ref Instance.datas; } }
        /// <summary>
        /// A list containing all entities that do not have a corresponding id from entitiesDatas
        /// </summary>
        public static ref List<int> MissingEntityIDs { get { return ref Instance.missingEntityIDs; } } //TODO is ref needed here?
        /// <summary>
        /// A list containing all entities that do not have a corresponding id from entitiesDatas
        /// </summary>
        public static ref Dictionary<int, GameObject[]> AvailableEntitiesData { get { return ref Instance.availableEntitiesData; } }

        private static VISSIMManager instance;

        [Header("VISSIM Data")]
        /// <summary>
        /// All VISSIM Data
        /// </summary>
        public List<Data> datas = new List<Data>(); //allVissimData
        /// <summary>
        /// A list containing all entities that do not have a corresponding id from entitiesDatas
        /// </summary>
        public List<int> missingEntityIDs = new List<int>(); //missingVissimTypes
        public Dictionary<int, GameObject[]> availableEntitiesData = new Dictionary<int, GameObject[]>(); //vehicleTypes
        public Dictionary<int, List<Data>> allVissimDataByVehicleID = new Dictionary<int, List<Data>>();//?? "Vehicle Sorting test, see SortDataByCar() function"

        [Header("Values")]
        [Tooltip("Show the Debug.Log() messages from VISSIM")]
        [SerializeField] private bool showDebugLog = true;
        [Tooltip("The binary mesh layer of the tile system")]
        [SerializeField] private BinaryMeshLayer binaryMeshLayer;

        [Header("Entity Data")]
        [Tooltip("List containing every available entity data (Scriptable Objects)")]
        public List<EntityData> entitiesDatas = new List<EntityData>();

        [Header("Components")]
        [Tooltip("Event that fires when files are imported")]
        [SerializeField] private StringEvent eventFilesImported;
        [Tooltip("Event that fires when the database needs to be cleared")]
        [SerializeField] private BoolEvent eventClearDatabase;

        /// <summary>
        /// The parent transform of the visulizer script
        /// </summary>
        private Transform visualizerParentTransform;
        /// <summary>
        /// For handling the string events
        /// </summary>
        private StringLoader stringLoader;
        /// <summary>
        /// The VISSIM visualizer to show cars etc.
        /// </summary>
        private Visualizer visualizer;

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
            visualizerParentTransform = new GameObject("Visulizer Parent").GetComponent<Transform>();
            visualizerParentTransform.SetParent(transform);
            stringLoader = new StringLoader(eventFilesImported, eventClearDatabase);
            visualizer = new Visualizer();
        }

        // Start is called before the first frame update
        void Start()
        {
            LoadDefaultData();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        /// <summary>
        /// Add the VISSIM data to VISSIMManager.datas
        /// </summary>
        /// <param name="dataString">A line in the format of RequiredTemplate</param>
        public static void AddData(string dataString) //TODO this is only for .FZP data, needs to be reworked if other data types are added
        {
            // Check if allowed to add
            if(DatasReachedMaxCount) return;

            string[] array = dataString.Split(';');
            float simulationSeconds = float.Parse(array[0], CultureInfo.InvariantCulture);
            int vehicleTypeIndex = int.Parse(array[2]);
            // Check if ID isn't set, then store it in missingEntityIDs
            if(!Instance.availableEntitiesData.ContainsKey(vehicleTypeIndex) && !Instance.missingEntityIDs.Contains(vehicleTypeIndex)) Instance.missingEntityIDs.Add(vehicleTypeIndex);

            Instance.datas.Add(new Data(simulationSeconds, int.Parse(array[1]), vehicleTypeIndex, ConverterFZP.StringToVector3(array[3]), ConverterFZP.StringToVector3(array[4]), float.Parse(array[5]))); //TODO error handling if parsing doesnt work
            if(ShowDebugLog) Debug.Log("[VISSIM] Added data");
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

                Instance.availableEntitiesData.Add(item.id, item.gameObjects);
            }
        }

        /// <summary>
        /// Loads a file into VISSIM
        /// </summary>
        public static void LoadFile(string file)
        {
            Instance.stringLoader.LoadFile(file);
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
    }
}
