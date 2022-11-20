using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu(fileName = "Character Config", menuName = "New Character Config", order = 0)]
    public class CharacterConfig : ScriptableObject
    {
        public float moveSpeed = 600;
        public float jumpForce = 15f;
        public float jumpTime = 0.3f;
        public float groundedFriction = 0.4f;
        public float gravityScaleFall = 3;
        public float gravityScaleJump = 5;
    }
}