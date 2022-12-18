using UnityEngine;

namespace Players
{
    public struct PlayerState
    {
        public string tag => Character.tag;

        public readonly int playerNumber;
        public readonly Camera Camera;
        public readonly GameObject Character;
    
    }
}


