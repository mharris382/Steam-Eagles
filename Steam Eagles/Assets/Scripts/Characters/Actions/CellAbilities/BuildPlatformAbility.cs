using UnityEngine;

namespace Characters
{
    public class BuildPlatformAbility : CellAbility
    {
        
        
        
        public enum  BuildStage
        {
            NOT_PLACED,
            PLACED_POINT,
        }

        public override bool CanPerformAbilityOnCell(AbilityUser abilityUser, Vector3Int cellPosition)
        {
            throw new System.NotImplementedException();
        }

        public override void PerformAbilityOnCell(AbilityUser user, Vector3Int cell)
        {
            throw new System.NotImplementedException();
        }
    }
}