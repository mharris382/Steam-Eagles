using UnityEngine;
using UnityEngine.Events;

namespace Puzzles
{
    public class HoldableItem : MonoBehaviour
    {
        public UnityEvent onPickedUp;
        public UnityEvent onDropped;
        
        
        public bool IsHeld
        {
            get;
            set;
        }
    }
}