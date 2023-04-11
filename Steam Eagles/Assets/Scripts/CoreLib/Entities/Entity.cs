using System;
using System.Collections.Generic;
using CoreLib.Entities.PersistentData;
using UnityEngine;

namespace CoreLib.Entities
{
    [DisallowMultipleComponent]
    public class Entity : MonoBehaviour
    {
        public EntityType entityType = EntityType.CHARACTER;
        public string entityGUID;

        private GameObject _linkedGameObject;
        // private List<InventoryData> _inventoryData = new List<InventoryData>();


        public GameObject linkedGameObject
        {
            set
            {
                if (_linkedGameObject == null)
                {
                    _linkedGameObject = value;
                    return;
                }

                throw new Exception();
            }
            get
            {
                return _linkedGameObject;
            }
        }


        /// <summary>
        /// cannot be called before entity has been loaded, since the load data may choose 
        /// </summary>
        /// <param name="size"></param>
        /// <returns>index of new inventory</returns>
        public InventoryData GetInventory(int index)
        {
            // if (index >= _inventoryData.Count)
            // {
            //     //expand inventory
            //     for (int i = _inventoryData.Count; i <= index; i++)
            //     {
            //         _inventoryData.Add(null);
            //     }
            // }

            throw new NotImplementedException();
            // if (_inventoryData[index] == null)
            //     _inventoryData[index] = new InventoryData(index);
            // return _inventoryData[index];
        }
    }
}