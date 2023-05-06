using UnityEngine;

namespace Players.PCController.ParallaxSystems
{
    public class VCam : MonoBehaviour
    {
        private void OnEnable()
        {
            gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            gameObject.SetActive(false);
        }
    }
}