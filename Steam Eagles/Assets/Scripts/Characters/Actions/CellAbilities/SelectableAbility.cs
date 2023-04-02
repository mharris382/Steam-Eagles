using System;
using CoreLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

[System.Obsolete("Ability system prototypes will be phased out and replaced by the tool system")]
public class SelectableAbility : CellAbility
{
    [Serializable]
    private class Events
    {
        public SharedInt selectedAbility;
        public UnityEvent<int> onSelectedAbilityIndexChanged;
        public UnityEvent<CellAbility> onSelectedAbilityChanged;
    }
    
    [Header("Selectable Abilities")]
    public CellAbility[] abilities;
    [SerializeField] private Events events;
    [SerializeField] int selectedAbility = 0;
    [SerializeField] private SharedInt selectedAbilityIndex;
    
    public bool debug;
    
    public int SelectedAbility
    {
        get => selectedAbility;
        set
        {
            if(debug)Debug.Log($"{name} Selected Ability Changed:{selectedAbility}");
            selectedAbility = value % abilities.Length;
            events.selectedAbility.Value = selectedAbility;
            events.onSelectedAbilityIndexChanged?.Invoke(selectedAbility);
            events.onSelectedAbilityChanged?.Invoke(abilities[selectedAbility]);
        }
    }

    public override Tilemap Tilemap => abilities[SelectedAbility].Tilemap;

 
    
   
    
    public override bool CanPerformAbilityOnCell(AbilityUser abilityUser, Vector3Int cellPosition)
    {
        return abilities[selectedAbility].CanPerformAbilityOnCell(abilityUser, cellPosition);
    }

    public override void PerformAbilityOnCell(AbilityUser abilityUser,Vector3Int cell)
    {
        abilities[selectedAbility].PerformAbilityOnCell(abilityUser, cell);
    }
}