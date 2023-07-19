using Sirenix.OdinInspector;
using UnityEngine;

namespace SaveLoad
{
    public class SpawnHelper : MonoBehaviour
    {
        public string characterName;


        //public string savePath;

        
        // [Button]
        // void UpdateSpawnPosition()
        // {
        //     if(string.IsNullOrEmpty(characterName))
        //         return;
        //     var spawnDatabase = SpawnDatabase.Instance;
        //     
        //     
        //     spawnDatabase.UpdateDefaultSpawnPoint(characterName, transform.parent == null ? this.transform.position : transform.localPosition);
        // }

        // [Button]
        // void MoveToCurrentDefaultSpawnPosition()
        // {
        //     if(string.IsNullOrEmpty(characterName))
        //         return;
        //     var spawnDatabase = SpawnDatabase.Instance;
        //     if (spawnDatabase.HasDefaultSpawnPosition(characterName))
        //     {
        //         var pos = spawnDatabase.GetDefaultSpawnPointForCharacter(characterName);
        //         if (transform.parent == null)
        //         {
        //             this.transform.position = pos;
        //         }
        //         else
        //         {
        //             transform.localPosition = pos;
        //         }
        //     }
        // }
        // [Button]
        // void MoveToCurrentSpawnPosition()
        // {
        //     if(string.IsNullOrEmpty(characterName))
        //         return;
        //     var spawnDatabase = SpawnDatabase.Instance;
        //     if (spawnDatabase.HasDefaultSpawnPosition(characterName))
        //     {
        //         var pos = spawnDatabase.GetSpawnPointForScene(characterName, savePath);
        //         if (transform.parent == null)
        //         {
        //             this.transform.position = pos;
        //         }
        //         else
        //         {
        //             transform.localPosition = pos;
        //         }
        //     }
        // }
        //

        // private void OnDrawGizmos()
        // {
        //     Gizmos.color = Color.red;
        //     Gizmos.DrawWireSphere(transform.position, 0.5f);
        // }
    }
}