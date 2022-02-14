using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;
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
        /// The file location of the VISSIM file
        /// </summary>
        public static string FileLocation { get { return Instance.fileLocation; } set { Instance.fileLocation = value; } }
        /// <summary>
        /// The template for VISSIM
        /// </summary>
        public static string RequiredTemplate { get { return "$VEHICLE:SIMSEC;NO;VEHTYPE;COORDFRONT;COORDREAR;WIDTH"; } }
        /// <summary>
        /// All VISSIM data
        /// </summary>
        public static ref List<Data> Datas { get { return ref Instance.datas; } }
        /// <summary>
        /// A list containing all entities that do not have a corresponding id from entitiesDatas
        /// </summary>
        public static ref List<int> MissingEntityIDs { get { return ref Instance.missingEntityIDs; } } //TODO is ref needed here?

        private static VISSIMManager instance;

        /// <summary>
        /// A list containing all entities that do not have a corresponding id from entitiesDatas
        /// </summary>
        [HideInInspector] public List<int> missingEntityIDs = new List<int>(); //missingVissimTypes
        [HideInInspector] public Dictionary<int, GameObject[]> availableEntitiesData = new Dictionary<int, GameObject[]>(); //vehicleTypes
        /// <summary>
        /// All VISSIM Data
        /// </summary>
        [HideInInspector] public List<Data> datas = new List<Data>(); //allVissimData
        [HideInInspector] public Dictionary<int, List<Data>> allVissimDataByVehicleID = new Dictionary<int, List<Data>>();//?? "Vehicle Sorting test, see SortDataByCar() function"


        [Header("Values")]
        [Tooltip("The file location of the VISSIM file")]
        [SerializeField] private string fileLocation = "921929autoluw2030ref005.fzp";

        [Header("Entity Data")]
        [Tooltip("List containing every available entity data (Scriptable Objects)")]
        public List<EntityData> entitiesDatas = new List<EntityData>();

        [Header("Components")]
        [Tooltip("Event that fires when files are imported")]
        [SerializeField] private StringEvent eventFilesImported;
        [Tooltip("Event that fires when the database needs to be cleared")]
        [SerializeField] private BoolEvent eventClearDatabase;

        /// <summary>
        /// For handling the string events
        /// </summary>
        private StringLoader stringLoader;

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
            stringLoader = new StringLoader(eventFilesImported, eventClearDatabase);
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
        public static void AddData(string dataString)
        {
            string[] array = dataString.Split(';');
            float simulationSeconds = float.Parse(array[0], CultureInfo.InvariantCulture);
            int vehicleTypeIndex = int.Parse(array[2]);
            // Check if ID isn't set, then store it in missingEntityIDs
            if(!Instance.availableEntitiesData.ContainsKey(vehicleTypeIndex) && !Instance.missingEntityIDs.Contains(vehicleTypeIndex)) Instance.missingEntityIDs.Add(vehicleTypeIndex);

            Instance.datas.Add(new Data(simulationSeconds, int.Parse(array[1]), vehicleTypeIndex, ConverterFZP.StringToVector3(array[3]), ConverterFZP.StringToVector3(array[4]), float.Parse(array[5])));
        }

        /// <summary>
        /// Clears all VISSIM data
        /// </summary>
        public static void Clear()
        {

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
                Instance.availableEntitiesData.Add(item.id, item.gameObjects);
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
    }
}
