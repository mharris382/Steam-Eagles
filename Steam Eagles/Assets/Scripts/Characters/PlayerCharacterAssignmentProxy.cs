using System;
using CoreLib;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters
{
 
    // /// <summary>
    // /// handles the assignment of a player to a character, and updates character state 
    // /// </summary>
    // public class PlayerCharacterAssignmentProxy : MonoBehaviour
    // {
    //
    //     private ReactiveProperty<int> _assignedPlayer = new ReactiveProperty<int>();
    //     private ReactiveProperty<PlayerInput> _playerInput = new ReactiveProperty<PlayerInput>();
    //     
    //     private void Start()
    //     {
    //         MessageBroker.Default.Receive<CharacterAssignedPlayerInput>()
    //             .Where(t => CompareTag(t.characterName))
    //             .Select(t => t.inputGo.GetComponent<PlayerInput>())
    //             .Do(t => Debug.Log($"player {t.name} assigned to character {tag}", t))
    //             .Subscribe(OnAssignedInput).AddTo(this);
    //
    //         MessageBroker.Default.Receive<CharacterUnassignedPlayerInput>()
    //             .Where(t => CompareTag(t.characterName))
    //             .Select(t => t.inputGo.GetComponent<PlayerInput>())
    //             .Do(t => Debug.Log($"player {t.name} unassigned from character {tag}", t))
    //             .Subscribe(OnUnassignedInput).AddTo(this);
    //         
    //         new CharacterInputProcessorV2(GetComponent<CharacterState>(), _playerInput.ToReadOnlyReactiveProperty(), true)
    //             .AddTo(this);
    //     }
    //
    //     void OnAssignedInput(PlayerInput playerInput)
    //     {
    //         _playerInput.Value = playerInput;
    //         _assignedPlayer.Value = playerInput.playerIndex;
    //     }
    //     
    //     void OnUnassignedInput(PlayerInput playerInput)
    //     {
    //         if (_playerInput.Value == playerInput)
    //         {
    //             _playerInput.Value = null;
    //             _assignedPlayer.Value = -1;
    //         }
    //     }
    //     
    //     void OnPlayerAssignmentChanged(int playerNumber)
    //     {
    //         _assignedPlayer.Value = playerNumber;
    //         if (playerNumber == -1)
    //         {
    //             //unassigned
    //         }
    //         else
    //         {
    //             //assignment changed
    //         }
    //     }
    // }
}