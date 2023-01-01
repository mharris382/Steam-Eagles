using Characters;
using UnityEngine;

namespace Players
{
    public struct PlayerInstance
    {
        public readonly Player player;
        public readonly PlayerCharacterInput input;
        public readonly Camera camera;
        public readonly CharacterState state;

        public PlayerInstance(Player player, PlayerCharacterInput input, Camera camera, CharacterState characterState)
        {
            Debug.Assert(player != null, "Player is null");
            Debug.Assert(input != null, "Player Input is null", player);
            Debug.Assert(camera != null, "Camera is not assigned", player);
            Debug.Assert(characterState != null, "Character is not assigned", player);
            
            this.player = player;
            this.input = input;
            this.camera = camera;
            this.state = characterState;
        }
    }
}