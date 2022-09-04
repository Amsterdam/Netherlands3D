using System.Collections.Generic;
using System;
namespace Netherlands3D.ModelParsing
{
    [Serializable]
    public class GameObjectDataSet
    {
        public string name;
        public List<MaterialData> materials = new List<MaterialData>();
        public List<GameObjectData> gameObjects = new List<GameObjectData>();
        public void Clear()
        {
            foreach (GameObjectData item in gameObjects)
            {
                item.Clear();
            }
            gameObjects.Clear();
            foreach (MaterialData item in materials)
            {
                item.Clear();
            }
            materials.Clear();
        }
    }

}