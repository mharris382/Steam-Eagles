using System.Collections.Generic;
using CoreLib.Signals;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Input
{
    public class GameInput : GameInputBase
    {
        private Dictionary<GameObject, PlayerInput> _playerInputs = new Dictionary<GameObject, PlayerInput>();
        public override void UpdateInput(GameObject playerInputGO)
        {
            if (!_playerInputs.TryGetValue(playerInputGO, out var pInput))
            {
                pInput = playerInputGO.GetComponent<PlayerInput>();
                _playerInputs.Add(playerInputGO, pInput);
            }

            if (pInput != null)
                UpdatePlayerInput(pInput);
            else
            {
                Debug.LogError($"PlayerInput not found on {playerInputGO.name}!", playerInputGO);
            }
        }

        protected virtual void UpdatePlayerInput(PlayerInput playerInput)
        {
            bool pausedPressed = playerInput.actions["Pause"].WasPressedThisFrame();
            if (pausedPressed)
            {
                MessageBroker.Default.Publish(new PlayerPressedPausedSignal(playerInput.playerIndex, playerInput));
            }
        }
    }
}