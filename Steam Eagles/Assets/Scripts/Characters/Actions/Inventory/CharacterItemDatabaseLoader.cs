using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Characters.Actions.Inventory
{
    [CreateAssetMenu(fileName = "New Inventory Item", menuName = "Steam Eagles/Character Item Database Loader")]
    public class CharacterItemDatabaseLoader : ScriptableObject
    {
        public string itemDatabaseAddress;
        public string[] itemAddresses;
    
        public async Task<CharacterItemDatabase> LoadDatabase()
        {
            AsyncOperationHandle<CharacterItemDatabase> handle = Addressables.LoadAssetAsync<CharacterItemDatabase>(itemDatabaseAddress);
            
            await handle.Task;
            return handle.Result;
        }

        private void OnDatabaseLoaded(AsyncOperationHandle<CharacterItemDatabase> obj)
        {
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                var database = obj.Result;
                Debug.Log($"Loaded database {database.name}");
            }
        }
    }
    //[CreateAssetMenu(fileName = "New Inventory Item", menuName = "Steam Eagles/Inventory Data")]
}