using System;
using CoreLib;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhysicsFun.Airships
{
    public class AirshipFlightController : MonoBehaviour
    {
        [ValidateInput(nameof(ValidateBalloonCounterLiftEffector)), Required]
        public AreaEffector2D balloonCounterLiftEffector;
        
        [ValidateInput(nameof(ValidateAirshipBody)), Required]
        public Rigidbody2D airshipBody;

        public Transform heightTarget;

        private BoxCollider2D _airshipArea;
        private BoxCollider2D _liftArea;
        private Rect _airshipRect;
        private Rect _liftRect;
        private void Awake()
        {
            if (!HasRequiredValues())
            {
                Debug.LogError("Airship Flight controller missing required values", this);
            }

            _airshipArea = airshipBody.GetComponent<BoxCollider2D>();
            _liftArea = balloonCounterLiftEffector.GetComponent<BoxCollider2D>();
            _airshipRect = new Rect(_airshipArea.offset.x, _airshipArea.offset.y, _airshipArea.size.x, _airshipArea.size.y);
            _liftRect = new Rect(_liftArea.offset.x, _liftArea.offset.y, _liftArea.size.x, _liftArea.size.y);
        }

        private void Update()
        {
            if(!HasRequiredValues())return;
            
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying == false) return;
            if (!HasRequiredValues()) return;
            Gizmos.color = Color.red;
            RectUtilities.DrawGizmos(_airshipRect, airshipBody.transform);
            
            Gizmos.color = Color.blue;
            RectUtilities.DrawGizmos(_liftRect, balloonCounterLiftEffector.transform);
        }

        bool HasRequiredValues()
        {
            if (balloonCounterLiftEffector == null)
            {
                return false;
            }

            if (airshipBody == null)
            {
                return false;
            }

            if (heightTarget == null)
            {
                return false;
            }
            return true;
        }
        bool ValidateAirshipBody(Rigidbody2D rb, string msg)
        {
            if (rb == null) return false;
            if (rb.bodyType != RigidbodyType2D.Dynamic) return false;
            var box = rb.GetComponent<BoxCollider2D>();
            if (box == null || !box.isTrigger)
            {
                msg = "Airship body must have a box collider 2D with isTrigger set to true";
                return false;
            }
            return true;
        }
        bool ValidateBalloonCounterLiftEffector(AreaEffector2D balloonCounterLiftEffector, string msg)
        {
            if (balloonCounterLiftEffector == null) return false;
            if (balloonCounterLiftEffector.GetComponent<BoxCollider2D>() == null)
            {
                msg = "Balloon Counter Lift Effector must have a Box Collider 2D";
                return false;
            }

            return true;
        }
    }
}