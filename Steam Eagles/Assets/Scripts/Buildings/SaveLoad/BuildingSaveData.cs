using System;
using System.Collections.Generic;
using System.Linq;
using Buildables;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Buildings.SaveLoad
{
    [Serializable]
    public class BuildingSaveData
    {
      [SerializeField] private List<MachineSaveData> _machineSaves = new List<MachineSaveData>();
      [SerializeField] private TilemapSaveData _tilemapSaveData;
      [SerializeField] private TilemapsSaveDataV3 _tilemapsSaveDataV3;
        public BuildingSaveData(Building building)
        {
            _machineSaves = building.GetComponentsInChildren<BuildableMachineBase>().Select(t => new MachineSaveData(t))
                .Where(t => t.IsValid()).ToList();
            Debug.Log($"Saving {_machineSaves.Count} machines inside building: {building.ID}.");
            _tilemapSaveData = new TilemapSaveData(building);
        }

        
        public void Load(Building target, Action onCompleted)
        {
            target.StartCoroutine(UniTask.ToCoroutine(async () =>
            {
                Debug.Log($"Loading all data for building: {target.ID}...", target);
                var loadMachines = LoadAllMachines(target);
                LoadTilemapData(target);
                await loadMachines;
                onCompleted?.Invoke();
            }));
        }

        public async UniTask<bool> LoadAsync(Building target)
        {
            var loadMachines = LoadAllMachines(target);
            LoadTilemapData(target);
            await loadMachines;
            return true;
        }
        
        /// <summary>
        /// first loads prefabs from addressables using keys saved in machine save data
        /// after prefabs are loaded then spawns each machine in the building
        /// </summary>
        /// <param name="target"></param>
        private async UniTask LoadAllMachines(Building target)
        {
           
                var machineLoadOps = GetAllLoadOps();
                
                Debug.Log("Waiting for machines to load...");
                await UniTask.WhenAll(machineLoadOps.Values.Select(t => t.ToUniTask()));
                Debug.Log("Machines loaded.");
                
                var loadedMachines = new Dictionary<string, BuildableMachineBase>();
                foreach (var kvp in machineLoadOps)
                {
                    var machinePrefab = GetMachinePrefab(kvp.Key, kvp.Value);
                    loadedMachines.Add(kvp.Key,  machinePrefab);
                }
                
                Debug.Log("Instantiating machines...");
                
                foreach (var machineSaveData in _machineSaves)
                {
                    var prefab = loadedMachines[machineSaveData.machineAddress];
                    prefab.IsFlipped = machineSaveData.isFlipped;
                    var machine =prefab.Build((Vector3Int)machineSaveData.machineCellPosition, target);
                    var customSaveData = machine.GetComponent<IMachineCustomSaveData>();
                    if (customSaveData != null) customSaveData.LoadDataFromJson(machineSaveData.customSaveData);
                }

                foreach (var asyncOperationHandle in machineLoadOps)
                {
                    Addressables.Release(asyncOperationHandle.Value);
                }
                
                

            Dictionary<string, AsyncOperationHandle<GameObject>> GetAllLoadOps()
            {
                var machineLoadOps = new Dictionary<string, AsyncOperationHandle<GameObject>>();
                if (_machineSaves == null)
                {
                    return machineLoadOps;
                }
                foreach (var machineSaveData in _machineSaves)
                {
                    var loadOp = Addressables.LoadAssetAsync<GameObject>(machineSaveData.machineAddress);
                    if(!machineLoadOps.ContainsKey(machineSaveData.machineAddress))
                        machineLoadOps.Add(machineSaveData.machineAddress, loadOp);
                }

                return machineLoadOps;
            }
            BuildableMachineBase GetMachinePrefab(string loadKey, AsyncOperationHandle<GameObject> loadOp)
            {
                Debug.Assert(loadOp.Status == AsyncOperationStatus.Succeeded, $"Failed to load machine prefab {loadKey}.");
                var machinePrefab = loadOp.Result.GetComponent<BuildableMachineBase>();
                Debug.Assert(machinePrefab != null, $"{loadKey} has asset but no BuildableMachineBase component.", machinePrefab);
                machinePrefab.machineAddress = loadKey;
                return machinePrefab;
            }
        }


        private void LoadTilemapData(Building target)
        {
            _tilemapSaveData.LoadTilemapData(target);
        }
    }
}