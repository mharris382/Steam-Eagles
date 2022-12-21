using UnityEngine;

namespace PhysicsFun
{
    public abstract class WallFaderBase : MonoBehaviour
    {
        public abstract void SetWallAlpha(float alpha);

        public abstract float GetWallAlpha();
    }
}