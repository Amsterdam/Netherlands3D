using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netherlands3D.TileSystem;

namespace Netherlands3D.VISSIM
{
    /// <summary>
    /// Visualizes the VISSIM data
    /// </summary>
    public class Visualizer
    {
        /// <summary>
        /// Static raycasthit used by entities
        /// </summary>
        public static RaycastHit Hit;

        /// <summary>
        /// Dictionary containing all entites <Data.id, Entity>
        /// </summary>
        public Dictionary<int, Entity> entities = new Dictionary<int, Entity>();
        /// <summary>
        /// A default cube gameobject for entities that have no gameobjects assigned
        /// </summary>
        private GameObject defaultEntityPrefab;

        /// <summary>
        /// Constructor
        /// </summary>
        public Visualizer()
        {
            defaultEntityPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            defaultEntityPrefab.AddComponent<Entity>();
            defaultEntityPrefab.name = "Default Entity";
            defaultEntityPrefab.transform.SetParent(VISSIMManager.VisualizerParentTransform);
            VISSIMManager.OnAddData += UpdateEntities;
        }

        /// <summary>
        /// Deconstructor
        /// </summary>
        ~Visualizer()
        {
            VISSIMManager.OnAddData -= UpdateEntities;
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
                newData = VISSIMManager.Datas;
            }
            else
            {
                // Get updated data
                foreach(var item in dataKeysUpdated)
                {
                    newData.Add(item, VISSIMManager.Datas[item]);
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
                    if(!VISSIMManager.AvailableEntitiesData.ContainsKey(data.Value.vehicleTypeIndex) || VISSIMManager.AvailableEntitiesData[data.Value.vehicleTypeIndex].Length == 0) //TODO if no available entites give different error msg
                    {
                        // No gameobjects to choose from
                        prefab = defaultEntityPrefab;
                        Debug.LogWarning("[VISSIM] Entity has no gameobjects to choose from! Make sure that you assign a prefab in the entity Scriptable Object");
                    }
                    else
                    {
                        // Choose random prefab
                        prefab = VISSIMManager.AvailableEntitiesData[data.Value.vehicleTypeIndex][Random.Range(0, VISSIMManager.AvailableEntitiesData[data.Value.vehicleTypeIndex].Length)];
                    }
                    
                    // Create entity
                    Entity entity = Object.Instantiate(prefab, VISSIMManager.VisualizerParentTransform).GetComponent<Entity>();
                    entities.Add(data.Key, entity);
                    entity.Initialize(data.Value);
                }
            }

            if(VISSIMManager.ShowDebugLog) Debug.Log(string.Format("[VISSIM] Visualizer updated {0} entitie(s)", newData.Count));
        }
    }
}
