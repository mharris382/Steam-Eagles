using SteamEagles.Characters;

namespace Buildables.Tiles
{
    public class JumpPad : BuildableTile
    {
        public float jumpBoost = 2;
        protected override void OnCharacterEntered(CharacterState character)
        {
            character.ExtraJumpTime = jumpBoost;
            base.OnCharacterEntered(character);
        }

        protected override void OnCharacterExit(CharacterState character)
        {
            character.ExtraJumpTime = 0;
            base.OnCharacterExit(character);
        }
    }
}