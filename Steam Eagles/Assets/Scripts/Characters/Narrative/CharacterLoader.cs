using System;
using System.Collections.Generic;
using CoreLib;

namespace Characters.Narrative
{
    public class CharacterLoader : Singleton<CharacterLoader>
    {
        private Dictionary<CharacterDescription, Character> _loadedCharacters = new Dictionary<CharacterDescription, Character>();
        


        public bool TryGetCharacter(string characterName, out Character character)
        {
            if (IsCharacterLoaded(characterName))
            {
                
            }
            character = null;
            return false;
        }
        
        public bool GetOrLoadCharacter(string characterName, Action<Character> onLoad)
        {
            if (IsCharacterLoaded(characterName))
            {
                
            }
            return false;
        }
        
        public bool IsCharacterLoaded(string characterName)
        {
            return false;
        }
        
    }
}