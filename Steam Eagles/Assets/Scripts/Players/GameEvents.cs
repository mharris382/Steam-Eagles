using System;

namespace Players
{
    public static class GameEvents
    {
        public static event Action<Character> onCharacterSpawned;
        
        
        public static void NotifyCharacterSpawned(Character character)
        {
            onCharacterSpawned?.Invoke(character);
        }
    }
}