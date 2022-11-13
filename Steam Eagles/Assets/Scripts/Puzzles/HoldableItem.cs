using CoreLib;
using UnityEngine;
using UnityEngine.Events;

namespace Puzzles
{
    public class HoldableItem : MonoBehaviour
    {
        [SerializeField] private UnityEvent<GameObject> onPickedUp;
        [SerializeField] private UnityEvent<GameObject> onDropped;
        
        public virtual void Dropped(GameObject droppedBy)
        {
            onDropped?.Invoke(droppedBy);
            Debug.Log($"{name.Bolded()} was dropped by {droppedBy.name.Bolded()}");
        }
        
        public virtual void PickedUp(GameObject pickedUpBy)
        {
            onPickedUp?.Invoke(pickedUpBy);
            Debug.Log($"{name.Bolded()} was Picked Up by {pickedUpBy.name.Bolded()}");
        }
    }
}