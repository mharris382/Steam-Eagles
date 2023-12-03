using System;
using UnityEngine;

namespace CoreLib
{
    [System.Obsolete("probably use CharacterRegistry instead")]
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
        public Action callback;
        public GameObject Building { get; set; }

        public RequestPlayerCharacterSpawn(string characterName, GameObject characterPrefab, int playerCharacterIndex, Vector2 spawnPositionLocal)
        {
            this.characterName = characterName;
            this.characterPrefab = characterPrefab;
            this.playerCharacterIndex = playerCharacterIndex;
            this.spawnPositionLocal = spawnPositionLocal;
            Building = null;
            callback = null;
        }
    }

    public struct RequestSwitchPlayerCharacter
    {
        public readonly int playerCharacterIndex;
        public readonly string characterName;
        public readonly GameObject characterPrefab;
        public readonly Vector2 spawnPositionLocal;

        public RequestSwitchPlayerCharacter(int playerCharacterIndex, string characterName, GameObject characterPrefab, Vector2 spawnPositionLocal)
        {
            this.playerCharacterIndex = playerCharacterIndex;
            this.characterName = characterName;
            this.characterPrefab = characterPrefab;
            this.spawnPositionLocal = spawnPositionLocal;
        }
    }
}