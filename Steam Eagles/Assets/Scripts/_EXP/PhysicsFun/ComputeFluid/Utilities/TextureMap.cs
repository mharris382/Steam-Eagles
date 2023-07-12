using Buildings;
using Buildings.Rooms;
using UnityEngine;

namespace _EXP.PhysicsFun.ComputeFluid.Utilities
{
    public class TextureMap
    {
        private readonly GasTexture _gasTexture;
        private readonly BoundsLookup _boundsLookup;

        public TextureMap(GasTexture gasTexture, BoundsLookup boundsLookup)
        {
            _gasTexture = gasTexture;
            _boundsLookup = boundsLookup;
        }
        
        
        public bool IsWsPositionInBounds(Vector3 wsPosition)
        {
            var bounds = _boundsLookup.GetWsBounds();
            wsPosition.z = bounds.center.z;
            return bounds.Contains(wsPosition);
        }
        public Vector2Int GetTextureCoord(Vector3 wsPosition)
        {
            var bounds = _boundsLookup.GetWsBounds();
            wsPosition.z = bounds.center.z;
            if (!bounds.Contains(wsPosition)) wsPosition = bounds.ClosestPoint(wsPosition);
            var tx = Mathf.InverseLerp(bounds.min.x, bounds.max.x, wsPosition.x);
            var ty = Mathf.InverseLerp(bounds.min.y, bounds.max.y, wsPosition.y);
            var texelX = Mathf.RoundToInt(tx * _gasTexture.RenderTexture.width);
            var texelY = Mathf.RoundToInt(ty * _gasTexture.RenderTexture.height);
            return new Vector2Int(texelX, texelY);
        }
    }
}