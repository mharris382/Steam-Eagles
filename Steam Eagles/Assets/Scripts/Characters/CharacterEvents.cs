using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

public class CharacterEvents : MonoBehaviour
{
     CharacterState characterState;

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
        if (characterState == null)
        {
            characterState = GetComponent<CharacterState>();
        }
    }

    private void Start()
    {
        characterState.OnCharacterJumped.TakeUntilDisable(this).Subscribe(_ => OnCharacterJumped?.Invoke());
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        
    }
}
