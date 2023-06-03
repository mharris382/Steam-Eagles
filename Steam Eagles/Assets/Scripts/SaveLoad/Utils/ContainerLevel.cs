using System;

namespace SaveLoad
{
    public enum ContainerLevel
    {
        PROJECT,
        SCENE,
        GAMEOBJECT
    }
    
    public class ContainerLevelAttribute : Attribute
    {
        public ContainerLevel Level { get; }

        public ContainerLevelAttribute()
        {
            Level = ContainerLevel.PROJECT;
        }

        public ContainerLevelAttribute(ContainerLevel level)
        {
            Level = level;
        }
    }
}