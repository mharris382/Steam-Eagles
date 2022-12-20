using UnityEngine;
using UnityEngine.Events;

namespace PhysicsFun
{
    public class GenericWallFader : WallFaderBase
    {
        public Color wallColor = Color.white;
        public UnityEvent<Color> onWallColorChange;
        public UnityEvent<float> onWallAlphaChange;
        public override void SetWallAlpha(float alpha)
        {
            // do something
            var color = wallColor;
            color.a = alpha;
            onWallColorChange?.Invoke(color);
            onWallAlphaChange?.Invoke(alpha);
        }
    }
}