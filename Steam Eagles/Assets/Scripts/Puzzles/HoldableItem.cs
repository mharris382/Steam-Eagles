using System;
using CoreLib;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Puzzles
{
    
    /// <summary>
    /// a interface for a physical object which can be picked up, held, thrown, and dropped by the player.  There are
    /// several specific rules regarding the behavior of a holdable item.  First, a holdable item minimally must have
    /// two states: held and dynamic.  The default way to hold an item is to disable all collisions and attach a joint
    /// between the object and the player's hand, but this may be overridden by a subclass.  The dynamic state REQUIRERS
    /// that the rigidbody be in a dynamic state, and that the object is able to be thrown.
    /// <para>The prototype item holding system worked, but was incredibly bugging and not modular enough.  The new holder
    /// system will be more generic have a more stable core system controlling it</para>
    ///
    /// If a holdable object is held by the player:
    ///
    ///     ========= GENERIC ACTIONS =================================================
    ///     the player is necessarily able to (provided they are in normal input state):
    ///         - throw the object
    ///         - drop the object
    ///     ========= OBJECT SPECIFIC ACTIONS =====================================================
    ///     the player may also be able to perform additional input actions on the object, such as:
    ///         - turn the gas valve to vacuum/release/off mode
    ///         - close/open the gas valve on gas tank
    ///         - lengthen/tightne rope
    ///         - turn on/off light
    ///         - turn on/off fan
    ///         - turn on/off water
    ///         - fire gun/steam blower
    ///         - load tool
    ///         - climb up/down rope
    ///    ========= CONTEXT AND OBJECT SPECIFIC ACTIONS =====================================================
    ///    additionally, the player may be able to use the object to interact with other objects, such as:
    ///         - use the gas tank to fill the gas valve
    ///         - attach rope to physics object
    ///         - insert key into lock
    ///         - bind rope to physics object
    ///         - bind gas tank to physics object
    ///     ======= CONTEXT SPECIFIC ACTIONS =====================================================
    ///     if the player is not holding an object, they may be able to perform context specific actions, such as:
    ///         - pushing a button, lever, or switch
    ///         - enter/exit a vehicle
    ///         - initiate dialogue with an NPC
    ///         - turn on/off a machine, lights
    ///         - release an object from a holder
    ///         - open/close a door
    ///         - climb through portal/pipe/ladder
    ///     ======= AUTOMATIC ACTIONS =====================================================
    ///     currently the only automatic action is the player's ability to pick up an object.  This is handled by CharacterHeldObjectController atm
    ///         - triggering a story event if the player is carrying the object
    ///
    ///     if the object is not being dropped or thrown:
    ///         pass player input handle to the object interactor to process custom interactions like
    /// ==========HOLDABLE OBJECTS LOGIC=====================
    /// Else if a holdable object is not held by player:
    ///     if the holdable object is held by another holder then the player is not able to interact with the object,but may be able to interact with the object if allows removing the object from the holder
    ///         if the holder object allows players to detach item from the holder:
    ///             the player is able to (provided they are in normal input state):
    ///                 create an object interaction handle between the player and that object 
    ///                 register the interaction handle in the player's current interaction list
    /// ==========CHARACTER HOLDER LOGIC =======================
    /// if the player is not holding a holdable object:
    ///     scan the environment for holdable objects:
    ///
    ///
    /// holder.ScanForHoldableObjects(knownHoldablesList);
    /// 
    ///     perform some kind of physics query to locate nearby holdable objects
    ///     store the results in Nearby holdables list
    ///     put the knownHoldables into a queue
    ///     while the queue is not empty:
    ///         check that the known holdable is in the nearby holdables list:
    ///             if it is not in the nearby holdables list:
    ///                 dispose of the holdable object interaction
    /// 
    ///     for each holdable object in the environment:
    ///         if we already know about that holdable object then continue
    ///         if the holdable object is not held by another holder:
    ///             add holdable object pickup interaction handle to the player's current interaction list
    ///
    ///     
    /// 
    /// </summary>
    public interface IHoldableItem
    {
        public bool IsHeld { get; }
        public GameObject HeldBy { get; }
        public Rigidbody2D rb { get; }
        
    }

    
    /// <summary>
    /// the interaction handle pattern is an implememntation of the command pattern.  We use this to store and track
    /// valid context specific player actions that can be performed on a given object.  The interaction handle can
    /// be used to perform the action 
    /// 
    /// </summary>
    public class PickupHoldableItemInteractionHandle
    {
        
    }
    
    
    
    
    public class 
    HoldableItem : MonoBehaviour
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