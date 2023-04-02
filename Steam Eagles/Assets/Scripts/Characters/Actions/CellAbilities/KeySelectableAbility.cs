using UnityEngine;

[System.Obsolete("Ability system prototypes will be phased out and replaced by the tool system")]
public class KeySelectableAbility : SelectableAbility
{
    public KeyCode[] selectAbilityKeys;

    public void Update()
    {
        Debug.Assert(selectAbilityKeys.Length == abilities.Length);
        for (int i = 0; i < selectAbilityKeys.Length; i++)
        {
            if (Input.GetKeyDown(selectAbilityKeys[i]))
            {
                this.SelectedAbility = i;
                break;
            }
        }
    }
}