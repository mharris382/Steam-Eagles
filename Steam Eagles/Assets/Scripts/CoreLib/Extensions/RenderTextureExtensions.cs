using UnityEngine;

namespace CoreLib.Extensions
{
    public static class RenderTextureExtensions
    {
        public static bool SizeMatches(this RenderTexture renderTexture, RenderTexture other)
        {
            if (renderTexture == null || other == null) return false;
            
            return renderTexture.width == other.width && renderTexture.height == other.height;
        }
    }
}