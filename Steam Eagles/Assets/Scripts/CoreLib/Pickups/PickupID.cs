using UnityEngine;

namespace CoreLib.Pickups
{
    public class PickupID : MonoBehaviour
    {
        private bool _keyAssigned;
        private string _key;
        public string Key
        {
            get=> _key;
            set
            {
                if (!_keyAssigned)
                {
                    _keyAssigned = true;
                    _key = value;
                }
            }
        }

        public bool HasKeyBeenAssigned => _keyAssigned;
    }


}