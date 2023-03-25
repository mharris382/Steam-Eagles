using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace CoreLib.SaveLoad
{
    public class SpawnHelper : MonoBehaviour
    {
        public string characterName;


        public string savePath;

        [Button]
        void UpdateSpawnPosition()
        {
            if(string.IsNullOrEmpty(characterName))
                return;
            var spawnDatabase = SpawnDatabase.Instance;
            
            
            spawnDatabase.UpdateDefaultSpawnPoint(characterName, this.transform.position);
        }

        [Button]
        void MoveToCurrentDefaultSpawnPosition()
        {
            if(string.IsNullOrEmpty(characterName))
                return;
            var spawnDatabase = SpawnDatabase.Instance;
            if (spawnDatabase.HasDefaultSpawnPosition(characterName))
            {
                this.transform.position = spawnDatabase.GetDefaultSpawnPointForScene(characterName);
            }
        }
        [Button]
        void MoveToCurrentSpawnPosition()
        {
            if(string.IsNullOrEmpty(characterName))
                return;
            var spawnDatabase = SpawnDatabase.Instance;
            if (spawnDatabase.HasDefaultSpawnPosition(characterName))
            {
                this.transform.position = spawnDatabase.GetSpawnPointForScene(characterName, savePath);
            }
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}