using SteamEagles.Characters;

namespace Characters
{
    public class CharacterClimbState
    {
        private readonly CharacterState _characterState;
        private CharacterClimbHandle _climbHandle;

        public CharacterClimbHandle CurrentClimbHandle => _climbHandle;
        
        public CharacterClimbState(CharacterState characterState)
        {
            _characterState = characterState;
        }


        public void StartClimbing(IClimbable climbable)
        {
            _characterState.IsClimbing = true;
            _climbHandle = new CharacterClimbHandle(_characterState, climbable);
        }
        
        public void StopClimbing()
        {
            _characterState.IsClimbing = false;
            _climbHandle.Dispose();
        }
    }
}