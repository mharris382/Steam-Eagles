using CoreLib;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace SaveLoad.CoreSave
{
    [LoadOrder(-1000)]
    public class CoreSaveDataLoader : SaveFileLoader<CoreSaveData>
    {
        public override bool LoadData(CoreSaveData data)
        {
            SceneManager.LoadScene(data.Scene);
            
            return true;
        }
    }
    
    
    [LoadOrder(-1000)]
    public class AsyncCoreSaveDataLoader : AsyncSaveFileLoader<CoreSaveData>, IAsyncGameLoader
    {
        // public override bool LoadData(CoreSaveData data)
        // {
        //     SceneManager.LoadScene(data.Scene);
        //     var builderPrefab = Addressables.LoadAssetAsync<GameObject>("Builder_Prefab");
        //     var transporterPrefab = Addressables.LoadAssetAsync<GameObject>("Transporter_Prefab");
        //     
        //     return true;
        // }
        public async override UniTask<bool> LoadData(string savePath, CoreSaveData data)
        {
            var loadSceneOp = SceneManager.LoadSceneAsync(data.Scene);
            LoadedCharacterPrefabs.BuilderPrefabLoadOp = Addressables.LoadAssetAsync<GameObject>("Builder_Prefab");
            LoadedCharacterPrefabs.TransporterPrefabLoadOp = Addressables.LoadAssetAsync<GameObject>("Transporter_Prefab");
            
            await loadSceneOp;
            await LoadedCharacterPrefabs.BuilderPrefabLoadOp;
            await LoadedCharacterPrefabs.TransporterPrefabLoadOp;
            
            LoadedCharacterPrefabs.LoadedBuilderPrefab = LoadedCharacterPrefabs.BuilderPrefabLoadOp.Result;
            LoadedCharacterPrefabs.LoadedTransporterPrefab = LoadedCharacterPrefabs.TransporterPrefabLoadOp.Result;

            Debug.Assert(LoadedCharacterPrefabs.LoadedBuilderPrefab != null, "Failed to load builder prefab");
            Debug.Assert(LoadedCharacterPrefabs.LoadedTransporterPrefab != null, "Failed to load transporter prefab");
            for (int i = 0; i < data.PlayerCharacterNames.Length; i++)
            {
                var playerCharacter = data.PlayerCharacterNames[i];
                var spawnPoint = SpawnDatabase.Instance.GetSpawnPointForScene(playerCharacter, savePath);
                var prefab = playerCharacter == "Builder"
                    ? LoadedCharacterPrefabs.LoadedBuilderPrefab
                    : LoadedCharacterPrefabs.LoadedTransporterPrefab;
                MessageBroker.Default.Publish(new RequestPlayerCharacterSpawn(playerCharacter, prefab, i, spawnPoint));
            }
            
            
            
            var loadedSceneSuccess = SceneManager.GetActiveScene().buildIndex == data.Scene;
            return loadedSceneSuccess;
        }
    }


    public static class LoadedCharacterPrefabs
    {
        public static GameObject LoadedTransporterPrefab;
        public static GameObject LoadedBuilderPrefab;
        
        internal static AsyncOperationHandle<GameObject> TransporterPrefabLoadOp;
        internal static AsyncOperationHandle<GameObject> BuilderPrefabLoadOp;
    }
}