using System;
using CoreLib;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Puzzles
{
    public class HoldableItem : MonoBehaviour
    {
        [SerializeField]
        private float torqueMultiplier = 1;

        [SerializeField] private BoolReactiveProperty isHeld = new BoolReactiveProperty(false);
        
        
        
        [Serializable]
        public class Events
        {
            [SerializeField] internal UnityEvent<GameObject> onPickedUp;
            [SerializeField] internal UnityEvent<GameObject> onDropped;
            [SerializeField] internal UnityEvent<Vector2> onThrown;
        }
        
        [SerializeField] private Events events;
        [SerializeField] private UnityEvent<GameObject> onPickedUp;
        [SerializeField] private UnityEvent<GameObject> onDropped;
        public IObservable<bool> IsHeldStream => isHeld;
        
        public Collider2D[] grabColliders;
        
        public bool IsHeld
        {
            get => isHeld.Value;
            set => isHeld.Value = value;
        }

        public GameObject HeldBy
        {
            get;
            set;
        }


        private Rigidbody2D _rb;
        public Rigidbody2D rb => _rb ? _rb : _rb = GetComponent<Rigidbody2D>();


        
        public float TorqueMultiplier { get; set; }


        public virtual void Dropped(GameObject droppedBy)
        {
            IsHeld = false;
            onDropped?.Invoke(droppedBy);
            events.onDropped?.Invoke(droppedBy);
            Debug.Log($"{name.Bolded()} was dropped by {droppedBy.name.Bolded()}");
        }
        
        public virtual void PickedUp(GameObject pickedUpBy)
        {
            IsHeld = true;
            HeldBy = pickedUpBy;
            onPickedUp?.Invoke(pickedUpBy);
            events.onPickedUp?.Invoke(pickedUpBy);
            Debug.Log($"{name.Bolded()} was Picked Up by {pickedUpBy.name.Bolded()}");
        }

        public virtual void Thrown(Vector2 releaseForce, float releaseTorque)
        {
            events.onThrown?.Invoke(releaseForce);
        }
    }
}