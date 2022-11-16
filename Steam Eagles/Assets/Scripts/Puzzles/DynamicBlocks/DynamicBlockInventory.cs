using System;
using System.Collections;
using System.Collections.Generic;
using CoreLib;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace Puzzles
{
    /// <summary>
    /// add this to builder and transporter both.  They will then each have their own handler which will manage the
    /// blocks once they are held.
    /// </summary>
    public sealed class DynamicBlockInventory : MonoBehaviour
    {
        [SerializeField] private DynamicBlockCollector collector;
        [SerializeField] private float pickupAnimationDuration = 0.25f;
        [SerializeField] private Transform pickupPoint;
        
        [Header("Animation Parameters")] 
        [SerializeField] private float jumpPower = 1;
        [SerializeField] private Ease jumpEase = Ease.InOutCubic;
        [Header("Inventory")] 
        [SerializeField] private InventoryBlock[] inventoryBlocks;
        
        private IntReactiveProperty _heldBlocks = new IntReactiveProperty(0);
        public int HeldBlockCount
        {
            get => _heldBlocks.Value;
            set => _heldBlocks.Value = value;
        }

        public IObservable<int> OnHeldBlockCountChanged => _heldBlocks;
        
        
        [Serializable]
        public class InventoryBlock
        {
            public ScriptableObject dynamicBlock;
            public int startingAmount = 0;
            public SharedInt inventoryCount;

            public int HeldAmount
            {
                get => inventoryCount.Value;
                set => inventoryCount.Value = value;
            }
        }

        private Dictionary<ScriptableObject, InventoryBlock> _inventoryBlocks;
        private void Awake()
        {
            if (pickupPoint == null) pickupPoint = transform;
            if (collector == null)
            {
                collector = GetComponentInChildren<DynamicBlockCollector>();
            }

            _inventoryBlocks = new Dictionary<ScriptableObject, InventoryBlock>();
            foreach (var inventoryBlock in inventoryBlocks)
            {
                _inventoryBlocks.Add(inventoryBlock.dynamicBlock, inventoryBlock);
                inventoryBlock.HeldAmount = inventoryBlock.startingAmount;
            }
            //collector.onDynamicBlockCollected.AddListener(OnBlockAdded);
            collector.onDynamicBlockCollected.AsObservable().TakeUntilDestroy(this)
                .Where(t => _inventoryBlocks.ContainsKey(t.blockID)).Subscribe(OnBlockAdded);
        }
        

        public void OnBlockAdded(DynamicBlock block)
        {
            StartCoroutine(DoPickupAnimation(block, () => AddBlockToInventory(block)));
        }

        public void AddBlockToInventory(DynamicBlock block)
        {
            GameObject.Destroy(block.gameObject);
            HeldBlockCount += 1;
            _inventoryBlocks[block.blockID].HeldAmount++;
        }

        IEnumerator DoPickupAnimation(DynamicBlock block, Action onAnimationCompleted)
        {
            block.enabled = false;
            block.Rigidbody2D.transform.SetParent(pickupPoint, true);
            var tween = block.Rigidbody2D.transform
                .DOLocalJump(Vector3.zero, jumpPower, 1, pickupAnimationDuration)
                .SetAutoKill(true).SetEase(jumpEase).Play();
            yield return tween.WaitForCompletion();
            onAnimationCompleted?.Invoke();
        }
    
    }
}