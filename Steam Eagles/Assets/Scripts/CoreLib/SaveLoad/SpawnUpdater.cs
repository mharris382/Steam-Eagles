using System;
using UnityEngine;

namespace CoreLib.SaveLoad
{
    public class SpawnUpdater : MonoBehaviour
    {
        public string characterName;

        private void Awake()
        {
            SpawnDatabase.Instance.RegisterDynamicSpawn(characterName, transform);
        }

        private void OnDestroy()
        {
            SpawnDatabase.Instance.RemoveDynamicSpawn(characterName);
        }
    }
}