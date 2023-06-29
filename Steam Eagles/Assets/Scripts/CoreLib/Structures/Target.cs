using UnityEngine;

namespace CoreLib.Structures
{
    public struct Target
    {
        public Transform transform { get; }

        public Target(Transform transform)
        {
            this.transform = transform;
        }

        public override string ToString() => transform == null ? "NULL TARGET" : $"{transform.name} (Target)";
        public override int GetHashCode() => transform.GetHashCode();
    }
}