using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Trampoline : MonoBehaviour
{
    [Min(1)]
    public float bounciness = 1.5f;
    public float bounceTime= 1.5f;

    public float bounceResetTime = 1;
    
    private Rigidbody2D _rb;

    private float _minBounce;
    private float _maxBounce;
    private float timeLastBounced = 0;
    private bool CanBounce => Time.time - timeLastBounced > bounceResetTime;
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _maxBounce = bounciness;
        _minBounce = 1;
    }

    public UnityEvent<GameObject> onPlayerBounced;
    private void Update()
    {
        
        if (CanBounce)
        {
            _rb.sharedMaterial = new PhysicsMaterial2D() { friction = 0, bounciness = bounciness };
        }
        else
        {
            _rb.sharedMaterial = new PhysicsMaterial2D() { friction = 0, bounciness = 1 };
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Builder") || col.gameObject.CompareTag("Transporter"))
        {
            timeLastBounced = Time.time;
            onPlayerBounced.Invoke(col.gameObject);
        }
    }
}