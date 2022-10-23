using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Tilemaps;

namespace World
{
    public class LevelManager : MonoBehaviour
    {
        private static LevelManager _instance;

        public static LevelManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<LevelManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("LevelManager", typeof(LevelManager));
                        _instance = go.GetComponent<LevelManager>();
                    }
                }
                return _instance;
            }
        }

        public BlockData[] Blocks => HasBlocks ? _sharedBlocks.Value : Array.Empty<BlockData>();

        public enum Tilemaps
        {
            SKY = 30,
            BACKGROUND = 40,
            GROUND = 50,
            GAS = 60,
        }
        
        private void Awake()
        {
            if (_instance != null)
                Destroy(this);
            else
                _instance = this;
        }

       
        
        public bool IsLoaded => (_sharedTilemaps != null) && (_sharedBlocks != null);

        public bool HasTilemaps =>
            IsLoaded && _sharedTilemaps.Value is { Length: > 0 };

        public bool HasBlocks => IsLoaded && _sharedBlocks.Value is { Length: > 0 };

        private SharedBlocks _sharedBlocks;
        private SharedTilemaps _sharedTilemaps;
        private Tilemap[] tilemaps;
        

        private IEnumerator Start()
        {
            var loadOp1 = Addressables.LoadAssetAsync<SharedTilemaps>("Shared Tilemaps");
            var loadOp2 = Addressables.LoadAssetAsync<SharedBlocks>("Shared BlockData");
            yield return loadOp1;
            
            Debug.Assert(loadOp1.Status == AsyncOperationStatus.Succeeded );
            _sharedTilemaps = loadOp1.Result;
            _sharedTilemaps.onValueChanged.AddListener(maps =>
            {
                tilemaps = maps;
            });
            Debug.Assert(_sharedTilemaps !=null);

            _sharedTilemaps.onValueChanged.AddListener(OnSharedTilemapsChanged);
            if (_sharedTilemaps.Value is {Length: >0})
            {
                OnSharedTilemapsChanged(_sharedTilemaps.Value);
            }
            
            
            yield return loadOp2;
            _sharedBlocks = loadOp2.Result;
            Debug.Log($"Loaded Block Data: # of Block = {_sharedBlocks.Value.Length}");
        }

        private void OnSharedTilemapsChanged(Tilemap[] tilemaps)
        {
            this.tilemaps = tilemaps;
            Debug.Log($"Loaded Tilemaps: # of Tilemaps = {tilemaps.Length}");
        }

        private void OnDestroy()
        {
            if(_sharedTilemaps!=null) _sharedTilemaps.onValueChanged.RemoveListener(OnSharedTilemapsChanged);
            if (_instance == this) _instance = null;
        }
    }
}