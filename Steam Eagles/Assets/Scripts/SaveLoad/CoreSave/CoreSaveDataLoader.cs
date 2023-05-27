using CoreLib;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using Zenject;

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
    public class AsyncCoreSaveDataLoader : AsyncSaveFileLoader<CoreSaveData>, IAsyncGameLoader, IInitializable, ISaveLoaderSystem
    {
        private PersistenceConfig _config;
        // public override bool LoadData(CoreSaveData data)
        // {
        //     SceneManager.LoadScene(data.Scene);
        //     var builderPrefab = Addressables.LoadAssetAsync<GameObject>("Builder_Prefab");
        //     var transporterPrefab = Addressables.LoadAssetAsync<GameObject>("Transporter_Prefab");
        //     
        //     return true;
        // }

        [Inject]
        public void InjectConfig(PersistenceConfig config)
        {
            _config = config;
            _config.Log("Injected Config into AsyncCoreSaveDataLoader", true);
        }
        public override async UniTask<bool> LoadData(string savePath, CoreSaveData data)
        {
            var loadSceneOp = SceneManager.LoadSceneAsync(data.Scene);
            LoadedCharacterPrefabs.LoadPrefabs();
            await loadSceneOp;
            await LoadedCharacterPrefabs.BuilderPrefabLoadOp;
            await LoadedCharacterPrefabs.TransporterPrefabLoadOp;
            
            LoadedCharacterPrefabs.LoadedBuilderPrefab ??= LoadedCharacterPrefabs.BuilderPrefabLoadOp.Result;
            LoadedCharacterPrefabs.LoadedTransporterPrefab ??= LoadedCharacterPrefabs.TransporterPrefabLoadOp.Result;

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
            if (!loadedSceneSuccess)
            {
                Debug.LogError($"Failed to load scene {data.Scene}!");
            }
            return loadedSceneSuccess;
        }

        public void Initialize()
        {
            Debug.Assert(_config != null);
            _config.Log("Initialized AsyncCoreSaveDataLoader", true);
            LoadedCharacterPrefabs.LoadPrefabs();
        }

        public UniTask<bool> SaveGameAsync(string savePath)
        {
            var builder = GameObject.FindGameObjectWithTag("Builder");
            var transporter = GameObject.FindGameObjectWithTag("Transporter");
            if(builder == null && transporter == null)
                return UniTask.FromResult(false);
            if(builder != null)
                SpawnDatabase.Instance.SaveSpawnPoint(builder.tag, savePath, builder.transform.position);
            if(transporter != null)
                SpawnDatabase.Instance.SaveSpawnPoint(transporter.tag, savePath, transporter.transform.position);
            return UniTask.FromResult(true);
        }
    }


    public static class LoadedCharacterPrefabs
    {
        public static GameObject LoadedTransporterPrefab;
        public static GameObject LoadedBuilderPrefab;

        internal static AsyncOperationHandle<GameObject> TransporterPrefabLoadOp;
        internal static AsyncOperationHandle<GameObject> BuilderPrefabLoadOp;

        public static void LoadPrefabs()
        {
            if (LoadedBuilderPrefab == null)
            {
                BuilderPrefabLoadOp = Addressables.LoadAssetAsync<GameObject>("Builder_Prefab");
                BuilderPrefabLoadOp.Completed += res => LoadedBuilderPrefab = res.Result;
            }

            if (LoadedTransporterPrefab == null)
            {
                TransporterPrefabLoadOp = Addressables.LoadAssetAsync<GameObject>("Transporter_Prefab");
                TransporterPrefabLoadOp.Completed += res => LoadedTransporterPrefab = res.Result;
            }
        }
}
}