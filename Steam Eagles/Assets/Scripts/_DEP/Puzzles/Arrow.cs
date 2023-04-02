using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Puzzles
{
    public class Arrow : HoldableItem
    {
        public Collider2D arrowTip;
        public FixedJoint2D fixTipToArrow;
        public FixedJoint2D fixArrowToTip;

        void SetTipLive()
        {
            fixTipToArrow.enabled = true;
            fixArrowToTip.enabled = false;
            rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
            arrowTip.enabled = true;
        }
        
        void SetTipHit()
        {
            fixTipToArrow.enabled = false;
            fixArrowToTip.enabled = true;
            arrowTip.enabled = false;
            rb.isKinematic = true;
            rb.velocity = Vector2.zero;
        }
        public override void Thrown(Vector2 releaseForce, float releaseTorque)
        {

            float angle = Vector2.SignedAngle(Vector2.right, releaseForce.normalized);
            rb.rotation = angle;
            SetTipLive();
            
            arrowTip.OnCollisionEnter2DAsObservable().Where(t => t.contactCount > 0).Take(1).Subscribe(OnTipCollision).AddTo(this);
        }

        void OnTipCollision(Collision2D collision2D)
        {
            SetTipHit();
            if (collision2D.contactCount == 0) return;
            
            arrowTip.attachedRigidbody.position = collision2D.GetContact(0).point;
            arrowTip.attachedRigidbody.rotation = Vector2.SignedAngle(Vector2.right,  collision2D.contacts[0].normal);
            var fixedJoint2D = arrowTip.gameObject.AddComponent<FixedJoint2D>();
            arrowTip.attachedRigidbody.isKinematic = true;
        }
        
    }
}