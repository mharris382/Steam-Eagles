using System;
using StateMachine;
using UnityEngine;

namespace Characters
{
    public abstract class CharacterSystem : MonoBehaviour
    {
        
        [SerializeField] private SharedTransform characterTransform;

        private void Awake()
        {
            Debug.Assert(characterTransform != null, "Missing Character", this);
        }
    }


    public enum PlayerControlledCharacter
    {
        BUILDER,
        TRANSPORTER
    }
}