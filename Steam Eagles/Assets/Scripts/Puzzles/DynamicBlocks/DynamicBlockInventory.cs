using System;
using System.Collections;
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

        private IntReactiveProperty _heldBlocks = new IntReactiveProperty(0);
        public int HeldBlockCount
        {
            get => _heldBlocks.Value;
            set => _heldBlocks.Value = value;
        }

        public IObservable<int> OnHeldBlockCountChanged => _heldBlocks;

        private void Awake()
        {
            if (pickupPoint == null) pickupPoint = transform;
            if (collector == null)
            {
                collector = GetComponentInChildren<DynamicBlockCollector>();
            }
            collector.onDynamicBlockCollected.AddListener(OnBlockAdded);
        }

        private void OnDestroy()
        {
            collector.onDynamicBlockCollected.RemoveListener(OnBlockAdded);
        }

        public void OnBlockAdded(DynamicBlock block)
        {
            StartCoroutine(DoPickupAnimation(block, () => AddBlockToInventory(block)));
        }

        public void AddBlockToInventory(DynamicBlock block)
        {
            GameObject.Destroy(block.gameObject);
            HeldBlockCount += 1;
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