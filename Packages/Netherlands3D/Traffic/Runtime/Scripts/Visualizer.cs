using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.TileSystem;

namespace Netherlands3D.Traffic
{
    /// <summary>
    /// Visualizer for Traffic
    /// </summary>
    [AddComponentMenu("Traffic/Traffic Visualizer")]
    public class Visualizer : MonoBehaviour
    {
        /// <summary>
        /// Static raycasthit used by entities
        /// </summary>
        public static RaycastHit Hit;

        [Header("Entity Data")]
        [Tooltip("List containing every available entity data (Scriptable Objects)")]
        public List<EntityData> entitiesDatas = new List<EntityData>();

        /// <summary>
        /// Dictionary containing all entites <Data.id, Entity>
        /// </summary>
        public Dictionary<int, Entity> entities = new Dictionary<int, Entity>();
        /// <summary>
        /// A default cube gameobject for entities that have no gameobjects assigned
        /// </summary>
        private GameObject defaultEntityPrefab;

        public bool showDebugLog = true;
        public DataDatabase datas;
        /// <summary>
        /// A list containing all entities that do not have a corresponding id from entitiesDatas for debugging purposes
        /// </summary>
        private List<int> missingEntityIDs = new List<int>(); //TODO to so
        /// <summary>
        /// Contains all available enities ID's (cars/busses/bikes etc.) with corresponding prefab to spawn
        /// </summary>
        public Dictionary<int, GameObject> availableEntitiesData = new Dictionary<int, GameObject>(); //TODO to so


        private void OnEnable()
        {
            datas.OnAddData.AddListener(UpdateEntities);
        }

        private void OnDisable()
        {
            datas.OnAddData.RemoveListener(UpdateEntities);
        }

        private void Start()
        {
            LoadDefaultData();
            defaultEntityPrefab = Resources.Load<GameObject>("VISSIM Entity Default");
        }

        /// <summary>
        /// Constructor
        /// </summary>
        //public Visualizer()
        //{
        //    defaultEntityPrefab = Resources.Load<GameObject>("VISSIM Entity Default");

        //    //VISSIMManager.OnAddData += UpdateEntities; TODO
        //}

        /// <summary>
        /// Deconstructor
        /// </summary>
        //~Visualizer()
        //{
        //    //VISSIMManager.OnAddData -= UpdateEntities;
        //}

        /// <summary>
        /// Updates the entities dictionary with all data from VISSIMManager.Datas
        /// </summary>
        /// <param name="newData">Insert list of Data if you only want this list data to be updated</param>
        public void UpdateEntities(List<int> dataKeysUpdated = null)
        {
            // Setup Dictionary
            Dictionary<int, Data> newData = new Dictionary<int, Data>();

            // Check to update only partial or from entire datas list
            if(dataKeysUpdated == null)
            {
                // Update from entire VISSIMManager.Datas
                newData = datas.Value;
            }
            else
            {
                // Get updated data
                foreach(var item in dataKeysUpdated)
                {
                    newData.Add(item, datas.Value[item]);
                }
            }

            // Updating the entities
            GameObject prefab;
            foreach(var data in newData)
            {
                // Check if data already has an entity connected to it
                if(entities.ContainsKey(data.Key))
                {
                    // Already created, update data
                    entities[data.Key].UpdateData(data.Value);
                }
                else
                {
                    // Entity prefab
                    if(availableEntitiesData.ContainsKey(data.Value.entityTypeIndex) || availableEntitiesData[data.Value.entityTypeIndex] == null) //TODO if no available entites give different error msg
                    {
                        // No gameobjects to choose from
                        prefab = defaultEntityPrefab;
                        Debug.LogWarning("[VISSIM] Entity has no prefabEntity assigned! Make sure that you assign a prefab in the entity Scriptable Object");
                    }
                    else
                    {
                        // Choose random prefab
                        prefab = availableEntitiesData[data.Value.entityTypeIndex];
                    }
                    
                    // Create entity
                    Entity entity = Object.Instantiate(prefab, transform).GetComponent<Entity>();
                    entities.Add(data.Key, entity);
                    entity.Initialize(data.Value);
                }
            }

            if(showDebugLog) Debug.Log(string.Format("[VISSIM] Visualizer updated {0} entities", newData.Count));
        }

        /// <summary>
        /// Load the default VISSIM data
        /// </summary>
        private void LoadDefaultData()
        {
            missingEntityIDs.Clear();
            availableEntitiesData.Clear();

            foreach(var item in entitiesDatas)
            {
                if(availableEntitiesData.ContainsKey(item.id))
                {
                    Debug.LogError(string.Format("[VISSIM] VISSIM Entity with ID {0} has already been added. Check your entity data for duplicates with same ID", item.id));
                    continue;
                }

                availableEntitiesData.Add(item.id, item.prefabEntity);
            }
        }
    }
}
