using System;
using System.Collections.Generic;
using DefaultNamespace;
using SteamEagles.Characters;
using UnityEngine;
using CoreLib.Interfaces;

namespace Characters
{
    /// <summary>
    /// several situations need to be handled to integrate climbing logic into the already complex movement logic
    /// there are 2 main groups of situations:
    /// 1. the character is not climbing and we need to check if the character should start climbing
    /// 2. the character is climbing and we need to check if the character should stop climbing
    /// </summary>
    public class CharacterClimbCheck
    {
        private readonly CharacterState _characterState;
        private readonly CharacterClimbState _climbState;
        private readonly CharacterConfig.ClimbConfig _config;
        private readonly CapsuleCollider2D _capsuleCollider2D;
        
        
        private List<IClimbableFactory> _climbableFactories;
        private IClimbable _currentClimbable;
        
        public IClimbable CurrentClimbable => _currentClimbable;
        
        private readonly int _climbLayers;
        private float _timeLastClimbed;
        
        public float TimeSinceLastClimb => Time.time - _timeLastClimbed;
        private bool AbleToStartClimbing => TimeSinceLastClimb > _config.timeBetweenClimbs;

        public CharacterClimbCheck(CharacterState characterState, params IClimbableFactory[] climbableFactories)
        {
            _characterState = characterState;
            _climbState = _characterState.ClimbState;
            _config = _characterState.config.ClimbSettings;
            _climbLayers = LayerMask.GetMask("Triggers");
            _capsuleCollider2D = characterState.Collider;
            _climbableFactories = new List<IClimbableFactory>(climbableFactories);
            var childFactories = _characterState.GetComponentsInChildren<IClimbableFactory>();
            _climbableFactories.AddRange(childFactories);
            Debug.Assert(_climbableFactories.Count > 0, "No climbable factories were provided",characterState);
        }
        
        public bool ShouldCharacterStopClimbing()
        {
            if(_characterState.IsClimbing== false)
                return false;
            if(_characterState.JumpPressed)
                return true;
            return false;
        }

        public bool ShouldCharacterStartClimbing()
        {
            if(_characterState.IsClimbing)
                return false;
            
            if (!AbleToStartClimbing)
                return false;
            
            if (_characterState.MoveY == 0)
                return false;
            
            //only check for climbables if the character input is mostly up or mostly down
            if (Mathf.Abs(_characterState.MoveX) > Mathf.Abs(_characterState.MoveY))
                return false;
            
            
            
            var position = (Vector2)_characterState.transform.position;
            foreach (var climbableFactory in _climbableFactories)
            {
                if(climbableFactory.TryGetClimbable(position, out _currentClimbable, 0.5f))
                    return true;
            }
            return false;
        }

        public void TimestampClimb()
        {
            _timeLastClimbed = Time.time;
        }
    }
}