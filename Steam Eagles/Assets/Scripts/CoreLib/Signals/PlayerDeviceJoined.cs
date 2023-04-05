using UnityEngine;

namespace Players.Shared
{
    public class PlayerDeviceJoined
    {
        public int Index { get; }
        public GameObject PlayerInput { get; }

        public PlayerDeviceJoined(int index, GameObject playerInput)
        {
            Index = index;
            PlayerInput = playerInput;
        }
    }
    
    public class PlayerDeviceLost
    {
        public int Index { get; }
        public GameObject PlayerInput { get; }

        public PlayerDeviceLost(int index, GameObject playerInput)
        {
            Index = index;
            PlayerInput = playerInput;
        }
    }
}