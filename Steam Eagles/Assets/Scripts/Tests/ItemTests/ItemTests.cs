using System.Collections;
using CoreLib.Pickups;
using Cysharp.Threading.Tasks;
using Items;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Tests.ItemTests
{
    public class ItemTests
    {
        private ItemBase item;


        private string[] itemNames = new string[]
        {
            AssumedItemNames.ITEM_SCRAP_METAL,
            AssumedItemNames.ITEM_COPPER_PIPE,
            AssumedItemNames.ITEM_HYPERGLASS
        };


        [UnityTest]
        public IEnumerator ItemsHaveCorrespondingPickups()
        {
            
            foreach (var itemName in itemNames)
            {
                yield return AssertItemLoadsCorrectly(itemName);
                ItemHasAllValues(itemName);
                yield return RetrievesPickupCorrectly(item);
            }
        }

        [UnityTest]
        public IEnumerator ItemsLoadInParallelCorrectly()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var items =await ItemLoader.Instance.LoadItemsParallel(
                    AssumedItemNames.ITEM_HYPERGLASS, 
                    AssumedItemNames.ITEM_COPPER_PIPE, 
                    AssumedItemNames.ITEM_SCRAP_METAL);
                Assert.AreEqual(AssumedItemNames.ITEM_HYPERGLASS, items.item1.dropKey);
                Assert.AreEqual(AssumedItemNames.ITEM_COPPER_PIPE, items.item2.dropKey);
                Assert.AreEqual(AssumedItemNames.ITEM_SCRAP_METAL, items.item3.dropKey);
                ItemHasAllValues(items.item1);
                ItemHasAllValues(items.item2);
                ItemHasAllValues(items.item3);
            });
        }
        
        [UnityTest]
        public IEnumerator ScrapMetalItemLoadsCorrectly()
        {
            yield return AssertItemLoadsCorrectly(AssumedItemNames.ITEM_SCRAP_METAL);
        }
        [UnityTest]
        public IEnumerator CopperPipesItemLoadsCorrectly()
        {
            yield return AssertItemLoadsCorrectly(AssumedItemNames.ITEM_COPPER_PIPE);
        }
        [UnityTest]
        public IEnumerator HyperglassItemLoadsCorrectly()
        {
            yield return AssertItemLoadsCorrectly(AssumedItemNames.ITEM_HYPERGLASS);
        }
        
        [UnityTest]
        public IEnumerator HyperglassItemLoadsAsyncCorrectly()
        {
            return UniTask.ToCoroutine(async () =>
            {
                item = await ItemLoader.Instance.LoadItemAsync(AssumedItemNames.ITEM_HYPERGLASS);
                ItemHasAllValues(item);
            });
        }

        [UnityTest]
        public IEnumerator ScrapMetalLoadsAsyncCorrectly()
        {
            return UniTask.ToCoroutine(async () =>
            {
                item = await ItemLoader.Instance.LoadItemAsync(AssumedItemNames.ITEM_SCRAP_METAL);
                ItemHasAllValues(item);
            });
        }
        
        [UnityTest]
        public IEnumerator CopperPipeLoadsAsyncCorrectly()
        {
            return UniTask.ToCoroutine(async () =>
            {
                item = await ItemLoader.Instance.LoadItemAsync(AssumedItemNames.ITEM_COPPER_PIPE);
                ItemHasAllValues(item);
            });
        }
        
        [UnityTest]
        public IEnumerator ScrapMetalHasPickup()
        {
            yield return AssertItemLoadsCorrectly(AssumedItemNames.ITEM_SCRAP_METAL);
            yield return RetrievesPickupCorrectly(item);
        }
        [UnityTest]
        public IEnumerator CopperPipesHasPickup()
        {
            yield return AssertItemLoadsCorrectly(AssumedItemNames.ITEM_COPPER_PIPE);
            yield return RetrievesPickupCorrectly(item);
        }
        [UnityTest]
        public IEnumerator HyperglassHasPickup()
        {
            yield return AssertItemLoadsCorrectly(AssumedItemNames.ITEM_HYPERGLASS);
            yield return RetrievesPickupCorrectly(item);
        }
        [UnityTest]
        public IEnumerator ScrapMetalItemHasAllValues()
        {
            yield return AssertItemLoadsCorrectly(AssumedItemNames.ITEM_SCRAP_METAL);
            ItemHasAllValues(AssumedItemNames.ITEM_SCRAP_METAL);
        }
        [UnityTest]
        public IEnumerator CopperPipesItemHasAllValues()
        {
            yield return AssertItemLoadsCorrectly(AssumedItemNames.ITEM_COPPER_PIPE);
            ItemHasAllValues(AssumedItemNames.ITEM_COPPER_PIPE);
        }
        [UnityTest]
        public IEnumerator HyperglassItemHasAllValues()
        {
            yield return AssertItemLoadsCorrectly(AssumedItemNames.ITEM_HYPERGLASS);
            string itemName = AssumedItemNames.ITEM_HYPERGLASS;
            ItemHasAllValues(itemName);
        }

        private void ItemHasAllValues(string itemName)
        {
            Assert.IsNotEmpty(item.description, $"{itemName} has no description", item);
            Assert.IsNotEmpty(item.itemName, $"{itemName} has no name", item);
            Assert.IsNotEmpty(item.lore, $"{itemName} has no lore", item);
        }

        private void ItemHasAllValues(ItemBase itemBase)
        {
            Assert.NotNull(itemBase);
            string itemName = itemBase.dropKey;
            Assert.IsNotEmpty(itemBase.description, $"{itemName} has no description", item);
            Assert.IsNotEmpty(itemBase.itemName, $"{itemName} has no name", item);
            Assert.IsNotEmpty(itemBase.lore, $"{itemName} has no lore", item);
        }


        private IEnumerator RetrievesPickupCorrectly(ItemBase i)
        {
            var key = i.GetItemKey();
            Assert.NotNull(key);
            yield return UniTask.ToCoroutine(async () =>
            {
                var pickup = await PickupLoader.Instance.LoadPickupAsync(key);
                Assert.NotNull(pickup);
            });
        }

        IEnumerator AssertItemLoadsCorrectly(string key)
        {
            return UniTask.ToCoroutine(async () =>
            {
                var item = await ItemLoader.Instance.LoadItemAsync(key);
                Assert.IsNotNull(item, $"Item {key} is null", item);
                this.item = item;
            });
        }

       
    }
}