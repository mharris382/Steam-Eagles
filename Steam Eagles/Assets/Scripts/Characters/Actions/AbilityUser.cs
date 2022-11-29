using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib;
using StateMachine;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;
using World;

/// <summary>
/// since the ability system is getting newer more complex features added, I think it's important to setup a shared data container to hold shared state information
/// and act as a dependency injection entry point for other systems to interface with.
/// </summary>
public class AbilityUser : MonoBehaviour
{
    [System.Obsolete("user cannot own defaults?")]
    [Serializable]
    internal class DefaultResources
    {
        public SharedTilemap defaultPipeTilemap;
        public SharedTilemap defaultSolidTilemap;
    }

    
    [System.Obsolete("replace with something else")]
    [Serializable]
    public class BlockInventory
    {
        public bool infiniteResources;
        public int initialPipes = 10;
        public int initialBlocks = 10;

        int _storedPipes;

        public int StoredPipes
        {
            get
            {
                if (infiniteResources) return int.MaxValue;
                return _storedPipes;
            }
            set => _storedPipes = value;
        }

        int _storedBlocks;

        public int StoredBlocks
        {
            get
            {
                if (infiniteResources) return int.MaxValue;
                return _storedBlocks;
            }
            set => _storedBlocks = value;
        }

        public void Awake()
        {
            _storedBlocks = initialBlocks;
            _storedPipes = initialPipes;
        }
    }

    
    [SerializeField] private CharacterState characterState;
    [SerializeField] private SharedTransform characterTransform;
    
    public BlockInventory blockInventory = new BlockInventory();

    [SerializeField] internal DefaultResources defaultResources = new DefaultResources();


    [Serializable]
    public class TileSlot
    {
        public TilemapTypes TilemapType = TilemapTypes.PIPE;
    }

    private void Awake()
    {
        blockInventory.Awake();
        if (characterTransform != null)
        {
            if (characterTransform.HasValue) TryGetStateFromTransform(characterTransform.Value);
            characterTransform.onValueChanged.AddListener(TryGetStateFromTransform);

            void TryGetStateFromTransform(Transform t)
            {
                var characterState = t.GetComponent<CharacterState>();
                if (characterState != null)
                {
                    this.characterState = characterState;
                    Debug.Log($"Ability user found character state {characterState.name}");
                }
            }
        }

        if (characterState == null) characterState = GetComponentInParent<CharacterState>();
    }
}


public interface IInventoryItem
{
    string Name { get; }
    int Stored { get; }
    int Maximum { get; }
}







/// <summary>
/// literally only handles the storage of items, add items to inventory, remove items from inventory, etc.
/// <para>the most important part is how we identify the items, i'm optiming for string becasue it's fast </para>
/// </summary>
public class CharacterInventory : MonoBehaviour
{
    private ItemCounter _itemCount;
    private Dictionary<string, InventorySlot> _inventory = new Dictionary<string, InventorySlot>();
    private  Dictionary<string, InventorySlotGroup> _groups = new Dictionary<string, InventorySlotGroup>();

    public class ItemCounter
    {
        private Dictionary<string, IntReactiveProperty> _itemCount = new Dictionary<string, IntReactiveProperty>();
        
        
        /// <summary>
        /// for sake of simplicity I will only allow/adding removing items one at a time
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool AddItem(string name)
        {
            if (!_itemCount.ContainsKey(name))
            {
                _itemCount.Add(name, new IntReactiveProperty(0));
            }

            var count = _itemCount[name];
            count.Value += 1;
            return true;
        }
        /// <summary>
        /// for sake of simplicity I will only allow/adding removing items one at a time
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool RemoveItem(string name)
        {
            if (!_itemCount.ContainsKey(name))
            {
                return false;
            }

            var count = _itemCount[name];
            count.Value -= 1;
            return true;
        }
        public int GetItemCount(string name)
        {
            if (!_itemCount.ContainsKey(name))
                return 0;
            
            var count = _itemCount[name];
            return count.Value;
        }
        
        public ReadOnlyReactiveProperty<int> GetItemCountReactive(string name)
        {
            if (!_itemCount.ContainsKey(name))
            {
                _itemCount.Add(name, new IntReactiveProperty(0));
            }

            var count = _itemCount[name];
            return count.ToReadOnlyReactiveProperty();
        }
    }

    public InventorySlot[] slots;

    public ItemCounter Counter => _itemCount ??= new ItemCounter();
    private void Awake()
    {
        var slotGroups = GetComponentsInChildren<InventorySlotGroup>();
        foreach (var inventorySlotGroup in slotGroups)
        {
            _groups.Add(inventorySlotGroup.slotGroupName, inventorySlotGroup);
        }
        foreach (var slot in slots) 
        {
            
        }
    }

    InventorySlotGroup GetOrCreateGroup(string groupName)
    {
        if (_groups.ContainsKey(groupName))
        {
            return _groups[groupName];
        }
        var go = new GameObject(groupName);
        go.transform.SetParent(this.transform);
        var newGroup = go.AddComponent<InventorySlotGroup>();
        _groups.Add(groupName, newGroup);
        return newGroup;
    }
    
    int GetItemCount(string itemName)
    {
        if (_inventory.ContainsKey(itemName))
        {
            return Counter.GetItemCount(itemName);
        }
        return 0;
    }
    
    public void AddItem(string itemName, int amount) => Counter.AddItem(itemName);
    
    public void RemoveItem(string itemName, int amount) => Counter.RemoveItem(itemName);
    
    public bool HasItem(string itemName) => GetItemCount(itemName) > 0;

    public bool CanRemoveItem(string itemName) => HasItem(itemName);
    
    //TODO: allow restricting inventory count for certain items
    public bool CanAddItem(string itemName) => true;
    

    [Serializable]
    public class InventorySlot
    {
        public string slotName = "Slot";
        public string groupName = "";
        
        [Tooltip("write only to the stored value, do not read from it")]
        public SharedInt storedAmount;
    }
    
    
}

/// <summary>
/// used to create nested categories of inventory slots.  Say we want to allow the player to choose between more than one type of pipe block.
/// We can create a InventorySlotGroup called PipeBlocks, and have each pipe block a member of that group. Each group should also function as
/// a Item Selector for some item.  So for ConnectCellAbility, we can map the item name to arbitrary data resources  (those resources should be
/// loaded via the addressable system, avoiding the need to serialize them in the scene)
/// </summary>
public class InventorySlotGroup : MonoBehaviour
{
    public string slotGroupName;
    private List<string> _inventorySlotGroup;

    private void OnEnable()
    {
        
    }
}



public class InventoryItemDataLoader : MonoBehaviour
{
    
    
    
}
