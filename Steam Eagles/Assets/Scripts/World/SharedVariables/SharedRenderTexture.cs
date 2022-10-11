using CoreLib;
using UnityEngine;

namespace World
{
    [CreateAssetMenu(fileName = "New Shared Render Texture", menuName = "Shared Variables/Create Shared Render Texture", order = 0)]
    public class SharedRenderTexture : SharedVariable<RenderTexture>
    {
        
    }
}