using UnityEngine;
using UnityEngine.Events;

namespace Utilities
{
    public class RemappedEventListener_Float : MonoBehaviour
    {
        [Tooltip("The event chains are now becoming fairly convoluted so it's a good idea to comment what this is event is for")]
        [Multiline]
        public string descrtiption;
        [SerializeField] private EventValueReMap eventValueReMapper;

        public UnityEvent<float> floatOutputEvent;
        
        public void OnEventRaised(int value)
        {
            floatOutputEvent.Invoke(eventValueReMapper.RemapIntToFloat(value));
        }

        public void OnEventRaised(float value)
        {
            floatOutputEvent.Invoke(eventValueReMapper.RemapFloat(value));
        }
    }
}