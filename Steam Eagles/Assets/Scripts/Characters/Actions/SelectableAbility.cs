using UnityEngine;
using UnityEngine.Tilemaps;

public class SelectableAbility : CellAbility
{
    [SerializeField] int selectedAbility = 0;
    public bool debug;
    public int SelectedAbility
    {
        get => selectedAbility;
        set
        {
            if(debug)Debug.Log($"{name} Selected Ability Changed:{selectedAbility}");
            selectedAbility = value % abilities.Length;
        }
    }

    public override Tilemap Tilemap => abilities[SelectedAbility].Tilemap;

    public CellAbility[] abilities;
    public override bool CanPerformAbilityOnCell(AbilityUser abilityUser, Vector3Int cellPosition)
    {
        return abilities[selectedAbility].CanPerformAbilityOnCell(abilityUser, cellPosition);
    }

    public override void PerformAbilityOnCell(AbilityUser abilityUser,Vector3Int cell)
    {
        abilities[selectedAbility].PerformAbilityOnCell(abilityUser, cell);
    }
}