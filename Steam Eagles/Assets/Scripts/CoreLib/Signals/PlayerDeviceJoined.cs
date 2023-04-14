using UnityEngine;

namespace Players.Shared
{
    public class PlayerDeviceJoined
    {
        public int PlayerNumber { get; }
        public GameObject PlayerInput { get; }

        public PlayerDeviceJoined(int playerNumber, GameObject playerInput)
        {
            PlayerNumber = playerNumber;
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