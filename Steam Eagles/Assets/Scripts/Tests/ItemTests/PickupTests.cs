using System.Collections;
using CoreLib.Pickups;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.ItemTests
{
    public class PickupTests
    {
        Pickup pickup;
        PickupBody pickupBody;
        
        
        [UnityTest]
        public IEnumerator ScrapMetalPickupIsSpawnedCorrectly()
        {
            var key = AssumedItemNames.ITEM_SCRAP_METAL;
            yield return AssertAssumePickupLoadsAndSpawns(key);
        }
        
        [UnityTest]
        public IEnumerator CopperPipesPickupIsSpawnedCorrectly()
        {
            var key = AssumedItemNames.ITEM_COPPER_PIPE;
            yield return AssertAssumePickupLoadsAndSpawns(key);
        }
        
        [UnityTest]
        public IEnumerator HyperglassPickupIsSpawnedCorrectly()
        {
            var key = AssumedItemNames.ITEM_HYPERGLASS;
            yield return AssertAssumePickupLoadsAndSpawns(key);
        }

        
        IEnumerator AssertAssumePickupLoadsAndSpawns(string key)
        {
            yield return AssertPickupLoadsCorrectly(key);
            AssertPickupSpawnsCorrectly(key);
        }

        void AssertPickupSpawnsCorrectly(string key)
        {
            pickupBody = pickup.SpawnPickup(Vector3.zero);
            Assert.NotNull(pickupBody);
            var pickupId = pickupBody.GetComponent<PickupID>();
            Assert.NotNull(pickupId);
            Assert.IsTrue(pickupId.HasKeyBeenAssigned);
            Assert.AreEqual(key, pickupId.Key);
        }

        IEnumerator AssertPickupLoadsCorrectly(string key)
        {
            PickupLoader.Instance.LoadPickup(key);
            while(PickupLoader.Instance.IsPickupLoaded(key) == false)
            {
                yield return null;
            }
            pickup = PickupLoader.Instance.GetPickup(key);
            Assert.NotNull(pickup);
        }
        
        
    }
}