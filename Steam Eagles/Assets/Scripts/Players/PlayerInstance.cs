using Characters;
using UnityEngine;

namespace Players
{
    public struct PlayerInstance
    {
        public readonly Player player;
        public readonly PlayerInputWrapper inputWrapper;
        public readonly Camera camera;
        public readonly Character state;

        public PlayerInstance(Player player, PlayerInputWrapper inputWrapper, Camera camera, Character character)
        {
            Debug.Assert(player != null, "Player is null");
            Debug.Assert(inputWrapper != null, "Player Input is null", player);
            Debug.Assert(camera != null, "Camera is not assigned", player);
            Debug.Assert(character != null, "Character is not assigned", player);
            
            this.player = player;
            this.inputWrapper = inputWrapper;
            this.camera = camera;
            this.state = character;
        }
    }
}