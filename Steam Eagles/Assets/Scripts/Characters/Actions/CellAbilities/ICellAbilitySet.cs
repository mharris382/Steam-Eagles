using System.Collections.Generic;

namespace Characters.Actions.UI
{
    [System.Obsolete("Ability system prototypes will be phased out and replaced by the tool system")]
    public interface ICellAbilitySet : IEnumerable<CellAbility>
    {
     
        
        public CellAbility GetAbility(int index);
    }
}