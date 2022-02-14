using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.Events;

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

        private static VISSIMManager instance;

        [HideInInspector] public List<int> missingVissimTypes = new List<int>();
        [HideInInspector] public Dictionary<int, GameObject[]> vehicleTypes = new Dictionary<int, GameObject[]>();
        [HideInInspector] public List<Data> allVissimData = new List<Data>();
        [HideInInspector] public Dictionary<int, List<Data>> allVissimDataByVehicleID = new Dictionary<int, List<Data>>();


        [Header("Values")]
        [Tooltip("The file location of the VISSIM file")]
        [SerializeField] private string fileLocation = "921929autoluw2030ref005.fzp";

        [Header("Entity Data")]
        [Tooltip("List containing every available entity data (Scriptable Objects)")]
        public List<EntityData> entityDatas = new List<EntityData>();

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
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        /// <summary>
        /// Clears all VISSIM data
        /// </summary>
        public static void Clear()
        {

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
