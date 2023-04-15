using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace CoreLib.Pickups
{
    public class CharacterPickupController : MonoBehaviour
    {
        [ChildGameObjectsOnly]
        public Collider2D triggerArea;
        
        
        public float pickupDuration = 1f;
        public float jumpPower = 4;
        public int numJumps = 1;
        public Transform jumpTarget;

        private void Awake()
        {
            triggerArea.OnTriggerEnter2DAsObservable()
                .Select(t => t.GetComponent<PickupInstance>())
                .Where(t => t != null)
                .Subscribe(OnPickupDetected)
                .AddTo(this);
        }

        public void OnPickupDetected(PickupInstance pickup)
        {
            if(PickupFilter != null && PickupFilter(pickup.Pickup) == false)
                return;
            TriggerPickup(pickup);
        }

        void TriggerPickup(PickupInstance pickupInstance)
        {
            
            StartCoroutine(UniTask.ToCoroutine(async () =>
            {
               var sequence = pickupInstance.PickupBody.transform.DOLocalJump(jumpTarget.localPosition, jumpPower, Mathf.Max(1, numJumps),
                    pickupDuration);
                sequence.SetEase(Ease.OutQuad);
                sequence.Play();
                await sequence.ToUniTask();
                OnPickup.OnNext(pickupInstance.Pickup);
            }));
        }
        
        public Subject<Pickup> OnPickup { get; } = new Subject<Pickup>();

        public Predicate<Pickup> PickupFilter { get; set; }  
    }
}