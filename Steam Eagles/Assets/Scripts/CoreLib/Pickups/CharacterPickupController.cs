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
        public Ease ease = Ease.OutQuad;

        [SerializeField] private FadeModifier fadeModifier;
        [SerializeField] private ScaleModifier scaleModifier;
        private IPickupInput _input;

        private abstract class SequenceModifer
        {
            public bool enabled = true;
            [Range(0, 1)] public float fadeStart = 0.7f;

            public Ease ease = Ease.Linear;

            public Sequence ModifySequence(Sequence sequence, PickupInstance instance,
                CharacterPickupController controller)
            {
                if (!enabled)
                {
                    return sequence;
                }

                float duration = (1 - fadeStart) * controller.pickupDuration;
                float delay = fadeStart * controller.pickupDuration;
                return Modify(sequence, instance, duration, delay);
            }


            public abstract Sequence Modify(Sequence sequence, PickupInstance instance, float duration, float delay);
        }

        [Serializable]
        private class ScaleModifier : SequenceModifer
        {
            private float scale = 0.1f;
            public override Sequence Modify(Sequence sequence, PickupInstance instance, float duration, float delay)
            {
                return sequence.Insert(delay, instance.transform.DOScale(scale, duration).SetEase(ease));
            }
        }
        [Serializable]
        private class FadeModifier : SequenceModifer
        {
            public override Sequence Modify(Sequence sequence, PickupInstance instance, float duration, float delay)
            {
                var sr = instance.GetComponent<SpriteRenderer>();
                return sequence.Insert(delay, DOTween.To(() => sr.color, color => sr.color = color, Color.clear, duration).SetEase(ease));
            }
        }

        private void Awake()
        {
            _input = GetComponentInParent<IPickupInput>();
            _input.IsPickingUp.Subscribe(t => triggerArea.enabled = t).AddTo(this);
            Debug.Assert(_input !=null, "Missing Pickup Input", this);
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
            if (!_input.WantsToPickup(pickupInstance.Pickup.key))
            {
                return;
            }
            StartCoroutine(UniTask.ToCoroutine(async () =>
            { 
                var position = jumpTarget.position;
                pickupInstance.DisablePhysics();
               var sequence = pickupInstance.PickupBody.transform.DOJump(position, jumpPower, Mathf.Max(1, numJumps),
                    pickupDuration);
                sequence = fadeModifier.ModifySequence(sequence, pickupInstance, this);
                sequence = scaleModifier.ModifySequence(sequence, pickupInstance, this);
                sequence.SetEase(ease);
                sequence.Play();
                await UniTask.WaitUntil(sequence.IsComplete);
                OnPickup.OnNext(pickupInstance.Pickup);
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
                DestroyInstance(pickupInstance);
            }));
        }

        void DestroyInstance(PickupInstance instance)
        {
            //TODO: Pooling
            if(instance != null)
                Destroy(instance.gameObject);
        }
        
        public Subject<Pickup> OnPickup { get; } = new Subject<Pickup>();

        public Predicate<Pickup> PickupFilter { get; set; }  
    }
}