using System;
using System.Collections;
using UnityEngine;

namespace CoreLib.Pickups
{
    public class PickupTester : MonoBehaviour
    {
        [System.Serializable]
        public class PickupSpawnTest
        {
            public string key;
            public Vector3 spawnPosition;
            public float delay;
            public int count = 1;
        }

        public PickupSpawnTest[] tests;

        private void Start()
        {
            foreach (var test in tests)
            {
                StartCoroutine(RunTest(test));
            }
        }

        IEnumerator RunTest(PickupSpawnTest test)
        {
            PickupLoader.Instance.LoadPickup(test.key);
            while (!PickupLoader.Instance.IsPickupLoaded(test.key))
            {
                Debug.Log($"Waiting for pickup {test.key} to load");
                yield return null;
            }

            yield return new WaitForSeconds(test.delay);
            var pos = test.spawnPosition;
            var pickup = PickupLoader.Instance.GetPickup(test.key);
            

        }
    }
}