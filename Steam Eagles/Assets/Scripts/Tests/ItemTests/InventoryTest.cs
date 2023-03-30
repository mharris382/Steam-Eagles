using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Items;
using NUnit.Framework;
using UnityEngine;
using Rand = UnityEngine.Random;
using UnityEngine.TestTools;

namespace Tests.ItemTests
{
    public static class AssumedItemNames
    {
        public const string ITEM_SCRAP_METAL = "ScrapMetal";
        public const string ITEM_HYPERGLASS = "Hyperglass";
        public const string ITEM_COPPER_PIPE = "CopperPipe";


        [System.Obsolete("Use DestructTool")]
        public const string TOOL_SAW = "Saw";
        
        [System.Obsolete("Use DestructTool")]
        public const string TOOL_DRILL = "Drill";
        
        [System.Obsolete("Use RecipeTool")]
        public const string TOOL_RECIPE_BOOK = "RecipeBook";
        
        [System.Obsolete("Tool Not Implemented Yet")]
        public const string TOOL_FLASHLIGHT = "Flashlight";

        public const string BUILD_TOOL = "BuildTool";
        public const string RECIPE_TOOL = "RecipeTool";
        public const string REPAIR_TOOL = "RepairTool";
        public const string DESTRUCT_TOOL = "DestructTool";

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
    }



    public class InventorySlotTests
    {
        private const string InventoryName = "Test Inventory";
        private Inventory _inventory;
        private InventorySlot[] _slots;
        private const int InventorySize = 10;

        private ItemBase _scrapMetal;
        private ItemBase _hyperglass;
        private ItemBase _copperPipe;
        private ItemBase _flashlight;



        public IEnumerator BuildFullyOccupiedInventory()
        {
            (string, int)[] stackableItemNames = new (string, int)[]
            {
                (AssumedItemNames.ITEM_COPPER_PIPE, 2),
                (AssumedItemNames.ITEM_HYPERGLASS, 5),
                (AssumedItemNames.ITEM_SCRAP_METAL, 3),
                (AssumedItemNames.ITEM_SCRAP_METAL, 3),
                (AssumedItemNames.ITEM_SCRAP_METAL, 3),
                (AssumedItemNames.ITEM_SCRAP_METAL, 3)
            };
            string[] toolNames = new string[]
            {
                AssumedItemNames.TOOL_SAW,
                AssumedItemNames.TOOL_DRILL,
                AssumedItemNames.TOOL_RECIPE_BOOK,
                AssumedItemNames.TOOL_FLASHLIGHT,
            };
            List<ItemStack> stacks = new List<ItemStack>();

            foreach (var stackableItemName in stackableItemNames)
            {
                yield return stackableItemName.Item1.LoadItem(t =>
                {
                    var stack = new ItemStack(t, stackableItemName.Item2);
                    stacks.Add(stack);
                });

                var item = stackableItemName.Item1.GetItem();
                Assert.NotNull(item, "Item was not loaded!");
            }

            foreach (var toolName in toolNames)
            {
                yield return toolName.LoadItem(t =>
                {
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
            (string, int)[] stackableItemNames = new (string, int)[]
            {
                (AssumedItemNames.ITEM_COPPER_PIPE, 2),
                (AssumedItemNames.ITEM_HYPERGLASS, 5),
                (AssumedItemNames.ITEM_SCRAP_METAL, 3)
            };
            string[] toolNames = new string[]
            {
                AssumedItemNames.TOOL_SAW,
                AssumedItemNames.TOOL_DRILL,
                AssumedItemNames.TOOL_RECIPE_BOOK,
                AssumedItemNames.TOOL_FLASHLIGHT,
            };
            List<ItemStack> stacks = new List<ItemStack>();
            foreach (var stackableItemName in stackableItemNames)
            {
                yield return stackableItemName.Item1.LoadItem(t =>
                {
                    var stack = new ItemStack(t, stackableItemName.Item2);
                    stacks.Add(stack);
                });

                var item = stackableItemName.Item1.GetItem();
                Assert.NotNull(item, "Item was not loaded!");
            }

            foreach (var toolName in toolNames)
            {
                yield return toolName.LoadItem(t =>
                {
                    var stack = new ItemStack(t, 1);
                    stacks.Add(stack);
                });

                var item = toolName.GetItem();
                Assert.NotNull(item, "Item was not loaded!");
            }

            BuildFromStacks(stacks);
        }

        public IEnumerator LoadResourceItems()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                var (a, b, c) = await ItemLoader.Instance.LoadItemsParallel(
                    AssumedItemNames.ITEM_COPPER_PIPE,
                    AssumedItemNames.ITEM_SCRAP_METAL,
                    AssumedItemNames.ITEM_HYPERGLASS);
                Assert.NotNull(_copperPipe = a);
                Assert.NotNull(_scrapMetal = b);
                Assert.NotNull(_hyperglass = c);
            });
        }

        public IEnumerator LoadResourceItemsAndBuildTool()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                var (a, b, c, d) = await ItemLoader.Instance.LoadItemsParallel(
                    AssumedItemNames.ITEM_COPPER_PIPE,
                    AssumedItemNames.ITEM_SCRAP_METAL,
                    AssumedItemNames.ITEM_HYPERGLASS,
                    AssumedItemNames.BUILD_TOOL);
                Assert.NotNull(_copperPipe = a);
                Assert.NotNull(_scrapMetal = b);
                Assert.NotNull(_hyperglass = c);
                Assert.NotNull(_flashlight = d);
            });
        }
        public void BuildEmptyInventory()
        {
            _inventory = InventoryBuilder.BuildInventory(InventoryName, InventorySize);
            Assert.AreEqual(InventorySize, _inventory.GetEmptySlotCount());
            _slots = _inventory.itemSlots.ToArray();
            Assert.AreEqual(InventorySize, _slots.Length);
            for (int i = 0; i < InventorySize; i++)
                Assert.IsTrue(_slots[i].IsEmpty);
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

        private InventorySlot AddItemToFirstEmptySlot(ItemBase itemBase, int amount)
        {
            foreach (var inventorySlot in _slots)
            {
                if (inventorySlot.IsEmpty)
                {
                    inventorySlot.SetItemStack(itemBase, amount);
                    return inventorySlot;
                }
            }

            return null;
        }

        private InventorySlot AddScrapMetalToFirstEmptySlot(int amount) => AddItemToFirstEmptySlot(_scrapMetal, amount);

        private InventorySlot AddHyperglassToFirstEmptySlot(int amount)
        {
            if (amount > _hyperglass.MaxStackSize)
            {
                amount = _hyperglass.MaxStackSize;
            }
            return AddItemToFirstEmptySlot(_hyperglass, amount);
        }
        private InventorySlot AddCopperPipeToFirstEmptySlot(int amount) => AddItemToFirstEmptySlot(_copperPipe, amount);

        private InventorySlot AddFlashlightToFistEmptySlot() => AddItemToFirstEmptySlot(_flashlight, 1);
        private InventorySlot FindFirstEmptySlot()
        {
            foreach (var inventorySlot in _slots)
            {
                if (inventorySlot.IsEmpty)
                    return inventorySlot;
            }

            return null;
        }

        private InventorySlot FindFirstNonEmptySlot()
        {
            foreach (var inventorySlot in _slots)
            {
                if (!inventorySlot.IsEmpty)
                    return inventorySlot;
            }

            return null;
        }

        private InventorySlot FindRandomEmptySlot()
        {
            var info = new InventorySlotsInfo(_slots);
            var cnt = info.EmptySlots.Count;
            if (cnt == 0)
                return null;
            var index = Rand.Range(0, cnt - 1);
            return info.EmptySlots[index];
        }

        private InventorySlot FindRandomNonEmptySlot()
        {
            var info = new InventorySlotsInfo(_slots);
            var cnt = info.NonEmptySlots.Count;
            if (cnt == 0)
                return null;
            var index = Rand.Range(0, cnt - 1);
            return info.NonEmptySlots[index];
        }

        [UnityTest]
        public IEnumerator CanFindSlotsCorrectly()
        {
            BuildEmptyInventory();
            yield return LoadResourceItems();
            AddScrapMetalToFirstEmptySlot(10);
            AddScrapMetalToFirstEmptySlot(10);
            AddScrapMetalToFirstEmptySlot(10);
            var firstEmptyExpected = _slots[3];
            var firstNonEmptyExpected = _slots[0];
            var validNonEmpty = new[]
            {
                _slots[0],
                _slots[1],
                _slots[2]
            };
            List<InventorySlot> validEmpty = new List<InventorySlot>();
            for (int i = 3; i < _slots.Length; i++) validEmpty.Add(_slots[i]);

            var randEmpty  = FindRandomEmptySlot();
            var randNonEmpty = FindRandomNonEmptySlot();
            Assert.NotNull(randEmpty);
            Assert.NotNull(randNonEmpty);
            
            Assert.IsTrue(validEmpty.Contains(randEmpty));
            Assert.IsFalse(validEmpty.Contains(randNonEmpty));
            
            Assert.IsTrue(validNonEmpty.Contains(randNonEmpty));
            Assert.IsFalse(validNonEmpty.Contains(randEmpty));

            var firstEmpty = FindFirstEmptySlot();
            var firstNonEmpty = FindFirstNonEmptySlot();
            Assert.AreEqual(firstEmptyExpected, firstEmpty);
            Assert.AreEqual(firstNonEmptyExpected, firstNonEmpty);
            
        }

         [UnityTest]
         public IEnumerator MergesSlotsCorrectly()
         {
             BuildEmptyInventory();
             yield return LoadResourceItems();
             AddScrapMetalToFirstEmptySlot(10);
             AddScrapMetalToFirstEmptySlot(10);
             AddScrapMetalToFirstEmptySlot(10);
             var info = new InventorySlotsInfo(_slots);
             var slotsWithScrapMetal = info.GetSlotsContainingItem(_scrapMetal).ToList();
             Assert.AreEqual(3, slotsWithScrapMetal.Count);
             foreach (var inventorySlot in slotsWithScrapMetal)
             {
                    Assert.AreEqual(_scrapMetal, inventorySlot.ItemStack.item);
                    Assert.AreEqual(10, inventorySlot.ItemStack.Count);
             }
             _inventory.MergeStacks();
            info = new InventorySlotsInfo(_slots);
            slotsWithScrapMetal = info.GetSlotsContainingItem(_scrapMetal).ToList();
            Assert.AreEqual(1, slotsWithScrapMetal.Count);
            var slot = slotsWithScrapMetal[0];
            Assert.AreEqual(_scrapMetal, slot.ItemStack.item);
            Assert.AreEqual(30, slot.ItemStack.Count);
         }

         [UnityTest]
         public IEnumerator SortsSameItemTypeByNameCorrectly()
         {
             BuildEmptyInventory();
             yield return LoadResourceItems();
            

            AddHyperglassToFirstEmptySlot(2);
            AddCopperPipeToFirstEmptySlot(20);
            AddScrapMetalToFirstEmptySlot(10);
            AddCopperPipeToFirstEmptySlot(10);
             var expectedStartOrder = new[]
             {
                _hyperglass,
                _copperPipe,
                _scrapMetal,
                _copperPipe
             };
             var expectedSortedOrder = new[]
             {
                 _copperPipe,
                 _copperPipe,
                 _hyperglass,
                 _scrapMetal
             };
             _slots = _inventory.itemSlots.ToArray();
             for (int i = 0; i < expectedStartOrder.Length; i++)
             {
                 Assert.AreEqual(expectedStartOrder[i], _slots[i].ItemStack.item);
             }
             _inventory.SortInventory();
             _slots = _inventory.itemSlots.ToArray();
             for (int i = 0; i < expectedSortedOrder.Length; i++)
             {
                 Assert.AreEqual(expectedSortedOrder[i], _slots[i].ItemStack.item);
             }
         }
         
         [UnityTest]
         public IEnumerator SortsDifferentItemTypesCorrectly()
         {
             BuildEmptyInventory();
             yield return LoadResourceItemsAndBuildTool();
            

             AddHyperglassToFirstEmptySlot(20);
             AddCopperPipeToFirstEmptySlot(20);
             AddScrapMetalToFirstEmptySlot(10);
             AddFlashlightToFistEmptySlot();
             AddCopperPipeToFirstEmptySlot(10);
             var expectedStartOrder = new[]
             {
                 _hyperglass,
                 _copperPipe,
                 _scrapMetal,
                 _flashlight,
                 _copperPipe
             };
             var expectedSortedOrder = new[]
             {
                 _flashlight,
                 _copperPipe,
                 _copperPipe,
                 _hyperglass,
                 _scrapMetal
             };
             _slots = _inventory.itemSlots.ToArray();
             for (int i = 0; i < expectedStartOrder.Length; i++)
             {
                 Assert.AreEqual(expectedStartOrder[i], _slots[i].ItemStack.item);
             }
             _inventory.SortInventory();
             _slots = _inventory.itemSlots.ToArray();
             for (int i = 0; i < expectedSortedOrder.Length; i++)
             {
                 Assert.AreEqual(expectedSortedOrder[i], _slots[i].ItemStack.item);
             }
         }

         [UnityTest]
         public IEnumerator SortsEmptyStacksLast()
         {
             BuildEmptyInventory();
             yield return LoadResourceItemsAndBuildTool();

             var nonEmpty = _slots[^3];
             nonEmpty.SetItemStack(_flashlight, 1);
             List<ItemBase> expectedOutput = new List<ItemBase>();
             expectedOutput.Add(_flashlight);
             for (int i = 0; i < InventorySize-1; i++)
             {
                 expectedOutput.Add(null);
             }
             _slots = _inventory.itemSlots.ToArray();
             Assert.IsNull(_slots[0].ItemStack.item);
             _slots.Log();
             Debug.Log("Sorting Inventory");
             _inventory.SortInventory();
             Debug.Log("Sorted Inventory");
             _slots = _inventory.itemSlots.ToArray();
             _slots.Log();
             Assert.IsNotNull(_slots[0].ItemStack.item);
             Assert.AreEqual(_flashlight, _slots[0].ItemStack.item);
             Assert.AreEqual(expectedOutput.Count,_slots.Length);
             for (int i = 0; i < expectedOutput.Count; i++)
             {
                 Assert.AreEqual(expectedOutput[i], _slots[i].ItemStack.item);
             }
         }
         
         [UnityTest]
         public IEnumerator SwapSlotsCorrectly()
         {
             BuildEmptyInventory();
             yield return LoadResourceItems();
             AddScrapMetalToFirstEmptySlot(10);
             AddScrapMetalToFirstEmptySlot(10);
             AddScrapMetalToFirstEmptySlot(10);
             
             var s0 = FindFirstEmptySlot();
             var s1 = FindFirstNonEmptySlot();
             Assert.NotNull(s0);
             Assert.NotNull(s1);
             Assert.IsTrue(s0.IsEmpty);
             Assert.IsFalse(s1.IsEmpty);
             var stack0 = s0.ItemStack;
             var stack1 = s1.ItemStack;
             s0.SwapSlots(s1);
             Assert.IsFalse(s0.IsEmpty);
             Assert.IsTrue(s1.IsEmpty);
             
             Assert.AreEqual(stack0, s1.ItemStack);
             Assert.AreEqual(stack1, s0.ItemStack);
         }

         [UnityTest]
         public IEnumerator InventorySlotInfoIsCorrect()
         {
             BuildEmptyInventory();
             yield return LoadResourceItems();
             
             AddScrapMetalToFirstEmptySlot(10);
             AddScrapMetalToFirstEmptySlot(10);
             AddScrapMetalToFirstEmptySlot(10);

             AddCopperPipeToFirstEmptySlot(50);
             AddCopperPipeToFirstEmptySlot(20);

             AddHyperglassToFirstEmptySlot(3);
             
             var info = new InventorySlotsInfo(_slots);
             foreach (var inventorySlot in info.EmptySlots)
             {
                Assert.NotNull(inventorySlot);
                Assert.IsTrue(inventorySlot.IsEmpty);
             }

             Assert.AreEqual(2, info.CountSlotsContainingItem(_copperPipe));
             Assert.AreEqual(3, info.CountSlotsContainingItem(_scrapMetal));
             Assert.AreEqual(1, info.CountSlotsContainingItem(_hyperglass));
         }
    }
    public class InventoryTest
    {
        private const string InventoryName = "Test Inventory";
        private Inventory _inventory;
        private const int InventorySize = 10;
       
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



         [UnityTest]
         public IEnumerator CanRemoveItemFromInventory()
         {
             const int amountToRemove = 1;
             BuildEmptyInventory();
             Assert.Greater(_inventory.GetEmptySlotCount(), 0);
             yield return UniTask.ToCoroutine(async () =>
             {
                 var copperPipe = await AssumedItemNames.ITEM_COPPER_PIPE.LoadItemAsync();
                 Assert.AreEqual(0, _inventory.GetItemCount(copperPipe));
                 int cnt = _inventory.GetEmptySlotCount();
                 _inventory.AddItem(copperPipe, amountToRemove);
                 Assert.AreEqual(cnt-1, _inventory.GetEmptySlotCount());
                 Assert.AreEqual(amountToRemove, _inventory.GetItemCount(copperPipe));
                 Assert.IsTrue(_inventory.RemoveItem(copperPipe, amountToRemove));
                 Assert.AreEqual(0, _inventory.GetItemCount(copperPipe));
                 Assert.AreEqual(cnt, _inventory.GetEmptySlotCount());
             });
         }
         
         [UnityTest]
         public IEnumerator CannotRemoveMoreItemsThanExistInInventory()
         {
             const int amountToAdd = 1;
             BuildEmptyInventory();
             Assert.Greater(_inventory.GetEmptySlotCount(), 0);
             yield return UniTask.ToCoroutine(async () =>
             {
                 var copperPipe = await AssumedItemNames.ITEM_COPPER_PIPE.LoadItemAsync();
                 Assert.AreEqual(0, _inventory.GetItemCount(copperPipe));
                 Assert.IsTrue(_inventory.AddItem(copperPipe, amountToAdd));
                 Assert.AreEqual(amountToAdd, _inventory.GetItemCount(copperPipe));
                 Assert.IsFalse(_inventory.RemoveItem(copperPipe, amountToAdd + 1));
             });
         }

         [UnityTest]
         public IEnumerator CannotRemoveItemFromEmptyInventory()
         {
             BuildEmptyInventory();
             yield return UniTask.ToCoroutine(async () =>
             {
                 var copperPipe = await AssumedItemNames.ITEM_COPPER_PIPE.LoadItemAsync();
                 Assert.AreEqual(0, _inventory.GetItemCount(copperPipe));
                 Assert.IsFalse(_inventory.RemoveItem(copperPipe, 1));
                 Assert.IsFalse(_inventory.RemoveItem(copperPipe, 1));
             });
         }

         [UnityTest]
         public IEnumerator CanAddItemIfThereIsSpaceInStacks()
         {
             BuildEmptyInventory();
             yield return UniTask.ToCoroutine(async () =>
             {
                 var copperPipe = await AssumedItemNames.ITEM_COPPER_PIPE.LoadItemAsync();
                 var slots = _inventory.itemSlots.ToArray();
                 
                 for (int i = 0; i < slots.Length-1; i++)
                 {
                     var slot = slots[i];
                     slot.SetItemStack(new ItemStack(copperPipe, copperPipe.MaxStackSize));
                     Assert.IsTrue(slots[i].IsFull);
                     Assert.IsFalse(slots[i].IsEmpty);
                 }
                 slots[^1].SetItemStack(new ItemStack(copperPipe,1));
                 Assert.IsFalse(slots[^1].IsFull);
                 Assert.IsFalse(slots[^1].IsEmpty);
                 Assert.AreEqual(1, slots[^1].ItemStack.Count);
                 
                 //all slots full except the last slot so the last slot should be able to accept 1 more item
                 Assert.AreEqual(0, _inventory.GetEmptySlotCount());
                 Assert.IsTrue(_inventory.CanAddItem(copperPipe, 1));
                 Assert.IsTrue(_inventory.AddItem(copperPipe, 1));
                 Assert.AreEqual(2, slots[^1].ItemStack.Count);
             });
         }
         
         [UnityTest]
         public IEnumerator CannotAddItemIfThereIsNoSpaceInStacks()
         {
             BuildEmptyInventory();
             yield return UniTask.ToCoroutine(async () =>
             {
                 var copperPipe = await AssumedItemNames.ITEM_COPPER_PIPE.LoadItemAsync();
                 foreach(var slot in _inventory.itemSlots)
                     slot.SetItemStack(new ItemStack(copperPipe, copperPipe.MaxStackSize));
                 Assert.AreEqual(0, _inventory.GetEmptySlotCount());
                 Assert.IsFalse(_inventory.AddItem(copperPipe, 1));
                 Assert.IsFalse(_inventory.CanAddItem(copperPipe, 1));
             });
         }
         
         [UnityTest]
         public IEnumerator CanAddItemIfThereAreEmptySlots()
         {
             const int amountToAdd = 1;
             BuildEmptyInventory();
             Assert.Greater(_inventory.GetEmptySlotCount(), 0);
             yield return UniTask.ToCoroutine(async () =>
             {
                 var copperPipe = await AssumedItemNames.ITEM_COPPER_PIPE.LoadItemAsync();
                Assert.AreEqual(0, _inventory.GetItemCount(copperPipe));
                Assert.IsTrue(_inventory.AddItem(copperPipe, amountToAdd));
                Assert.AreEqual(amountToAdd, _inventory.GetItemCount(copperPipe));
             });
         }
    }
}