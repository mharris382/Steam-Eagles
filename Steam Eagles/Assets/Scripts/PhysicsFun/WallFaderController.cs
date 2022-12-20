using UnityEngine;

namespace PhysicsFun
{
    /// <summary>
    /// handles fading FG walls in and out when the player enters the area.
    /// </summary>
    [RequireComponent(typeof(WallFaderBase))]
    public class WallFaderController : MonoBehaviour
    {
        private WallFaderBase _wfb;
        public WallFaderBase wfb => _wfb ? _wfb : (_wfb = GetComponent<WallFaderBase>());

        public float fadeTime = 1f;
        public float timeToTriggerUnFade = 5;
        
        
    }
    
    public abstract class WallFaderBase : MonoBehaviour
    {
        public abstract void SetWallAlpha(float alpha);
    }
}