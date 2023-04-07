using System.Collections.Generic;
using CoreLib.EntityTag;
using UnityEngine;

namespace SaveLoad.CoreSave
{
    [System.Serializable]
    public class EntitiesSaveData
    {
        public EntitiesSaveData(List<Entity> entities)
        {
            
        }
        [System.Serializable]
        public class EntitySaveData
        {
            public string Name;
            public string PrefabName;
            public Vector3 Position;
            public Quaternion Rotation;
        } 
    }
}