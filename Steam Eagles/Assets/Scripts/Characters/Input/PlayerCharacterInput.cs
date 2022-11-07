using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters
{
    
    /// <summary>
    /// component responsible for the PlayerInput side of the PlayerController, this component will be attached to the PlayerInput GameObject
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerCharacterInput : MonoBehaviour
    {
        private PlayerInput _playerInput;
        public PlayerInput PlayerInput => _playerInput == null ? (_playerInput = GetComponent<PlayerInput>()) : _playerInput;
        
        public void Assign(CharacterInputState characterInputState)
        {
            throw new System.NotImplementedException();
        }

        public void UnAssign(CharacterInputState characterInputState)
        {
            throw new System.NotImplementedException();
        }
    }
}