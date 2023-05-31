using System;
using Cysharp.Threading.Tasks;
using Items.SaveLoad;
using UnityEngine;
using System.Linq;
using JetBrains.Annotations;

namespace CoreLib.Entities.Factory
{
    [UsedImplicitly]
    public class CharacterEntitySaveLoader : EntityJsonSaveLoaderBase<CharacterSaveData>
    {
        public override EntityType GetEntityType() => EntityType.CHARACTER;
        public override UniTask<CharacterSaveData> GetDataFromEntity(EntityHandle entityHandle)
        {
            var saveData = new CharacterSaveData();
            SaveInventoryData(entityHandle, entityHandle.LinkedGameObject, saveData);
            return UniTask.FromResult(saveData);
        }
        public override UniTask<bool> LoadDataIntoEntity(EntityHandle entityHandle, CharacterSaveData data)
        {
            return UniTask.FromResult(LoadInventoryData(entityHandle, entityHandle.LinkedGameObject, data));
        }
        private static bool SaveInventoryData(EntityHandle entity, GameObject LinkedGameObject, CharacterSaveData saveData)
        {
            var persistentInventories = LinkedGameObject.GetComponentsInChildren<PersistentInventoryBase>();
            if (persistentInventories.Length == 0)
            {
                Debug.LogWarning(
                    $"Character Factory: Character Entity has no persistent inventories. Characters should have at least one inventory!\n(Entity GUID:{entity.EntityGUID.Bolded()})\n(Linked GO:{entity.LinkedGameObject.name.Bolded()})",
                    LinkedGameObject);
                return true;
            }
            else
            {
                if (EntityManager.Instance.debug)
                    Debug.Log(
                        $"Character Factory: Found {persistentInventories.Length} persistent inventories on character entity \nEntity:{entity.EntityGUID}\n{entity.LinkedGameObject.name}");
                saveData.inventorySaveData =
                    persistentInventories.Select(t => new CharacterSaveData.InventorySaveData(t)).ToList();
                return true;
            }
        }
        private static bool LoadInventoryData(EntityHandle handle, GameObject LinkedGameObject, CharacterSaveData data)
        {
            var persistentInventories = LinkedGameObject.GetComponentsInChildren<PersistentInventoryBase>();
            if (EntityManager.Instance.debug)
                Debug.Log(
                    $"Character Factory: Expecting {persistentInventories.Length} persistent inventories by character entity \nEntity:{handle.EntityGUID}\n{handle.LinkedGameObject.name}",
                    LinkedGameObject);

            if (persistentInventories.Length == 0)
            {
                Debug.LogWarning(
                    $"Character Factory: Character Entity has no persistent inventories. Characters should have at least one inventory!\n(Entity GUID:{handle.EntityGUID.Bolded()})\n(Linked GO:{LinkedGameObject.name.Bolded()})",
                    LinkedGameObject);
                return true;
            }
            else if (data.inventorySaveData.Count == 0)
            {
                Debug.LogError(
                    $"Characters should always have at least one inventory! {handle.EntityGUID.Bolded()} has no inventory save data!",
                    LinkedGameObject.gameObject);
                return false;
            }
            else if (persistentInventories.Length != data.inventorySaveData.Count)
            {
                Debug.LogWarning(
                    $"Expected number of inventories ({persistentInventories.Length}) does not match number of inventories in save data ({data.inventorySaveData.Count})\n" +
                    $"Entity GUID: {handle.EntityGUID.Bolded()}\n" +
                    $"Linked GO: {LinkedGameObject.name.Bolded()}", LinkedGameObject);
                return false;
            }
            else
            {
                //correct number of inventories found in save data and on character entity linked game object
                for (int i = 0; i < persistentInventories.Length; i++)
                {
                    var inventory = persistentInventories[i];
                    var inventorySaveData = data.inventorySaveData[i];
                    if (EntityManager.Instance.debug)
                        Debug.Log($"Character Factory: Loading inventory {inventory.UniqueInventoryID} from save data", LinkedGameObject);
                    inventory.LoadFromSaveData(inventorySaveData.inventoryData.itemSlots.Select(itemSlot => (itemSlot.itemID, itemSlot.itemAmount)).ToArray());
                }

                return true;
            }
        }
    }
}