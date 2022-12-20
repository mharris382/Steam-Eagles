using UnityEngine;
using UnityEngine.Tilemaps;

namespace PhysicsFun
{
    [RequireComponent(typeof(Tilemap))]
    public class WallTilemapFader : WallFaderBase
    {
        private Tilemap _tmr;
        public Tilemap tmr => _tmr ? _tmr : _tmr = GetComponent<Tilemap>();
        public override void SetWallAlpha(float alpha)
        {
            var c = tmr.color;
            c.a = alpha;
            tmr.color = c;
        }
    }
}