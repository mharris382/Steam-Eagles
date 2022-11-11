using System;
using UnityEngine;

namespace Characters
{
    /// <summary>
    /// acts as a proxy for passing input to the player character
    /// </summary>
     [RequireComponent(typeof(CharacterState))]
    public class CharacterInputState : MonoBehaviour
    {
        private CharacterState _characterState;
        private CharacterState CharacterState => _characterState == null ? (_characterState = GetComponent<CharacterState>()) : _characterState;


        public bool JumpPressed
        {
            get => CharacterState.JumpPressed;
            set => CharacterState.JumpPressed = value;
        }

        public bool JumpHeld
        {
            get => CharacterState.JumpHeld;
            set => CharacterState.JumpHeld = value;
        }

        public float MoveX
        {
            get => CharacterState.MoveX;
            set => CharacterState.MoveX = value;
        }

        public float MoveY
        {
            get => CharacterState.MoveY;
            set => CharacterState.MoveY = value;
        }


        public Vector2 MoveInput
        {
            get => CharacterState.MoveInput;
            set => CharacterState.MoveInput = value;
        }


        public bool DropHeldItem
        {
            get;
            set;
        }

        public Vector2 AimInput
        {
            get;
            set;
        }
    }
}