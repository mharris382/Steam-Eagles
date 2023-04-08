using System;
using System.Collections.Generic;
using CoreLib;
using Players.Shared;
using UniRx;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(GameInputBase))]
    public class GameManager : Singleton<GameManager>
    {
        public override bool DestroyOnLoad => false;
        private Dictionary<int, GameObject> _playerDevices = new Dictionary<int, GameObject>();
        private Dictionary<int, string> _playerCharacterNames = new Dictionary<int, string>();
        private GameInputBase _gameInput;

        /// <summary>
        /// called when player device was at some point connected, but device became disconnected
        /// </summary>
        public event System.Action<int> PlayerLostDevice;

        
        public event Action<string[]> CharacterAssignmentsChanged; 
        public event Action<int> NumberOfPlayerDevicesChanged;
        
        protected override void Init()
        {
            _gameInput = GetComponent<GameInputBase>();
            Debug.Assert(_gameInput != null, "Game Manager requires a GameInputBase component!");
            MessageBroker.Default.Receive<PlayerDeviceJoined>().Subscribe(OnPlayerDeviceJoined).AddTo(this);
            MessageBroker.Default.Receive<PlayerDeviceLost>().Subscribe(OnPlayerDeviceLost).AddTo(this);
            MessageBroker.Default.Receive<PlayerCharacterBoundInfo>().Subscribe(OnPlayerCharacterBound).AddTo(this);
            MessageBroker.Default.Receive<PlayerCharacterUnboundInfo>().Subscribe(OnPlayerCharacterUnbound).AddTo(this);
        }


        #region [Event Listeners]

        private void OnPlayerCharacterBound(PlayerCharacterBoundInfo playerCharacterBound)
        {
            AssignCharacterToPlayer(playerCharacterBound.playerNumber, playerCharacterBound.character);
        }
        
        private void OnPlayerCharacterUnbound(PlayerCharacterUnboundInfo playerCharacterBound)
        {
            AssignCharacterToPlayer(playerCharacterBound.playerNumber, null);
        }

        private void OnPlayerDeviceLost(PlayerDeviceLost playerDeviceLost)
        {
            if (_playerDevices.ContainsKey(playerDeviceLost.Index))
            {
                _playerDevices.Remove(playerDeviceLost.Index);
                PlayerLostDevice?.Invoke(playerDeviceLost.Index);
            }
            NumberOfPlayerDevicesChanged?.Invoke(_playerDevices.Count);
        }

        private void OnPlayerDeviceJoined(PlayerDeviceJoined playerDeviceJoined)
        {
            if (_playerDevices.ContainsKey(playerDeviceJoined.Index))
            {
                _playerDevices[playerDeviceJoined.Index] = playerDeviceJoined.PlayerInput;
            }
            else
            {
                _playerDevices.Add(playerDeviceJoined.Index, playerDeviceJoined.PlayerInput);
            }
            NumberOfPlayerDevicesChanged?.Invoke(_playerDevices.Count);
        }

        #endregion

        #region [Public Methods]
        
        public GameObject GetPlayerDevice(int playerIndex)
        {
            if (_playerDevices.ContainsKey(playerIndex))
                return _playerDevices[playerIndex];
            return null;
        }
        
        public string GetPlayerCharacterName(int playerIndex)
        {
            if (_playerCharacterNames.ContainsKey(playerIndex))
                return _playerCharacterNames[playerIndex];
            return null;
        }

        public bool PlayerHasCharacterAssigned(int playerIndex)
        {
            if (!PlayerHasJoined(playerIndex))
                return false;
            
            switch (playerIndex)
            {
                case 0:
                case 1:
                    if(_playerCharacterNames.ContainsKey(playerIndex))
                        return !string.IsNullOrEmpty(_playerCharacterNames[playerIndex]);
                    return false;
                default:
                    Debug.LogError("Game Manager does not support more than 2 players!");
                    return false;
            }
        }

        public bool PlayerHasJoined(int playerIndex)
        {
            switch (playerIndex)
            {
                case 0:
                case 1:
                    if(_playerDevices.ContainsKey(playerIndex))
                        return _playerDevices[playerIndex] != null;
                    return false;
                default:
                    Debug.LogError("Game Manager does not support more than 2 players!");
                    return false;
            }
        }

        public bool CanStartGameInSingleplayer()
        {
            return PlayerHasJoined(0) && PlayerHasCharacterAssigned(0);
        }


        public void AssignCharacterToPlayer(int playerIndex, string characterName)
        {
            if (!PlayerHasJoined(playerIndex))
            {
                Debug.LogError($"Player {playerIndex} has not joined yet!");
                return;
            }
            
            switch (playerIndex)
            {
                case 0:
                case 1:
                    _playerCharacterNames[playerIndex] = characterName;
                    CharacterAssignmentsChanged?.Invoke(GetCharacterAssignments());
                    break;
                default:
                    Debug.LogError("Game Manager does not support more than 2 players!");
                    break;
            }
        }

        private string[] GetCharacterAssignments()
        {
            void AddPlayerCharacterAssignment(int i, List<string> list)
            {
                if (PlayerHasJoined(i))
                {
                    if (PlayerHasCharacterAssigned(i))
                    {
                        list.Add(GetPlayerCharacterName(i));
                    }
                }
            }

            List<string> characterAssignments = new List<string>();
            int p = 0;
            AddPlayerCharacterAssignment(0, characterAssignments);
            AddPlayerCharacterAssignment(1, characterAssignments);
            return characterAssignments.ToArray();
        }
        
        
        public int GetNumberOfPlayerDevices()
        {
            int cnt = 0;
            foreach (var playerDevices in _playerDevices)
            {
                if(playerDevices.Value != null)
                    cnt++;
            }

            return cnt;
        }

        #endregion

        private void Update()
        {
            foreach (var playerDevice in _playerDevices)
            {
                if (playerDevice.Value == null)
                    continue;
                _gameInput.UpdateInput(playerDevice.Value);
                //if (playerDevice.Value.GetComponent<PlayerInput>().actions["Pause"].triggered)
                //{
                //    MessageBroker.Default.Publish(new PlayerPausedGame());
                //}
            }
        }

        public bool CanStartGameInMultiplayer()
        {
            throw new NotImplementedException();
        }
    }
}