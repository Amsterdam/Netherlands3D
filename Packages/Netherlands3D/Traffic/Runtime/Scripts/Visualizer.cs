using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.TileSystem;
using System.Linq;

namespace Netherlands3D.Traffic
{
    /// <summary>
    /// Visualizer for Traffic
    /// </summary>
    [AddComponentMenu("Netherlands3D/Traffic/Traffic Visualizer")]
    public class Visualizer : MonoBehaviour
    {
        [Header("Options")]
        [Tooltip("Show debug.log messages from this class")]
        public bool showDebugLog = true;
        [Tooltip("Should the Scriptable Objects Variables be reset on start to default?")]
        [SerializeField] private bool resetSOVOnStart = true;

        [Header("Layer Masks")]
        [Tooltip("The layer mask used for collision detection for traffic entities")]
        [SerializeField] private LayerMask layerMask;
        [Tooltip("For raycasting purposes. Does not need to be assigned")]
        [SerializeField] private BinaryMeshLayer binaryMeshLayer;

        [Header("Scriptable Objects")]
        [Tooltip("The database for Data")]
        public DataDatabase datas;
        [Tooltip("List containing every available entity data (Scriptable Objects)")]
        public List<EntityData> entitiesDatas = new List<EntityData>();
        [Tooltip("The scriptable objects for an entity")]
        public EntityScriptableObjects entitySO;

        /// <summary>
        /// Dictionary containing all entites <Data.id, Entity>
        /// </summary>
        public Dictionary<int, Entity> entities = new Dictionary<int, Entity>();
        /// <summary>
        /// A default cube gameobject for entities that have no gameobjects assigned
        /// </summary>
        private GameObject defaultEntityPrefab;

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
            entitySO.eventSimulationStateChanged.started.AddListener(OnSimulationStateChanged);
        }

        private void OnDisable()
        {
            datas.OnAddData.RemoveListener(UpdateEntities);
            entitySO.eventSimulationStateChanged.started.RemoveListener(OnSimulationStateChanged);
        }

        private void Start()
        {
            LoadDefaultData();
            defaultEntityPrefab = Resources.Load<GameObject>("Traffic Entity Default");

            if(resetSOVOnStart)
            {
                entitySO.simulationTime.value = 0;
                entitySO.simulationState.value = 1;
                entitySO.simulationSpeed.value = 1;
            }
        }

        private void Update()
        {
            UpdateSimulationTime();
        }

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
                    if(!availableEntitiesData.ContainsKey(data.Value.entityTypeIndex) || availableEntitiesData[data.Value.entityTypeIndex] == null) //TODO if no available entites give different error msg
                    {
                        // No gameobjects to choose from
                        prefab = defaultEntityPrefab;                        
                        Debug.LogWarning("[Traffic] Entity has no prefabEntity assigned! Make sure that you assign a prefab in the entity Scriptable Object");
                    }
                    else
                    {
                        // Get entity prefab
                        prefab = availableEntitiesData[data.Value.entityTypeIndex];
                    }

                    // Set entity height
                    EntityData ed = entitiesDatas.Single(x => x.id == data.Value.entityTypeIndex);
                    if(ed != null) data.Value.size.y = ed.averageHeight;

                    // Create entity
                    Entity entity = Object.Instantiate(prefab, transform).GetComponent<Entity>();
                    entities.Add(data.Key, entity);
                    entity.Initialize(data.Value, entitySO, layerMask, binaryMeshLayer);
                }
            }

            if(showDebugLog) Debug.Log(string.Format("[Traffic] Visualizer updated {0} entities", newData.Count));
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

        /// <summary>
        /// Update the simulation time value
        /// </summary>
        private void UpdateSimulationTime()
        {
            if(datas.Value == null || datas.Value.Count < 1) return;

            switch(entitySO.simulationState.Value)
            {
                case -1: // Reversed
                    if(entitySO.simulationTime.Value > 0)
                    {
                        // Note that it is updating the value and not Value, we dont need a callback here every frame that it updates the value
                        entitySO.simulationTime.value -= Time.deltaTime * entitySO.simulationSpeed.Value;
                        if(entitySO.simulationTime.Value < 0) entitySO.simulationTime.Value = 0;
                    }
                    break;
                case 0: // Paused
                    break;
                case 1: // Play
                    entitySO.simulationTime.value += Time.deltaTime * entitySO.simulationSpeed.Value;
                    break;
                case -2: // Reset
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Callback when the VISSIM.SimulationState gets changed
        /// </summary>
        /// <param name="newState"></param>
        protected virtual void OnSimulationStateChanged(int newState)
        {
            switch(newState)
            {
                case 1: // Play
                    break;
                case 0: // Paused
                    break;
                case -1: // Reversed
                    break;
                case -2: // Reset
                    entitySO.simulationTime.Value = 0;
                    break;
                default:
                    break;
            }
        }
    }
}
