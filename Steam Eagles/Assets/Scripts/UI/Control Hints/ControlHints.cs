using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.Control_Hints
{
    public class ControlHints : MonoBehaviour
    {
        public TextMeshProUGUI text;
        public int targetPlayer;
        private PlayerInput _playerInput;

        void Awake()
        {
            
        }

        void Update()
        {
        
        }

        public void OnPlayerInputJoined(PlayerInput playerInput)
        {
            if (playerInput.playerIndex == targetPlayer)
            {
                this._playerInput = playerInput;
            }
        }

        public void OnPlayerInputLeft(PlayerInput playerInput)
        {
            if (playerInput == _playerInput)
            {
                
            }
        }
    }
}
