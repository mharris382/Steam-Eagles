using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class PickupBody : MonoBehaviour
{
    
    private Rigidbody2D _rigidbody2D;
    private Collider2D _collider2D;
    private SpriteRenderer _spriteRenderer;
    public Rigidbody2D Rigidbody2D => _rigidbody2D ? _rigidbody2D : _rigidbody2D = GetComponent<Rigidbody2D>();
    public Collider2D Collider2D => _collider2D ? _collider2D : _collider2D = GetComponent<Collider2D>();
    public SpriteRenderer SpriteRenderer => _spriteRenderer ? _spriteRenderer : _spriteRenderer = GetComponent<SpriteRenderer>();
    
    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _collider2D = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        gameObject.layer = LayerMask.NameToLayer("Pickups");
    }
}
