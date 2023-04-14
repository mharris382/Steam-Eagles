using System.Collections;
using System.Collections.Generic;
using Characters;
using SteamEagles.Characters;
using UnityEngine;

public class AICharacterController : MonoBehaviour
{
    CharacterState _state;
    
    void Awake()
    {
        _state = GetComponent<CharacterState>();
    }

    void Update()
    {
        
    }
}
