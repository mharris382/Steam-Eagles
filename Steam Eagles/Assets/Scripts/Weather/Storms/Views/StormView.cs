using UnityEngine;
using Zenject;

namespace Weather.Storms.Views
{
    public class StormView : MonoBehaviour
    {
        
        
        public class Factory : PlaceholderFactory<StormView>{ }
    }
}