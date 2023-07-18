using CoreLib.MyEntities.ECS;
using Unity.Entities;
using UnityEngine.InputSystem;

namespace CoreLib.MyEntities.ECS
{
    public partial class GetPlayerInputSystem : SystemBase
    {
        private PlayerControls.GameplayActions _gameplayActions;
        private Entity _playerEntity;


        protected override void OnCreate()
        {
            RequireForUpdate<MoveInput>();
            RequireForUpdate<PlayerTag>();
        }

        protected override void OnStartRunning()
        {
            _gameplayActions.Enable();
        }

        protected override void OnStopRunning()
        {
            _gameplayActions.Disable();
            _gameplayActions.Jump.performed += OnPlayerShoot;
        }

        protected override void OnUpdate()
        {
            
        }

        private void OnPlayerShoot(InputAction.CallbackContext obj)
        {
            
        }
    }
}