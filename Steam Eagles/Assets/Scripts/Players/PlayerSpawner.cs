using System;
using Characters;
using CoreLib.SaveLoad;
using UnityEngine;

namespace Players
{
    public class PlayerSpawner : MonoBehaviour
    {
        private void Awake()
        {
            PlayerWrapper.onPlayerInitialized += SpawnPlayer;
        }

        private void SpawnPlayer(Player obj)
        {
            var playerCharacterTag = obj.characterTag;
            var position = SpawnDatabase.Instance.GetSpawnPointForScene(playerCharacterTag, PersistenceManager.Instance.SaveDirectoryPath);
            var playerCharacter = obj.characterTransform.Value;
            Debug.Assert(playerCharacter != null, "player should not have been passed into initialized because character is null", this);
            playerCharacter.transform.position = position;
        }
    }
    
}