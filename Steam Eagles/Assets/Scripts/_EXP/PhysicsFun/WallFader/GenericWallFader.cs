using UnityEngine;
using UnityEngine.Events;

namespace PhysicsFun
{
    public class GenericWallFader : WallFaderBase
    {
        public Color wallColor = Color.white;
        public UnityEvent<Color> onWallColorChange;
        public UnityEvent<float> onWallAlphaChange;
        
        
        private float _alpha = 1f;
        public override void SetWallAlpha(float alpha)
        {
            _alpha = alpha;
            // do something
            var color = wallColor;
            color.a = alpha;
            onWallColorChange?.Invoke(color);
            onWallAlphaChange?.Invoke(alpha);
        }

        public override float GetWallAlpha() => _alpha;
    }
}