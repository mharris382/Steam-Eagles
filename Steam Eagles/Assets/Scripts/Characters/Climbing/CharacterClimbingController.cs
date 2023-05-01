using System;
using DefaultNamespace;
using SteamEagles.Characters;
using UnityEngine;

namespace Characters
{
    /// <summary>
    /// responsible for managing the physics of the character when climbing
    /// </summary>
    public class CharacterClimbingController
    {
        private readonly CharacterState _characterState;
        private readonly CharacterConfig.ClimbConfig _config;
        private readonly CharacterClimbState _climbState;
        private readonly StructureState _structureState;
        private readonly CharacterClimbCheck _climbCheck;
        private readonly CapsuleCollider2D _capsuleCollider2D;

        private CharacterClimbHandle _climbHandle;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="characterState"></param>
        /// <param name="config"></param>
        /// <param name="climbCheck">updates climb check timing so that climb check knows how long it's been since we were climbing</param>
        public CharacterClimbingController(
            CharacterState characterState, 
            CharacterConfig config, 
            CharacterClimbCheck climbCheck)
        {
            _characterState = characterState;
            _climbState = characterState.ClimbState;
            _config = config.ClimbSettings;
            _climbCheck = climbCheck;
            _capsuleCollider2D = characterState.Collider;
            _structureState = characterState.GetComponent<StructureState>();
        }

        public void OnClimbStart()
        {
            _structureState.Mode = StructureState.JointMode.DISABLED;
            Debug.Assert(!_structureState.BuildingJoint.enabled, "StructureState.JointMode.DISABLED should have disabled the joint");
            var climbable = _climbCheck.CurrentClimbable;
            Debug.Assert(climbable != null, "climbable == null");
            _climbState.StartClimbing(climbable);
            _climbHandle = _climbState.CurrentClimbHandle;
        }
        public void OnClimbStopped()
        {
            _structureState.Mode = StructureState.JointMode.DISABLED;
            _climbState.StopClimbing();
            _climbHandle.Dispose();
        }


        public bool ShouldStopClimbing()
        {
            if (_climbHandle.IsExplicitlyDisposed)
                return true;
            
            return false;
        }
        public void UpdateClimbing(float deltaTime)
        {
            _climbCheck.TimestampClimb();
            if (_characterState.JumpHeld)
            {
                _climbHandle.Dispose();
                return;
            }
            var input = _characterState.MoveY;
            if (input == 0)
            {
                return;
            }
            bool climbUp = input > 0;
            float maxSpeed = _config.climbSpeedBase * (climbUp ? _config.climbUpSpeedMultiplier : _config.climbDownSpeedMultiplier);
            if (_characterState.SprintHeld && !climbUp)
            {
                maxSpeed = _config.slideDownSpeed;
            }
            
            float speed = input * maxSpeed * (climbUp ? 1 : -1);
           _climbHandle.MoveClimber(speed * deltaTime);
        }
    }
}