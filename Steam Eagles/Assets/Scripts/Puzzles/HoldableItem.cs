using System;
using CoreLib;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Puzzles
{
    public class HoldableItem : MonoBehaviour
    {
        [SerializeField] private UnityEvent<GameObject> onPickedUp;
        [SerializeField] private UnityEvent<GameObject> onDropped;
        [SerializeField] private BoolReactiveProperty isHeld = new BoolReactiveProperty(false);
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
        
        
        
        public virtual void Dropped(GameObject droppedBy)
        {
            IsHeld = false;
            onDropped?.Invoke(droppedBy);
            Debug.Log($"{name.Bolded()} was dropped by {droppedBy.name.Bolded()}");
        }
        
        public virtual void PickedUp(GameObject pickedUpBy)
        {
            IsHeld = true;
            HeldBy = pickedUpBy;
            onPickedUp?.Invoke(pickedUpBy);
            Debug.Log($"{name.Bolded()} was Picked Up by {pickedUpBy.name.Bolded()}");
        }

        public virtual void Thrown(Vector2 releaseForce, float releaseTorque)
        {
            
        }
    }
}