using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Traffic
{
    [CreateAssetMenu(fileName = "Traffic Data Database", menuName = "ScriptableObjects/Traffic/Data Database", order = 1)]
    public class DataDatabase : ScriptableObject
    {
        /// <summary>
        /// If the Datas list has reached its max count
        /// </summary>
        public bool DatasReachedMaxCount { get { return maxDatabaseCount > 0 && Value.Count >= maxDatabaseCount; } }

        public UnityEvent<List<int>> OnAddData = new UnityEvent<List<int>>();

        public int maxDatabaseCount = -1;

        public Dictionary<int, Data> Value;

        /// <summary>
        /// Add VISSIM data to VISSIMManager.datas
        /// </summary>
        /// <param name="data">The Data class to add</param>
        public void AddData(Data data)
        {
            // Check if allowed to add
            if(DatasReachedMaxCount) return;

            Value.Add(data.id, data);

            OnAddData?.Invoke(new List<int>() { data.id });
        }

        /// <summary>
        /// Add VISSIM data to VISSIMManager.datas
        /// </summary>
        /// <param name="datas">The Data list to add</param>
        public void AddData(Dictionary<int, Data> newDatas)
        {
            // Keep track of what datas keys have been added/updated
            List<int> dataKeysUpdated = new List<int>();

            foreach(var data in newDatas)
            {
                if(DatasReachedMaxCount) break;

                // Check if key is already present
                if(Value.ContainsKey(data.Key))
                {
                    // Key already present, update it
                    Value[data.Key].AddCoordinates(data.Value.coordinates);
                }
                else
                {
                    // Add new key
                    Value.Add(data.Key, data.Value);
                }
                dataKeysUpdated.Add(data.Key);
            }

            OnAddData?.Invoke(dataKeysUpdated);
        }
    }
}
