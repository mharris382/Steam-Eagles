using System.Collections;
using CoreLib.Pickups;
using Cysharp.Threading.Tasks;
using Items;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
            yield return UniTask.ToCoroutine(async () => {
                var loadOp = Addressables.LoadAssetAsync<Pickup>("Hyperglass_pickup");
                await loadOp;
                pickup = loadOp.Result;
            });
            yield return AssertAssumePickupLoadsAndSpawns(key);
        }
        IEnumerable Start()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                var loadOp = Addressables.LoadAssetAsync<Pickup>("Hyperglass_pickup");
                await loadOp;
                pickup = loadOp.Result;
            });
        }
        
        IEnumerator AssertAssumePickupLoadsAndSpawns(string key)
        {
            yield return AssertPickupLoadsCorrectly(key);
            AssertPickupSpawnsCorrectly(key);
            yield return UniTask.ToCoroutine(async () =>
            {
                var item = await key.LoadItemAsync();
                Assert.NotNull(item);
                //because pickup was already loaded, we should not have to load it a second time and calling this should always return true
                Assert.IsTrue(item.GetPickup(out var p));
                //the item identified as the spawned pickup should then return the same pickup, verifying the pickup was spawned and identified correctly 
                Assert.AreEqual(p, pickup);
            });
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