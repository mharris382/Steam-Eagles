using System;
using UnityEngine;

namespace CoreLib.Cinematics
{
    /// <summary>
    /// to make it easier to work with cinemachine alongside the split-screen system, this class will automatically create a new virtual camera for each player
    /// </summary>
    public class PlayerCamera : MonoBehaviour
    {
        
        public void CreateCamera(int playerNumber)
        {
            switch (playerNumber)
            {
                case 1:
                    break;
                case 2:
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }

            throw new NotImplementedException();
        }
    }
}