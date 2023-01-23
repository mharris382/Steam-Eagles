using System;
using System.Linq;
using Characters.Actions.Selectors;
using Characters.Actions.UI;
using CoreLib;
using Players;
using UnityEngine;

namespace Characters
{
    [RequireComponent(typeof(AbilityUser))]
    [RequireComponent(typeof(CellAbilitySet))]
    [RequireComponent(typeof(SelectorBase))]
    public class CharacterTilemapAbilityManager : MonoBehaviour
    {
        [SerializeField] private Player player;
        [SerializeField] private SharedCamera camera;
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
            
            _selector.TargetCamera = camera.Value;
            var primaryAbility = _abilitySet.GetAbility(0);
            var secondaryAbility = _abilitySet.GetAbility(1);
            int targetAbilityIndex = 0;
            Vector3 wsPos = Vector3.zero;
            Vector3Int cellPos = Vector3Int.zero;
            foreach (var ability in _abilitySet)
            {
                if (TrySelectCell(ability, out wsPos, out cellPos))
                {
                    abilityUI.ShowAbilityPreview(wsPos, targetAbilityIndex);
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
            if (Input.GetMouseButton(targetAbilityIndex))
            {
                ability.PerformAbilityOnCell(_user, cellPos);
                return true;
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

        

        bool HasAbilities()
        {
            bool hasAbilities = false;
            foreach (var ability in _abilitySet)
            {
                if (ability == null) return false;
                hasAbilities = true;
            }
            return hasAbilities;
        }

        bool DoAbilitiesHaveTargets()
        {
            if(!HasAbilities())return false;
            foreach (var ability in _abilitySet)
            {
                if (ability.Tilemap == null)
                {
                    Debug.LogError($"Ability {ability.name} has no tilemap!", ability);
                    return false;
                }
            }
            return true;
        }
        bool IsAbleToUseAbilities()
        {
            if (camera == null || !camera.HasValue) return false;
            if (player == null) return false;
            if (!HasAbilities()) return false;
            if (!DoAbilitiesHaveTargets()) return false;
            if (_selector == null) return false;
            return true;
        }
        
        
    }
}