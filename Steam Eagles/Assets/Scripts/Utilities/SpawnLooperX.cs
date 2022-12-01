using System;
using System.Collections;
using DG.DemiEditor;
using NaughtyAttributes;
using UnityEngine;

namespace Utilities
{
    public class SpawnLooperX : MonoBehaviour
    {
        public float xLimit = 100;

        [Required]
        public Spawnable targetObject;

        
        public Transform spawnPosition;
        
        private Spawnable _prefab;
        private Spawnable _instance;
        
        private void Awake()
        {
            if (targetObject == null) return;
            _prefab = targetObject;
            _prefab.gameObject.SetActive(false);
            Respawn();
            if (spawnPosition == null)
            {
                var go = new GameObject("SpawnPosition");
                spawnPosition = go.transform;
                spawnPosition.position = targetObject.transform.position;
                spawnPosition.rotation = targetObject.transform.rotation;
            }
        }

        Spawnable Respawn()
        {
            _prefab.gameObject.SetActive(true);
            _instance = Instantiate(_prefab, spawnPosition.position, spawnPosition.rotation);
            _prefab.gameObject.SetActive(false);
            return _instance;
        }

        public IEnumerator Start()
        {
            var limit = _instance.trackPosition.position.x + xLimit;
            while (enabled)
            {
                yield return null;
                var xPos = _instance.trackPosition.position.x;
                if (xPos > limit)
                {
                    Destroy(_instance.gameObject);
                    _instance = Respawn();
                }
            }
        }
        
        private void OnDrawGizmos()
        {
            if(targetObject == null) return;
            var pos = transform.position;
            var limit = pos;
            limit.x += xLimit;
            var line0 = limit;
            var line1 = limit;
            line0.y += 100;
            line1.y -= 100;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(line0, line1);
            Gizmos.color = Color.red.SetAlpha(0.78f);
            Gizmos.DrawSphere(targetObject.trackPosition.position, 1);
        }
    }
}