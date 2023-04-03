using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoreLib.SaveLoad;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Items;
using Items.SaveLoad;
using SaveLoad;
using UnityEngine.TestTools;


namespace Tests.ItemTests.SaveLoad
{
    
    public class InventorySaveTests
    {
        private const string SAVE_PATH = "Automated Testing Save Path";

        private const string TEST_GROUP_ID = "Builder";
        private const string TEST_UNIQUE_ID = "Backpack";
        private const string TEST_UNIQUE_ID_2 = "ToolBelt";

        private InventorySaves _initialLoadState;
        
        private static string[] ToolBeltTestItemSave => new string[] {
            AssumedItemNames.RECIPE_TOOL,
            AssumedItemNames.BUILD_TOOL,
            AssumedItemNames.REPAIR_TOOL,
            AssumedItemNames.DESTRUCT_TOOL
        };
        

        /// <summary>
        /// exposes two ids for logical grouping in class hierarchy
        /// <see cref="PersistentInventoryBase"/>
        /// <seealso cref="CharacterInventoryBase"/>
        /// </summary>
        private static readonly string TestKeyBackpack = $"{TEST_GROUP_ID}_{TEST_UNIQUE_ID}";
        private static string TestKeyToolBelt = $"{TEST_GROUP_ID}_{TEST_UNIQUE_ID_2}";


        [SetUp]
        public void SetUp()
        {
            PersistenceManager.Instance.Initialize(SAVE_PATH);
            _initialLoadState = InventorySaveLoader.LoadedInventorySave;
        }


        [TearDown]
        public void TearDown()
        {
            InventorySaveLoader.UnloadInventorySave();
            _initialLoadState = null;
        }


        [Test]
        public void LoadedInitialState()
        {
            Assert.NotNull(_initialLoadState);
            
        }

        [Test]
        public void CanRetrieveBuilderBackpackSaveState()
        {
            var backpackSave = _initialLoadState[TestKeyBackpack];
            Assert.NotNull(backpackSave);
        }
        
        [Test]
        public void CanRetrieveBuilderToolBeltSaveState()
        {
            var toolBeltSave = _initialLoadState[TestKeyToolBelt];
            Assert.NotNull(toolBeltSave);
        }

        [UnityTest]
        public  IEnumerator CanSaveInventoryFromStacks()
        {
            const int hgStackCount = 2;
            const int copStackCount = 4;
            const int smStackCount = 8;
            
            const int hgStackSize = 1;
            const int copStackSize = 10;
            const int smStackSize = 15;
            
            int totalStacks = hgStackCount + copStackCount + smStackCount;
            
            var hgRequest = new SimpleStackBuildRequest()
            {
                stackCount = hgStackCount,
                stackSize = hgStackSize
            };
            var copRequest = new SimpleStackBuildRequest()
            {
                stackCount = copStackCount,
                stackSize = copStackSize
            };
            var smRequest = new SimpleStackBuildRequest()
            {
                stackCount = smStackCount,
                stackSize = smStackSize
            };
            return UniTask.ToCoroutine(async () => {
                var stacks = await LoadAndBuildStacks(hgRequest, copRequest, smRequest);
                int hgStacksFound = stacks.Count(t => t.item.GetItemKey() == AssumedItemNames.ITEM_HYPERGLASS);
                int copStacksFound = stacks.Count(t => t.item.GetItemKey() == AssumedItemNames.ITEM_COPPER_PIPE);
                int smStacksFound = stacks.Count(t => t.item.GetItemKey() == AssumedItemNames.ITEM_SCRAP_METAL);
                Assert.AreEqual(hgStackCount, hgStacksFound);
                Assert.AreEqual(copStackCount, copStacksFound);
                Assert.AreEqual(smStackCount, smStacksFound);
                var hgStacks =stacks.Where(t => t.item.GetItemKey() == AssumedItemNames.ITEM_HYPERGLASS).ToList();
                foreach (var itemStack in hgStacks)
                {
                    Assert.AreEqual(hgStackSize, itemStack.Count);
                }
                var copStacks =stacks.Where(t => t.item.GetItemKey() == AssumedItemNames.ITEM_COPPER_PIPE).ToList();
                foreach (var itemStack in copStacks)
                {
                    Assert.AreEqual(copStackSize, itemStack.Count);
                }
                var smStacks =stacks.Where(t => t.item.GetItemKey() == AssumedItemNames.ITEM_SCRAP_METAL).ToList();
                foreach (var itemStack in smStacks)
                {
                    Assert.AreEqual(smStackSize, itemStack.Count);
                }
            });
        }

        [UnityTest]
        public IEnumerator CanSaveToolBeltFromStacks()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var toolBeltItems = await LoadToolBeltState();
                Assert.AreEqual(4, toolBeltItems.Count);
                
                foreach (var toolBeltItem in toolBeltItems)
                {
                    Assert.NotNull(toolBeltItem.Item);
                    Assert.IsTrue(toolBeltItem.Item is Tool);
                }
                
                Assert.AreEqual(AssumedItemNames.RECIPE_TOOL, toolBeltItems[0].Key);
                Assert.AreEqual(AssumedItemNames.BUILD_TOOL, toolBeltItems[1].Key);
                Assert.AreEqual(AssumedItemNames.REPAIR_TOOL, toolBeltItems[2].Key);
                Assert.AreEqual(AssumedItemNames.DESTRUCT_TOOL, toolBeltItems[3].Key);
            });
        }
        
        private struct SimpleStackBuildRequest
        {
            public int stackCount;
            public int stackSize;
        }
        
        private async UniTask<List<ItemStack>> LoadAndBuildStacks(
            SimpleStackBuildRequest hyperglass,
            SimpleStackBuildRequest copperPipes,
            SimpleStackBuildRequest scrapMetal,
            int emptyRequested = 0)
        {
            var items = await ItemLoader.Instance.LoadItemsParallel(
                AssumedItemNames.ITEM_HYPERGLASS, 
                AssumedItemNames.ITEM_COPPER_PIPE, 
                AssumedItemNames.ITEM_SCRAP_METAL);
                
            Assert.NotNull(items.item1);
            Assert.NotNull(items.item2);
            Assert.NotNull(items.item3);
            var stacks = new List<ItemStack>();
            for (int i = 0; i < hyperglass.stackCount; i++)
            {
                stacks.Add(new ItemStack(items.item1, hyperglass.stackSize));
            }

            for (int i = 0; i < copperPipes.stackCount; i++)
            {
                stacks.Add(new ItemStack(items.item2, copperPipes.stackSize));
            }
            
            for (int i = 0; i < scrapMetal.stackCount; i++)
            {
                stacks.Add(new ItemStack(items.item3, scrapMetal.stackSize));
            }

            for (int i = 0; i < emptyRequested; i++)
            {
                stacks.Add(ItemStack.Empty);
            }
            return stacks;
        }

        private async UniTask<List<ItemStack>> LoadToolBeltState(
            string recipeTool = AssumedItemNames.RECIPE_TOOL, 
            string buildTool = AssumedItemNames.BUILD_TOOL, 
            string repairTool = AssumedItemNames.REPAIR_TOOL,
            string destructTool = AssumedItemNames.DESTRUCT_TOOL)
        {
            var items = await ItemLoader.Instance.LoadItemsParallel(
                recipeTool, 
                buildTool, 
                repairTool,
                destructTool);
            var list = new List<ItemStack>();
            list.Add(items.item1);
            list.Add(items.item2);
            list.Add(items.item3);
            list.Add(items.item4);
            return list;
        }
        public IEnumerator CanSave()
        {
            throw new NotImplementedException();
        }
    }
}