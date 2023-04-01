using System;
using CoreLib;
using UnityEngine;

namespace Characters.Stats
{
    public class CharacterManager : Singleton<CharacterManager>
    {
        public bool IsCharacterLoaded(CharacterDescription characterDescription)
        {
            return IsCharacterLoaded(characterDescription.name);
        }
        
        public bool IsCharacterLoaded(string characterName)
        {
            throw new NotImplementedException();
        }
    }
}