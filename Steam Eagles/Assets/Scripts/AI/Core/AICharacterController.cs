using System.Collections;
using System.Collections.Generic;
using Characters;
using UnityEngine;

public class AICharacterController : MonoBehaviour
{
    Character _state;
    
    void Awake()
    {
        _state = GetComponent<Character>();
    }

    void Update()
    {
        
    }
}
