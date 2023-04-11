using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib.Entities.PersistentData;
using DG.Tweening.Plugins.Core.PathCore;
using Items;
using Items.SaveLoad;
using SaveLoad;
using UnityEngine;

namespace CoreLib.Entities.Factory
{
    public abstract class EntityFactoryBase<T> : EntityFactoryBase
    {
        protected override void LoadFromJSON(Entity entity, string json)
        {
            var data = JsonUtility.FromJson<T>(json);
            if (data == null)
            {
                Debug.LogWarning($"Failed to load entity from JSON. Entity: {entity.entityGUID}\nJSON: {json}");
                return;
            }
            LoadFromSaveData(entity, data);
        }

        protected override string SaveToJSON(Entity entity)
        {
            var saveData = GetSaveDataFromEntity(entity);
            if (EntityManager.Instance.debug) 
                Debug.Log($"Saving Entity: {entity.entityGUID}");
            
            var json =  JsonUtility.ToJson(saveData);
            
            if (EntityManager.Instance.debug)
                Debug.Log($"Save Data for Entity: {entity.entityGUID}\n{json.InItalics()}");
            
            return json;
        }

        protected abstract void LoadFromSaveData(Entity entity, T data);

        protected abstract T GetSaveDataFromEntity(Entity entity);
    }
    public class CharacterFactory : EntityFactoryBase<CharacterSaveData>
    {
        public CharacterFactory()
        {
            if (EntityManager.Instance.debug) 
                Debug.Log("Created Character Factory");
        }

        public virtual Entity CreateEntity(GameObject entityTransform, string guid, string loadPath = null)
        {
            throw new NotImplementedException();
        }

        public override EntityType GetEntityType()
        {
            return EntityType.CHARACTER;
        }

        protected override void LoadFromSaveData(Entity entity, CharacterSaveData data)
        {
            Debug.Assert(entity.linkedGameObject != null, "Character Factory: Entity has no linked game object", entity);
            LoadInventoryData(entity, data);
        }

        protected override CharacterSaveData GetSaveDataFromEntity(Entity entity)
        {
            Debug.Assert(entity.linkedGameObject != null, "Character Factory: Entity has no linked game object", entity);
            var saveData = new CharacterSaveData();
            saveData.characterName = entity.linkedGameObject.tag;
            SaveInventoryData(entity, saveData);
            return saveData;
        }

        private static void LoadInventoryData(Entity entity, CharacterSaveData data)
        {
            var persistentInventories = entity.linkedGameObject.GetComponentsInChildren<PersistentInventoryBase>();
            if (EntityManager.Instance.debug)
                Debug.Log(
                    $"Character Factory: Expecting {persistentInventories.Length} persistent inventories by character entity \nEntity:{entity.entityGUID}\n{entity.linkedGameObject.name}",
                    entity.linkedGameObject);

            if (persistentInventories.Length == 0)
            {
                Debug.LogWarning(
                    $"Character Factory: Character Entity has no persistent inventories. Characters should have at least one inventory!\n(Entity GUID:{entity.entityGUID.Bolded()})\n(Linked GO:{entity.linkedGameObject.name.Bolded()})",
                    entity.gameObject);
            }
            else if (data.inventorySaveData.Count == 0)
            {
                Debug.LogError(
                    $"Characters should always have at least one inventory! {entity.entityGUID.Bolded()} has no inventory save data!",
                    entity.gameObject);
            }
            else if (persistentInventories.Length != data.inventorySaveData.Count)
            {
                Debug.LogWarning(
                    $"Expected number of inventories ({persistentInventories.Length}) does not match number of inventories in save data ({data.inventorySaveData.Count})\n" +
                    $"Entity GUID: {entity.entityGUID.Bolded()}\n" +
                    $"Linked GO: {entity.linkedGameObject.name.Bolded()}", entity.gameObject);
            }
            else
            {
                //correct number of inventories found in save data and on character entity linked game object
                for (int i = 0; i < persistentInventories.Length; i++)
                {
                    var inventory = persistentInventories[i];
                    var inventorySaveData = data.inventorySaveData[i];
                    if (EntityManager.Instance.debug)
                        Debug.Log($"Character Factory: Loading inventory {inventory.UniqueInventoryID} from save data",
                            entity.linkedGameObject);
                    inventory.LoadFromSaveData(inventorySaveData.inventoryData.itemSlots.Select(itemSlot =>
                        (itemSlot.itemID, itemSlot.itemAmount)).ToArray());
                }
            }
        }

        private static void SaveInventoryData(Entity entity, CharacterSaveData saveData)
        {
            var persistentInventories = entity.linkedGameObject.GetComponentsInChildren<PersistentInventoryBase>();
            if (persistentInventories.Length == 0)
            {
                Debug.LogWarning(
                    $"Character Factory: Character Entity has no persistent inventories. Characters should have at least one inventory!\n(Entity GUID:{entity.entityGUID.Bolded()})\n(Linked GO:{entity.linkedGameObject.name.Bolded()})",
                    entity.gameObject);
            }
            else
            {
                if (EntityManager.Instance.debug)
                    Debug.Log(
                        $"Character Factory: Found {persistentInventories.Length} persistent inventories on character entity \nEntity:{entity.entityGUID}\n{entity.linkedGameObject.name}");
                saveData.inventorySaveData =
                    persistentInventories.Select(t => new CharacterSaveData.InventorySaveData(t)).ToList();
            }
        }
    }

    [Serializable]
    public class CharacterSaveData
    {
        public string characterName;
        
        public List<InventorySaveData> inventorySaveData;
        
        [Serializable]
        public class InventorySaveData
        {
            public const int MIN_INVENTORY_SIZE = 2;

            public int size = 10;
            
            public InventoryData inventoryData;
            public InventorySaveData(){ }

            public InventorySaveData(PersistentInventoryBase inventory)
            {
                inventoryData  = new InventoryData(inventory.UniqueInventoryID);
                foreach (var itemStack in inventory.Inventory.Items)
                {
                    if (itemStack.IsEmpty)
                    {
                        inventoryData.itemSlots.Add(new InventoryData.ItemSlots()
                        {
                            itemID = "",
                            itemAmount = 0
                        });
                    }
                    else
                    {
                        inventoryData.itemSlots.Add(new InventoryData.ItemSlots()
                        {
                            itemID = itemStack.Key,
                            itemAmount = itemStack.Count
                        });
                    }
                }
            }
        }
    }


    //public abstract class PCNewGameSaveCreator : NewGameSaveFileCreator<CharacterSaveData>
    //{
    //    public override void CreateNewSaveFile(string savePath)
    //    {
    //        savePath = System.IO.Path.Combine(savePath, "Entities");
    //        base.CreateNewSaveFile(savePath);
    //    }   
//
    //    protected override CharacterSaveData GetNewGameSaveState()
    //    {
    //        
    //    }
    //}
}