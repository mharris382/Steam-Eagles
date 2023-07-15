using UnityEngine;

namespace CoreLib.Extensions
{
    public static class RenderTextureExtensions
    {
        public static bool SizeMatches(this RenderTexture texture, int width , int height)
        {
            if (texture == null) return false;
            return texture.width == width && texture.height == height;
        }
        public static bool SizeMatches(this RenderTexture renderTexture, params RenderTexture[] others)
        {
            if (others.Length == 1)
            {
                var other = others[0];
                if (renderTexture == null || other == null) return false;
            
                return renderTexture.width == other.width && renderTexture.height == other.height;
            }

            foreach (var texture in others)
            {
                if (texture == null) return false;
                if (renderTexture.width != texture.width || renderTexture.height != texture.height)
                {
                    return false;
                }
            }
            return true;
        }
    }
}