using UnityEngine;

namespace GasSim
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class GasAgent : MonoBehaviour
    {
        Rigidbody2D _rigidbody2D;
        Collider2D _collider2D;

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _collider2D = GetComponent<Collider2D>();
        }
    }
}