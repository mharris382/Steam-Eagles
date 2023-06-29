using UnityEngine;

namespace PhysicsFun
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class WallFaderSprite : WallFaderBase
    {
        private SpriteRenderer _sr;
        public SpriteRenderer sr => _sr ? _sr : _sr = GetComponent<SpriteRenderer>();
        public override void SetWallAlpha(float alpha)
        {
            var c = sr.color;
            c.a = alpha;
            sr.color = c;
        }

        public override float GetWallAlpha() => sr.color.a;
    }
}