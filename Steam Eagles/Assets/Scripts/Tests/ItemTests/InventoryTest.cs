using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Items;
using NUnit.Framework;
using UnityEngine;

namespace Tests.ItemTests
{
    public static class AssumedItemNames
    {
        public const string ITEM_SCRAP_METAL = "ScrapMetal";
        public const string ITEM_HYPERGLASS = "Hyperglass";
        public const string ITEM_COPPER_PIPE = "CopperPipe";


        public const string TOOL_SAW = "Saw";
        public const string TOOL_DRILL = "Drill";
        public const string TOOL_RECIPE_BOOK = "RecipeBook";
        public const string TOOL_FLASHLIGHT = "Flashlight";


        public static string[] Tools = new[]
        {
            TOOL_SAW,
            TOOL_DRILL,
            TOOL_RECIPE_BOOK,
            TOOL_FLASHLIGHT
        };

        public static string[] Items = new[]
        {
            ITEM_SCRAP_METAL,
            ITEM_HYPERGLASS,
            ITEM_COPPER_PIPE
        };
        
        public static string[] All = Tools.Concat(Items).ToArray();
        public static IEnumerator LoadAllItems()
        {
            foreach (var s in All)
            {
                var loadItem = s.LoadItem(t =>
                {
                    Assert.NotNull(t);
                    Debug.Log("Loaded " + s);
                });
            }

            foreach (var s in All)
            {
                while (!s.IsItemLoaded())
                {
                    yield return null;
                }
            }
        }
    }

    public class InventoryTest
    {
        private const string InventoryName = "Test Inventory";
        private Inventory _inventory;
        private const int InventorySize = 10;
        // A UnityTest behaves like a coroutine in PlayMode
        // and allows you to yield null to skip a frame in EditMode
        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator InventoryTestWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // yield to skip a frame
            yield return null;
        }
        public IEnumerator BuildFullyOccupiedInventory()
        {
            (string, int)[] stackableItemNames = new(string, int)[] {
                (AssumedItemNames.ITEM_COPPER_PIPE,2),
                (AssumedItemNames.ITEM_HYPERGLASS,5),
                (AssumedItemNames.ITEM_SCRAP_METAL,3),
                (AssumedItemNames.ITEM_SCRAP_METAL,3),
                (AssumedItemNames.ITEM_SCRAP_METAL,3),
                (AssumedItemNames.ITEM_SCRAP_METAL,3)
            };
            string[] toolNames = new string[] {
                AssumedItemNames.TOOL_SAW,
                AssumedItemNames.TOOL_DRILL,
                AssumedItemNames.TOOL_RECIPE_BOOK,
                AssumedItemNames.TOOL_FLASHLIGHT,
            };
            List<ItemStack> stacks = new List<ItemStack>();
            
            foreach (var stackableItemName in stackableItemNames)
            {
                yield return stackableItemName.Item1.LoadItem(t => {
                    var stack = new ItemStack(t, stackableItemName.Item2);
                    stacks.Add(stack);
                });
                
                var item = stackableItemName.Item1.GetItem();
                Assert.NotNull(item, "Item was not loaded!");
            }
            foreach (var toolName in toolNames)
            {
                yield return toolName.LoadItem(t => {
                    var stack = new ItemStack(t, 1);
                    stacks.Add(stack);
                });
                
                var item = toolName.GetItem();
                Assert.NotNull(item, "Item was not loaded!");   
            }
            BuildFromStacks(stacks);
        }
        public IEnumerator BuildNonEmptyInventory()
        {
            (string, int)[] stackableItemNames = new(string, int)[] {
                (AssumedItemNames.ITEM_COPPER_PIPE,2),
                (AssumedItemNames.ITEM_HYPERGLASS,5),
                (AssumedItemNames.ITEM_SCRAP_METAL,3)
            };
            string[] toolNames = new string[] {
                AssumedItemNames.TOOL_SAW,
                AssumedItemNames.TOOL_DRILL,
                AssumedItemNames.TOOL_RECIPE_BOOK,
                AssumedItemNames.TOOL_FLASHLIGHT,
            };
            int[] stackCounts = new int[] {
                11,
                22,
                30
            };
            List<ItemStack> stacks = new List<ItemStack>();
            
            foreach (var stackableItemName in stackableItemNames)
            {
                yield return stackableItemName.Item1.LoadItem(t => {
                    var stack = new ItemStack(t, stackableItemName.Item2);
                    stacks.Add(stack);
                });
                
                var item = stackableItemName.Item1.GetItem();
                Assert.NotNull(item, "Item was not loaded!");
            }

            foreach (var toolName in toolNames)
            {
                yield return toolName.LoadItem(t => {
                    var stack = new ItemStack(t, 1);
                    stacks.Add(stack);
                });
                
                var item = toolName.GetItem();
                Assert.NotNull(item, "Item was not loaded!");   
            }
            BuildFromStacks(stacks);
        }

        public void BuildEmptyInventory()
        {
            _inventory = InventoryBuilder.BuildInventory(InventoryName, InventorySize);
            Assert.AreEqual(InventorySize, _inventory.GetEmptySlotCount());
            var slots = _inventory.itemSlots.ToArray();
            Assert.AreEqual(InventorySize, slots.Length);
            for (int i = 0; i < InventorySize; i++)
            {
                Assert.IsTrue(slots[i].IsEmpty);
            }
        }
         private void BuildFromStacks(List<ItemStack> stacks)
         {
             Assert.AreEqual(7, stacks.Count);
             var inventory = InventoryBuilder.BuildInventory(InventoryName, InventorySize, stacks);
             Assert.NotNull(inventory);
             Assert.AreEqual(0, inventory.GetEmptySlotCount());
             var slots = inventory.itemSlots.ToArray();
             Assert.AreEqual(InventorySize, slots.Length);
             for (int i = 0; i < slots.Length; i++)
             {
                 Assert.NotNull(slots[i]);
                 Assert.IsFalse(slots[i].IsEmpty);
                 Assert.AreEqual(stacks[i].item, slots[i].ItemStack.item);
                 Assert.AreEqual(stacks[i].Count, slots[i].ItemStack.Count);
             }

             _inventory = inventory;
         }
    }
}