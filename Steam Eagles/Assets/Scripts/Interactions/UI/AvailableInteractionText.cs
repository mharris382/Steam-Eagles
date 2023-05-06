using UnityEngine;
using UnityEngine.Events;

namespace Interactions.UI
{
    public class AvailableInteractionText : MonoBehaviour, IAvailableInteractionLabel
    {
        public UnityEvent<string> OnSetText;


        public void SetText(string text)
        {
            OnSetText?.Invoke(text);
        }
    }
}