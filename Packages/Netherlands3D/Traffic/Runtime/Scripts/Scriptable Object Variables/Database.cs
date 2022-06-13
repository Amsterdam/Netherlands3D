using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Traffic.VISSIM
{
    /// <summary>
    /// Data base containing Traffic data
    /// </summary>
    [CreateAssetMenu(fileName = "VISSIM Database", menuName = "ScriptableObjects/Traffic/VISSIM/Database", order = 1)]
    public class Database : ScriptableObject
    {
        /// <summary>
        /// If the Datas list has reached its max count
        /// </summary>
        public bool DatasReachedMaxCount { get { return maxDatabaseCount > 0 && Value.Count >= maxDatabaseCount; } }
        /// <summary>
        /// The max simulation time in seconds this database has
        /// </summary>
        public float MaxSimulationTime { get; private set; }

        /// <summary>
        /// The max amount of data the database can contain
        /// </summary>
        public int maxDatabaseCount = -1;
        /// <summary>
        /// The data <Data.id, Data>
        /// </summary>
        public Dictionary<int, Data> Value { get; private set; }
        /// <summary>
        /// Called when data is added. Contains the int indexes of new/updated Data values of Value
        /// </summary>
        public UnityEvent<List<int>> OnAddData = new UnityEvent<List<int>>();
        /// <summary>
        /// Called when data is removed. Contains the int indexes of removed data values of Value
        /// </summary>
        public UnityEvent<List<int>> OnRemoveData = new UnityEvent<List<int>>();

        #region new

        /// <summary>
        /// Dictionary containing all signal heads with their number as key
        /// </summary>
        /// <remarks>For adding signalheads use AddSignalHeads function!</remarks>
        public Dictionary<int, SignalHeadData> SignalHeads
        {
            get
            {
                return signalHeads;
            }
            set 
            {
                signalHeads = value;
            }
        }

        /// <summary>
        /// Called when the data is changed
        /// </summary>
        public UnityEvent OnDataChanged;
        /// <summary>
        /// When the signalHeads data is changed
        /// </summary>
        public UnityEvent OnSignalHeadsChanged;

        /// <summary>
        /// All signal heads data
        /// </summary>
        /// <remarks><number, DataATT></remarks>
        private Dictionary<int, SignalHeadData> signalHeads = new Dictionary<int, SignalHeadData>();

        #endregion

        private void OnEnable()
        {
            if(Value == null) Value = new Dictionary<int, Data>();
        }

        /// <summary>
        /// Add Data to the database
        /// </summary>
        /// <param name="data">The Data class to add</param>
        public void AddData(Data data)
        {
            // Check if allowed to add
            if(DatasReachedMaxCount) return;

            Value.Add(data.id, data);
            CheckMaxSimulationTime(data);

            OnAddData?.Invoke(new List<int>() { data.id });
        }

        /// <summary>
        /// Add Data to the database
        /// </summary>
        /// <param name="newDatas">The Data dictionary to add</param>
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
                    CheckMaxSimulationTime(data.Value);
                }
                else
                {
                    // Add new key
                    Value.Add(data.Key, data.Value);
                    CheckMaxSimulationTime(data.Value);
                }
                dataKeysUpdated.Add(data.Key);
            }

            OnAddData?.Invoke(dataKeysUpdated);
        }

        /// <summary>
        /// Add signal heads to the database
        /// </summary>
        public void AddSignalHeads(Dictionary<int, SignalHeadData> signalHeads, bool triggerRefresh = true)
        {
            Debug.Log(string.Format("[VISSIM] Adding {0} signalHeads", signalHeads.Count)); 
            this.signalHeads.AddRange(signalHeads);
            if(triggerRefresh)
            {
                OnDataChanged.Invoke();
                OnSignalHeadsChanged.Invoke();
            }
        }

        /// <summary>
        /// Clears the database of all data
        /// </summary>
        public void Clear()
        {
            OnRemoveData?.Invoke(Value.Keys.ToList());
            Value.Clear();
            SignalHeads.Clear();
            signalHeads.Clear();
            SignalHeads = signalHeads;
            MaxSimulationTime = 0;
            Debug.Log("Cleared Data");
        }

        /// <summary>
        /// Remove a specific instance of data
        /// </summary>
        /// <param name="data">The data to remove</param>
        public void RemoveData(Data data)
        {
            OnRemoveData?.Invoke(new List<int>() { data.id });
            Value.Remove(data.id);
        }

        /// <summary>
        /// Remove specific instances of data
        /// </summary>
        /// <param name="datas">The datas to remove</param>
        public void RemoveData(Dictionary<int, Data> datas)
        {
            OnRemoveData?.Invoke(datas.Keys.ToList());
            foreach(var item in datas)
            {
                Value.Remove(item.Key);
            }
        }

        /// <summary>
        /// Check if the data contains a higher simulation time then the max and if so set the new max
        /// </summary>
        /// <param name="data"></param>
        private void CheckMaxSimulationTime(Data data)
        {
            if(data.coordinates.Last().Key > MaxSimulationTime) MaxSimulationTime = data.coordinates.Last().Key;
        }
    }
}
