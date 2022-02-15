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
        /// Dictionary containing all entites <Data.id, Entity>
        /// </summary>
        public Dictionary<int, Entity> entities = new Dictionary<int, Entity>();

        /// <summary>
        /// Updates the entities dictionary with all data from VISSIMManager.Datas
        /// </summary>
        /// <param name="newData">Insert list of Data if you only want this list data to be updated</param>
        public void UpdateEntities(List<Data> newData = null)
        {
            // Check to update only partial or from entire datas list
            if(newData == null)
            {
                // Update from entire VISSIMManager.Datas
                newData = VISSIMManager.Datas;
            }

            foreach(Data data in newData)
            {
                // Check if data already has an entity connected to it
                if(entities.ContainsKey(data.id))
                {
                    // Already created, update data
                    entities[data.id].data = data;
                }
                else
                {
                    // Create entity
                    Entity entity = Object.Instantiate(
                        VISSIMManager.AvailableEntitiesData[data.vehicleTypeIndex][Random.Range(0, VISSIMManager.AvailableEntitiesData[data.vehicleTypeIndex].Length)], 
                        VISSIMManager.VisualizerParentTransform).GetComponent<Entity>();
                    entities.Add(data.id, entity);
                    entity.Initialize(data);
                }
            }
        }
    }
}
