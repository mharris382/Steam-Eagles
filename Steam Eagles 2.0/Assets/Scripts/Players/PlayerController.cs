using System;
using UnityEngine;

namespace Players
{
    public class PlayerController : MonoBehaviour 
    {
        [Range(1,2)]
        public int playerNumber;
    
        [HideInInspector]
        public bool hasJoined;


        public PlayerState Assign(Camera c, GameObject o, PlayerInput i)
        {
            throw new NotImplementedException();
        }
    }
}

    


