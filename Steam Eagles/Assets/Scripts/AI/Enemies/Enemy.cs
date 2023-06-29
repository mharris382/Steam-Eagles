using System;
using CoreLib;
using UnityEngine;

namespace AI.Enemies.Installers
{
    [RequireComponent(typeof(Health), typeof(EnemyAIContext))]
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private Config config;
        private Health _health;
        public Config EnemyConfig => config;
        
        private void Awake()
        {
            _health = GetComponent<Health>();
        }
        
        [Serializable]
        public class Config : ConfigBase
        {
            public float maxMoveSpeed = 10;
            public float targetSwitchInterval = 1;
            public float maxEngagementDistance = 10;
        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            var pos = transform.position;
            Gizmos.DrawWireSphere(pos, config.maxEngagementDistance);
        }
    }
}