using UnityEngine.InputSystem;

namespace Characters
{
    public interface ICharacterInput
    {
        void AssignPlayer( PlayerInput playerInput);
        void UnAssignPlayer();
    }
}