using UnityEngine;

namespace Utilities
{
    public class EventHelperGameObject : MonoBehaviour
    {
        public float rotation;
        public void SetRotation(GameObject gameObject)
        {
            gameObject.transform.rotation = Quaternion.Euler(0, 0, rotation);
        }
    
        public void DisableCollider(GameObject gameObject)
        {
            gameObject.GetComponentInChildren<Collider2D>().enabled = false;
        }
    
        public void EnableCollider(GameObject gameObject)
        {
            gameObject.GetComponentInChildren<Collider2D>().enabled = true;
        }
    }
}
