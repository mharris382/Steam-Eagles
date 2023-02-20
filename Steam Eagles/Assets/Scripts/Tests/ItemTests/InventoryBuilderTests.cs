using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Items;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Tests.ItemTests
{
    public class InventoryBuilderTests
    {
        private const string InventoryName = "Test Inventory";
        
        [Test]
        public void BuildsEmptyInventoryCorrectly()
        {
            var slotCount = 10;
            var inventory = InventoryBuilder.BuildInventory(InventoryName, slotCount);
            Assert.NotNull(inventory);
            Assert.AreEqual(slotCount, inventory.GetEmptySlotCount());
            var slots = inventory.itemSlots.ToArray();
            Assert.AreEqual(slotCount, slots.Length);
            for (int i = 0; i < slotCount; i++)
            {
                Assert.NotNull(slots[i]);
                Assert.IsTrue(slots[i].IsEmpty);
            }
        }

        [UnityTest]
        public IEnumerator BuildsFromListCorrectly()
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
            Assert.AreEqual(7, stacks.Count);
            var inventory = InventoryBuilder.BuildInventory(InventoryName, stacks);
            Assert.NotNull(inventory);
            Assert.AreEqual(0, inventory.GetEmptySlotCount());
            var slots = inventory.itemSlots.ToArray();
            Assert.AreEqual(7, slots.Length);
            for (int i = 0; i < slots.Length; i++)
            {
                Assert.NotNull(slots[i]);
                Assert.IsFalse(slots[i].IsEmpty);
                Assert.AreEqual(stacks[i].item, slots[i].ItemStack.item);
                Assert.AreEqual(stacks[i].Count, slots[i].ItemStack.Count);
            }
        }
        
        
        [UnityTest]
        public IEnumerator BuildsPartiallyEmptyInventoryCorrectly()
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
            Assert.AreEqual(7, stacks.Count);
            int totalSlotCount = 10;
            int emptySlotCount = 3;
            
            var inventory = InventoryBuilder.BuildInventory(InventoryName, 10, stacks);
            Assert.NotNull(inventory);
            Assert.AreEqual(emptySlotCount, inventory.GetEmptySlotCount());
            
            var slots = inventory.itemSlots.ToArray();
            Assert.AreEqual(totalSlotCount, slots.Length);
            
        }
    }
}