using System.Collections.Generic;
using CoreLib;
using Players.Shared;
using UniRx;
using UnityEngine;

namespace Game
{
    public class GameManager : Singleton<GameManager>
    {
        public override bool DestroyOnLoad => false;
        private Dictionary<int, GameObject> _playerDevices = new Dictionary<int, GameObject>();
        private Dictionary<int, string> _playerCharacterNames = new Dictionary<int, string>();

        /// <summary>
        /// called when player device was at some point connected, but device became disconnected
        /// </summary>
        public event System.Action<int> PlayerLostDevice;

        protected override void Init()
        {
            MessageBroker.Default.Receive<PlayerDeviceJoined>().Subscribe(OnPlayerDeviceJoined).AddTo(this);
            MessageBroker.Default.Receive<PlayerDeviceLost>().Subscribe(OnPlayerDeviceLost).AddTo(this);
        }


        #region [Event Listeners]

        private void OnPlayerDeviceLost(PlayerDeviceLost playerDeviceLost)
        {
            if (_playerDevices.ContainsKey(playerDeviceLost.Index))
            {
                _playerDevices.Remove(playerDeviceLost.Index);
                PlayerLostDevice?.Invoke(playerDeviceLost.Index);
            }
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
                    break;
                default:
                    Debug.LogError("Game Manager does not support more than 2 players!");
                    break;
            }
        }

        #endregion
    }
}