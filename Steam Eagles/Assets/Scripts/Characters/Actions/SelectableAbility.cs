using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class SelectableAbility : CellAbility
{
    [Serializable]
    private class Events
    {
        public UnityEvent<int> onSelectedAbilityIndexChanged;
        public UnityEvent<CellAbility> onSelectedAbilityChanged;
    }
    
    [Header("Selectable Abilities")]
    public CellAbility[] abilities;
    [SerializeField] private Events events;
    [SerializeField] int selectedAbility = 0;
    
    public bool debug;
    
    public int SelectedAbility
    {
        get => selectedAbility;
        set
        {
            if(debug)Debug.Log($"{name} Selected Ability Changed:{selectedAbility}");
            selectedAbility = value % abilities.Length;
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