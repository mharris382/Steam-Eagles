using System;
using CoreLib.SaveLoad;
using SaveLoad;
using SteamEagles.Characters;
using UnityEngine;

namespace Players
{
    public class CharacterManager : MonoBehaviour
    {
        public CharacterAssignments characterAssignments;

        
        private void Awake()
        {
            
        }


        public static void MoveCharacterToSpawn(CharacterState characterState)
        {
            // var spawnPosition = SpawnDatabase.Instance.GetSpawnPointForScene(characterState.tag,
            //     PersistenceManager.Instance.SaveDirectoryPath);
            
            // Debug.Log($"Spawning {characterState.tag} at {spawnPosition}",characterState);
            
            GameEvents.NotifyCharacterSpawned(characterState);
        }
    }
    
    
}