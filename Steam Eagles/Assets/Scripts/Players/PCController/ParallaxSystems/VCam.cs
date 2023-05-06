using UnityEngine;
using UnityEngine.Events;

namespace Players.PCController.ParallaxSystems
{
    public class VCam : MonoBehaviour
    {
        public UnityEvent<int> onPriorityChanged;
        private void OnEnable()
        {
            gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            gameObject.SetActive(false);
        }

        public void SetPriority(int i)
        {
            onPriorityChanged?.Invoke(i);
        }
    }
}