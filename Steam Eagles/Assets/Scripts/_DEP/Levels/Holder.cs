using System;
using UnityEngine;
using UnityEngine.Events;

namespace Levels
{
    public class Holder : MonoBehaviour
    {
        /// <summary>
        /// The game-object which logically owns this holder
        /// <para>the owner game-object by input system when the player wants to pickup an object.  The </para>
        /// </summary>
        public GameObject owner;
        [SerializeField]
        private Transform holdTransform;
        public Events holderEvents;

        Transform _previousParent;
        private GameObject _heldObject;

        public Transform HoldTransform
        {
            get => holdTransform;
            set
            {
                if (value == null) return;
                if (IsHolding)
                {
                    Hold(HeldObject);
                    holdTransform = value;
                }
            }
        }

        public GameObject HeldObject
        {
            get => _heldObject;
            set => _heldObject = value;
        }

        public bool IsHolding => _heldObject != null;

        
        [Serializable]
        public class Events
        {
            public UnityEvent<GameObject> onPickedUpObject;
            public UnityEvent<GameObject> onDroppedUpObject;
        }
        
        
        public void DropHeldObject()
        {
            if(!IsHolding) return;
            var obj = _heldObject;
            if (obj == null) return;
            _heldObject = null;
            
            obj.transform.SetParent(_previousParent);
            ObjectReleased(obj);
            obj.SendMessage("OnDropped", this, SendMessageOptions.DontRequireReceiver);
            
            holderEvents.onDroppedUpObject.Invoke(obj);
        }
    
        
        public void PickUpObject(GameObject obj)
        {
            if (IsHolding) DropHeldObject();
            if (obj == null) return;
            if (!CanPickupGameObject(obj)) return;
            HeldObject = obj;
            
            obj.transform.position = HoldTransform.position;
            obj.transform.rotation = HoldTransform.rotation;
            _previousParent = obj.transform.parent;
            obj.transform.SetParent(HoldTransform);
            
            obj.SendMessage("OnPickup", this, SendMessageOptions.DontRequireReceiver);
            holderEvents.onPickedUpObject?.Invoke(obj);
        }


        public bool CanPickupGameObject(GameObject gameObject)
        {
            if (IsHolding)
            {
                return false;
            }
            return false;
        }

        protected virtual bool CanPickup(GameObject gameObject)
        {
            return true;
        }

        protected virtual void Hold(GameObject obj)
        {
        }
        
        protected virtual void ObjectReleased(GameObject ob)
        {
            
        }
    }
    
    
    
     
    public struct ObjectPickedUpInfo
    {
        public readonly GameObject droppedObject;
        public readonly Holder droppedBy;
        
        public readonly Vector2 pickedUpPosition;
        public Vector2 dropVelocity;
        
        
        public ObjectPickedUpInfo(GameObject droppedObject, Holder droppedBy, Vector2 pickedUpPosition, Vector2 dropVelocity)
        {
            this.droppedObject = droppedObject;
            this.droppedBy = droppedBy;
            this.pickedUpPosition = pickedUpPosition;
            this.dropVelocity = dropVelocity;
        }
    }
    
    public struct ObjectDroppedInfo
    {
        public readonly GameObject droppedObject;
        public readonly Holder droppedBy;
        
        public readonly Vector2 dropPosition;
        public Vector2 dropVelocity;
        
        
        public ObjectDroppedInfo(GameObject droppedObject, Holder droppedBy, Vector2 dropPosition, Vector2 dropVelocity)
        {
            this.droppedObject = droppedObject;
            this.droppedBy = droppedBy;
            this.dropPosition = dropPosition;
            this.dropVelocity = dropVelocity;
        }
    }
}