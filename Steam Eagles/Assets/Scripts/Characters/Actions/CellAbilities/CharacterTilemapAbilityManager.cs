using System;
using System.Linq;
using Characters.Actions.Selectors;
using Characters.Actions.UI;
using CoreLib;
using Players;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters
{
    [RequireComponent(typeof(AbilityUser))]
    [RequireComponent(typeof(CellAbilitySet))]
    [RequireComponent(typeof(SelectorBase))]
    public class CharacterTilemapAbilityManager : MonoBehaviour
    {
        [SerializeField] private Player player;
        
        [SerializeField] private AbilityUI abilityUI;
        
        private SelectorBase _selector;
        private ICellAbilitySet _abilitySet;
        private AbilityUser _user;

        private void Awake()
        {
            _user = GetComponent<AbilityUser>();
            _selector = GetComponent<SelectorBase>();
            _abilitySet = GetComponent<ICellAbilitySet>();
            Debug.Assert(_abilitySet != null, "Ability set is null");
            
        }

        private void OnDisable()
        {
            abilityUI.HideAllAbilityPreviews();
        }

        private void Update()
        {
            if (!IsAbleToUseAbilities())
            {
                abilityUI.HideAllAbilityPreviews();
                return;
            }
            
            _selector.TargetCamera = player.playerCamera.Value;
            int targetAbilityIndex = 0;
            
            Vector3 wsPos = Vector3.zero;
            Vector3Int cellPos = Vector3Int.zero;
            
            foreach (var ability in _abilitySet)
            {
                if (TrySelectCell(ability, out wsPos, out cellPos))
                {
                    abilityUI.ShowAbilityPreviewForAbility(ability, wsPos);
                    if (TryInputOnCell(ability, cellPos, targetAbilityIndex))
                    {
                        Debug.Log("Ability  used");
                    }
                    return;
                }
                else
                {
                    targetAbilityIndex++;
                }
            }
            abilityUI.HideAllAbilityPreviews();
        }

        private bool TryInputOnCell(CellAbility ability, Vector3Int cellPos, int targetAbilityIndex)
        {
            var action = ability.InputActionName;
            bool HasValidInput(out PlayerInput playerInput)
            {
                playerInput = null;
                var characterInput = player.CharacterInput;
                if (characterInput == null) return false;
                 playerInput = characterInput.PlayerInput;
                if (playerInput == null) return false;
                if(playerInput.actions.FindAction(action) == null) return false;
                return true;
            }

            if (!HasValidInput(out var playerInput))
                return false;
            
            if (ability.ReadInput(playerInput))
            {
                ability.PerformAbilityOnCell(_user, cellPos);
                return true;
            }

            switch (ability.InputMode)
            {
                case CellAbilityInputMode.DRAG:
                    if (playerInput.actions[action].IsPressed())
                    {
                        ability.PerformAbilityOnCell(_user, cellPos);
                        return true;
                    }
                    break;
                case CellAbilityInputMode.CLICK:
                    if (playerInput.actions[action].WasPerformedThisFrame())
                    {
                        ability.PerformAbilityOnCell(_user, cellPos);
                        return true;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            return false;
        }

        private bool TrySelectCell(CellAbility ability, out Vector3 wsPos, out Vector3Int cellPos)
        {
            wsPos = Vector3.zero;
            cellPos = Vector3Int.zero;
            _selector.TargetGrid = ability.Tilemap.layoutGrid;
            _selector.TargetTilemap = ability.Tilemap;
            if (!_selector.CanSelectCells())
            {
                return false;
            }
            foreach (var selectableCell in _selector.GetSelectableCells())
            {
                if (ability.CanPerformAbilityOnCell(_user, selectableCell.cellPos))
                {
                    cellPos = selectableCell.cellPos;
                    wsPos = selectableCell.wsPos;
                    cellPos.z = 0;
                    wsPos.z = 0;
                    return true;
                }
            }
            return false;
        }
        
        

        private bool HasAbilities()
        {
            bool hasAbilities = false;
            foreach (var ability in _abilitySet)
            {
                if (ability == null) return false;
                hasAbilities = true;
            }
            return hasAbilities;
        }

        private bool DoAbilitiesHaveTargets()
        {
            if(!HasAbilities())return false;
            foreach (var ability in _abilitySet)
            {
                if (ability.Tilemap == null)
                {
                    Debug.LogWarning($"Ability {ability.name} has no tilemap!", ability);
                    return false;
                }
            }
            return true;
        }
        private bool IsAbleToUseAbilities()
        {
            if (player == null) return false;
            if (player.playerCamera == null || !player.playerCamera.HasValue) return false;
            if (!HasAbilities()) return false;
            if (!DoAbilitiesHaveTargets()) return false;
            if (_selector == null) return false;
            return true;
        }
        
        
    }
}