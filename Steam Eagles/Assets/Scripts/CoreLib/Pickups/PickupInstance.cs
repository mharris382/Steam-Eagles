using System;
using UnityEngine;

namespace CoreLib.Pickups
{
    [RequireComponent(typeof(PickupBody))]
    public class PickupInstance : MonoBehaviour
    {
        private Pickup _pickup;
        private PickupBody _pickupBody;
        public PickupBody PickupBody => _pickupBody ? _pickupBody : _pickupBody = GetComponent<PickupBody>();
        public Pickup Pickup => _pickup;
        public void InjectPickup(Pickup pickup)
        {
            this._pickup = pickup;
            SetupPickupID();
        }
        void SetupPickupID()
        {
            var id = gameObject.AddComponent<PickupID>();
            id.Key = _pickup.key;
        }

        public void DisablePhysics()
        {
            PickupBody.DisablePhysics();
        }
    }
}