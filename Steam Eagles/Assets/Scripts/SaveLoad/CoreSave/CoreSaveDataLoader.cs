using System;
using System.Collections.Generic;
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
    public class AsyncCoreSaveDataLoader : AsyncSaveFileLoader<CoreSaveData>, IAsyncGameLoader, IInitializable, IJsonSaveLoaderSystem
    {
        private PersistenceConfig _config;

        private PCRegistry _pcRegistry;
        // public override bool LoadData(CoreSaveData data)
        // {
        //     SceneManager.LoadScene(data.Scene);
        //     var builderPrefab = Addressables.LoadAssetAsync<GameObject>("Builder_Prefab");
        //     var transporterPrefab = Addressables.LoadAssetAsync<GameObject>("Transporter_Prefab");
        //     
        //     return true;
        // }

        [Inject]
        public void InjectConfig(PersistenceConfig config, PCRegistry pcRegistry)
        {
            _config = config;
            _config.Log("Injected Config into AsyncCoreSaveDataLoader", true);
            _pcRegistry = pcRegistry;
        }
        public override async UniTask<bool> LoadData(string savePath, CoreSaveData data)
        {
            var scene = SceneManager.GetActiveScene();
            
            LoadedCharacterPrefabs.LoadPrefabs();
            
            if (scene.buildIndex != data.Scene)
            {
                await SceneManager.UnloadSceneAsync(scene);
                var loadSceneOp = SceneManager.LoadSceneAsync(data.Scene);
                await loadSceneOp;
            }
            await LoadedCharacterPrefabs.BuilderPrefabLoadOp;
            await LoadedCharacterPrefabs.TransporterPrefabLoadOp;
            
            LoadedCharacterPrefabs.LoadedBuilderPrefab ??= LoadedCharacterPrefabs.BuilderPrefabLoadOp.Result;
            LoadedCharacterPrefabs.LoadedTransporterPrefab ??= LoadedCharacterPrefabs.TransporterPrefabLoadOp.Result;

            Debug.Assert(LoadedCharacterPrefabs.LoadedBuilderPrefab != null, "Failed to load builder prefab");
            Debug.Assert(LoadedCharacterPrefabs.LoadedTransporterPrefab != null, "Failed to load transporter prefab");
            for (int i = 0; i < data.PlayerCharacterNames.Length; i++)
            {
               
                var playerCharacter = data.PlayerCharacterNames[i];
                SpawnDatabase.Instance.filePath = savePath;
                var spawnPoint = await SpawnDatabase.Instance.GetSpawnPoint(playerCharacter);
                var instance = GameObject.FindGameObjectWithTag(playerCharacter);
                if (instance != null)
                {
                    var rb = instance.GetComponent<Rigidbody2D>();
                    rb.position = spawnPoint;
                    rb.velocity = Vector2.zero;
                }
                else
                {
                    var prefab = playerCharacter == "Builder"
                        ? LoadedCharacterPrefabs.LoadedBuilderPrefab
                        : LoadedCharacterPrefabs.LoadedTransporterPrefab;
                    MessageBroker.Default.Publish(new RequestPlayerCharacterSpawn(playerCharacter, prefab, i,
                        spawnPoint));
                }
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

        public override async UniTask<CoreSaveData> GetSaveData(string savePath)
        {
            List<(string, Vector3)> spawnPoints = new();
            var sceneIndex = SceneManager.GetActiveScene().buildIndex;
            string[] playerCharacterNames = new string[_pcRegistry.PCCount];
            for (int i = 0; i < _pcRegistry.PCCount; i++)
            {
                var instance = _pcRegistry.GetInstance(i);
                if (instance == null) continue;
                var character = instance.character;
                if (character == null) continue;
                spawnPoints.Add((character.tag, character.transform.localPosition));
                playerCharacterNames[i] = character.tag;
            }
            var coreSaveData = new CoreSaveData(sceneIndex, playerCharacterNames);
            await SpawnDatabase.Instance.SaveSpawnPointsAsync(savePath, spawnPoints);
            return coreSaveData;
        }

        // public UniTask<bool> SaveGameAsync(string savePath)
        // {
        //     for (int i = 0; i < _pcRegistry.PCCount; i++)
        //     {
        //         var instance = _pcRegistry.GetInstance(i);
        //         if (instance == null)
        //         {
        //             continue;
        //         }
        //         var character = instance.character;
        //         if (character == null)
        //         {
        //             continue;
        //         }
        //         SpawnDatabase.Instance.SaveSpawnPoint(character.tag, savePath, character.transform.localPosition);
        //     }
        //     return UniTask.FromResult(true);
        // }

        public override IEnumerable<(string, string)> GetSaveFileNames()
        {
            yield return (GetJsonFileName(), "json");
        }

        public string GetJsonFileName()
        {
            return "CoreSaveData";
        }

        public Type GetJsonFileType()
        {
            return typeof(CoreSaveData);
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