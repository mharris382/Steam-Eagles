using System;
using UnityEngine;

namespace CoreLib
{
    public class PCInstance : IDisposable
    {
        public PCInstance(GameObject character, GameObject camera, GameObject input)
        {
            this.character = character;
            this.camera = camera;
            this.input = input;
        }

        public GameObject character {get; private set;}
        public GameObject camera {get; private set;}
        public GameObject input {get; private set;}

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool IsValid() => character != null && camera != null && input != null;
    }

    public struct PCInstanceChangedInfo
    {
        public int playerNumber;
        public PCInstance pcInstance;

        public PCInstanceChangedInfo(int player, PCInstance pc)
        {
            playerNumber = player;
            pcInstance = pc;
        }
    }
}