using UnityEngine;

namespace CoreLib.Signals
{
    public struct PlayerPressedPausedSignal
    {
        public PlayerPressedPausedSignal(int playerID, Component playerInput)
        {
            PlayerID = playerID;
            PlayerInput = playerInput;
        }

        public int PlayerID { get; set; }
        public Component PlayerInput { get; }
    }
}