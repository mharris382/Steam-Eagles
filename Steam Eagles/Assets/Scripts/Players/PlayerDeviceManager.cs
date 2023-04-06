using System;
using System.Collections;
using System.Linq;
using Characters;
using CoreLib;
using Cysharp.Threading.Tasks;
using Players.Shared;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Players
{
    [RequireComponent(typeof(PlayerInputManager))]
    public class PlayerDeviceManager : Singleton<PlayerDeviceManager>
    {
        public override bool DestroyOnLoad => false;
        
        public const int MAX_LOCAL_PLAYERS = 2;
        private PlayerInputManager _playerInputManager;
        
        [SerializeField, Required, AssetList(Path = "Data")]
        private SharedInt numberOfJoinedDevices;
        
        
        
        private Player[] _players;
        private ReactiveCollection<PlayerInput> _playerInputs; 

        protected override void Init()
        {
            if (!PlayerPrefs.HasKey("Local Player Count")) 
                UpdateDesiredNumberOfPlayers(1);
            
            _playerInputManager = GetComponent<PlayerInputManager>();
            _playerInputs = new ReactiveCollection<PlayerInput>();
            _playerInputManager.onPlayerJoined += OnPlayerJoin;
            _playerInputManager.onPlayerLeft += OnPlayerLeft;
        }

        public void UpdateDesiredNumberOfPlayers(int cnt)
        {
            PlayerPrefs.SetInt("Local Player Count", cnt);
        }

        protected override void CleanupDuplicate()
        {
            Debug.Log("Destroying duplicate player input manager", gameObject);
            Destroy(_playerInputManager);
        }

        public bool CanAssignPlayers { get; set; }
        
        private IEnumerator Start()
        {
            CanAssignPlayers = false;
            //TODO: load player assets from addressables
            AsyncOperationHandle<System.Collections.Generic.IList<Player>> _loadOp;
            _players = new Player[MAX_LOCAL_PLAYERS];
            
            //TODO: wait for assets to finish loading
            yield return UniTask.ToCoroutine(async () =>
            {
                Debug.Log("Loading Player Assets");
                _loadOp = Addressables.LoadAssetsAsync<Player>("players", Callback);
                await _loadOp.Task;
                _loadOp.Completed += handle =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        Debug.Log("Loaded Player Assets");
                        Debug.Assert(_loadOp.Result.Count == _players.Length, $"There should be exactly {_players.Length} local player assets, found {_loadOp.Result.Count}! ");
                        for (int i = 0; i < _players.Length; i++)
                        {
                            var i1 = i;
                            _players[i] = _loadOp.Result.FirstOrDefault(t => t.playerNumber == i1);
                        }
                    }
                    else
                    {
                        Debug.LogError("Failed to load player assets!");
                    }
                };
            });
            bool hasAllPlayerAssets = true;
            foreach (var player in _players)
            {
                if (player == null)
                {
                    hasAllPlayerAssets = false;
                    break;
                }
            }
            if (!hasAllPlayerAssets)
            {
                yield break;
            }
            CanAssignPlayers = true;
        }

        private void Callback(Player obj)
        {
            
        }

        private void OnDestroy()
        {
            _playerInputManager.onPlayerJoined -= OnPlayerJoin;
            _playerInputManager.onPlayerLeft -= OnPlayerLeft;
        }

        public void OnPlayerJoin(PlayerInput obj)
        {
            MessageBroker.Default.Publish(new PlayerDeviceJoined(obj.playerIndex, obj.gameObject));
            DontDestroyOnLoad(obj.gameObject);
            if (_playerInputs.Contains(obj)) return;
            _playerInputs.Add(obj);
            obj.transform.SetParent(transform);
        }

        public void OnPlayerLeft(PlayerInput obj)
        {
            MessageBroker.Default.Publish(new PlayerDeviceLost(obj.playerIndex, obj.gameObject));
            if (!_playerInputs.Contains(obj)) return;
            _playerInputs.Remove(obj);
        }
    }
}