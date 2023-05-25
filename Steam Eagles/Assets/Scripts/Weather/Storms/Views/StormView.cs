using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Weather.Storms.Views
{
    public class StormView : MonoBehaviour
    {
        private Storm _storm;

        [ShowInInspector]
        public bool hasStormBeenAssigned => _storm != null;

        
        

        public void AssignStorm(Storm storm)
        {
            if (hasStormBeenAssigned)
                return;
            _storm = storm;
        }

        public class Factory : PlaceholderFactory<StormView>{ }
    }
}