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
}
