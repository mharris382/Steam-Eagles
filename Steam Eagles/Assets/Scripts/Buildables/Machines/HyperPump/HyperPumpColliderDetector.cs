using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Buildables
{
    [InfoBox("Make sure animator has Triggers: Compress, DeCompress")]
    public class HyperPumpColliderDetector : MonoBehaviour
    {
        public Animator animator;
        private static readonly int Compress = Animator.StringToHash("Compress");
        private List<Collider2D> other = new();
        private static readonly int DeCompress = Animator.StringToHash("DeCompress");

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (Vector2.Dot(other.contacts[0].normal, Vector2.up) > 0.5f && !this.other.Contains(other.collider))
            {
                if(this.other.Count == 0)
                    animator.SetTrigger(Compress);
                this.other.Add(other.collider);
            }
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (this.other.Contains(other.collider))
            {
                this.other.Remove(other.collider);
                if(this.other.Count == 0)
                    animator.SetTrigger(DeCompress);
                
            }
        }
    }
}