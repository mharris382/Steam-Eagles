using System.Collections.Generic;

namespace Characters.Actions.UI
{
    public interface ICellAbilitySet : IEnumerable<CellAbility>
    {
     
        
        public CellAbility GetAbility(int index);
    }
}