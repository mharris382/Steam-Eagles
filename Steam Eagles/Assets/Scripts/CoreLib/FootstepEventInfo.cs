using UnityEngine;

namespace CoreLib
{
    public struct FootstepEventInfo
    {
        private readonly int _footstepType;
        public Vector2 Position { get; }

        public bool IsFrontFoot => (_footstepType & 1) != 0;
        public bool IsBackFoot => _footstepType >= 2;
        public bool IsBothFeet => _footstepType == 3;

        public FootstepEventInfo(Vector2 position, int footstepType)
        {
            _footstepType = footstepType;
            Position = position;
        }
    }
}