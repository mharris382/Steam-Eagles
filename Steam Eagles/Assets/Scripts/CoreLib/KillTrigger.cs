using UnityEngine;

public class KillTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        Health health = null;
        if (col.gameObject.TryGetComponent(out health))
        {
            health.CurrentHealth = 0;
        }
    }
}