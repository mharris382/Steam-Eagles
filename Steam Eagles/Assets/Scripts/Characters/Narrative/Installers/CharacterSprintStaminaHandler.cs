using CoreLib.Entities;
using Zenject;

namespace Characters.Narrative.Installers
{
    public class CharacterSprintStaminaHandler : ITickable
    {
        private readonly CharacterEntities _characterEntities;

        public CharacterSprintStaminaHandler(CharacterEntities characterEntities)
        {
            _characterEntities = characterEntities;
        }
        public void Tick()
        {
            foreach (var entity in _characterEntities)
            {
               
            }
        }
        
        private void UpdateEntityStamina(Entity entity)
        {
            
        }
    }


    public class CharacterEntities : EntityTypeTracker
    {
        public override bool IsEntityTrackedByThis(Entity entity) => entity.entityType == EntityType.CHARACTER;
    }
}