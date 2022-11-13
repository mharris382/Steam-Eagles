using UnityEngine;
using UnityEngine.Events;

namespace Puzzles
{
    public class HoldableItem : MonoBehaviour
    {
        public UnityEvent<GameObject> onPickedUp;
        public UnityEvent<GameObject> onDropped;
        
        
        public bool IsHeld
        {
            get;
            set;
        }
    }
}