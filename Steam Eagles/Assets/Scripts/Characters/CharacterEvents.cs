using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

public class CharacterEvents : MonoBehaviour
{
     Character _character;

     public UnityEvent OnCharacterJumped;
     
     [Serializable]
     public class CollisionEvents
     {
        public class LandingEvents
        {
            
            public UnityEvent landedOnPipe;
            public UnityEvent landedOnSolid;
        }
     }
     
    void Awake()
    {
        if (_character == null)
        {
            _character = GetComponent<Character>();
        }
    }

    private void Start()
    {
        _character.OnCharacterJumped.TakeUntilDisable(this).Subscribe(_ => OnCharacterJumped?.Invoke());
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        
    }
}
