using UnityEngine;
using UnityEngine.U2D;

namespace PhysicsFun
{
    [RequireComponent(typeof(SpriteShapeRenderer))]
    public class WallSpriteShapeFader : WallFaderBase
    {
        public SpriteShapeRenderer spriteShapeController;
        public override void SetWallAlpha(float alpha)
        {
            // do something
            
            spriteShapeController.color = new Color(1, 1, 1, alpha);
        }
    }
}