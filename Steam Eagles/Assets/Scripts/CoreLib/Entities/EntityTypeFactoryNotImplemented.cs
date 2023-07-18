using System;

namespace CoreLib.MyEntities.Factory
{
    public class EntityTypeFactoryNotImplemented : NotImplementedException
    {
        public EntityTypeFactoryNotImplemented(EntityType entityType) : base($"Factory for {entityType} not implemented!")
        {
        }   
    }
}