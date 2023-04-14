using System;
using System.Collections;
using System.Collections.Generic;
using SteamEagles.Characters;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

public class CharacterEvents : MonoBehaviour
{
     CharacterState _characterState;

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
        if (_characterState == null)
        {
            _characterState = GetComponent<CharacterState>();
        }
    }

    private void Start()
    {
        _characterState.OnCharacterJumped.TakeUntilDisable(this).Subscribe(_ => OnCharacterJumped?.Invoke());
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        
    }
}
