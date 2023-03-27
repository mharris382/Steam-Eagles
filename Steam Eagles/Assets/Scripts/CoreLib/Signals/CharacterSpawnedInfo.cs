using UnityEngine;

namespace CoreLib
{
    public struct CharacterSpawnedInfo
    {
        public readonly string characterName;
        public readonly GameObject character;

        public CharacterSpawnedInfo(string characterName, GameObject character)
        {
            this.characterName = characterName;
            this.character = character;
            Debug.Assert(this.character != null);
        }
    }
}