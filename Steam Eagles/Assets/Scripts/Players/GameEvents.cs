using System;

namespace Players
{
    public static class GameEvents
    {
        public static event Action<CharacterState> onCharacterSpawned;
        
        
        public static void NotifyCharacterSpawned(CharacterState characterState)
        {
            onCharacterSpawned?.Invoke(characterState);
        }
    }
}