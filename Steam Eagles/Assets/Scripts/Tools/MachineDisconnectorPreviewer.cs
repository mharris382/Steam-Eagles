using UnityEngine;

namespace Tools.BuildTool
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class MachineDisconnectorPreviewer : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;

        public SpriteRenderer spriteRenderer => _spriteRenderer ??= GetComponent<SpriteRenderer>();
    }
}