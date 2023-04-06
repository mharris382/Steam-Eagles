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


    public struct RequestPlayerCharacterSpawn
    {
        public readonly string characterName;
        public readonly GameObject characterPrefab;
        public readonly int playerCharacterIndex;
        public readonly Vector2 spawnPositionLocal;

        public RequestPlayerCharacterSpawn(string characterName, GameObject characterPrefab, int playerCharacterIndex, Vector2 spawnPositionLocal)
        {
            this.characterName = characterName;
            this.characterPrefab = characterPrefab;
            this.playerCharacterIndex = playerCharacterIndex;
            this.spawnPositionLocal = spawnPositionLocal;
        }
    }
}