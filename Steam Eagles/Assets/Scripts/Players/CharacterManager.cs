using System;
using CoreLib.SaveLoad;
using SaveLoad;
using UnityEngine;

namespace Players
{
    public class CharacterManager : MonoBehaviour
    {
        public CharacterAssignments characterAssignments;

        
        private void Awake()
        {
            
        }


        public static void MoveCharacterToSpawn(Character character)
        {
            var spawnPosition = SpawnDatabase.Instance.GetSpawnPointForScene(character.tag,
                PersistenceManager.Instance.SaveDirectoryPath);
            
            Debug.Log($"Spawning {character.tag} at {spawnPosition}",character);
            
            GameEvents.NotifyCharacterSpawned(character);
        }
    }
    
    
}