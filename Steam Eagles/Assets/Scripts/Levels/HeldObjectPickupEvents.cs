using UnityEngine;
using UnityEngine.Events;

namespace Levels
{
    public class HeldObjectPickupEvents : MonoBehaviour
    {
        public UnityEvent<Holder> onHoldStart;
        public UnityEvent<Holder> onHoldEnd;
        public void OnPickup(Holder holder)
        {
            Debug.Log("Picked up by " + holder.name);
            onHoldStart ?.Invoke(holder);
        }

        public void OnDropped(Holder droppedBy)
        {
            Debug.Log("Dropped by " + droppedBy.name);
            onHoldEnd ?.Invoke(droppedBy);
        }
    }
}