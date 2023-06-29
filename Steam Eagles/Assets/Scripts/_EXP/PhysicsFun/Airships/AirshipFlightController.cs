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
        public float raiseSpeed = 1f;
        public float lowerSpeed = 1f;
        public float minHeight = 10;
        private Rigidbody2D _balloonCounterLiftEffectorRigidbody2D;
        private BoxCollider2D _airshipArea;
        private BoxCollider2D _liftArea;
        private Rect _airshipRect;
        private Rect _liftRect;
        private void Awake()
        {
            if(heightTarget==null)
            {
                heightTarget = new GameObject($"{name} Height Target").transform;
                heightTarget.position = balloonCounterLiftEffector.transform.position;
            }
            _balloonCounterLiftEffectorRigidbody2D = balloonCounterLiftEffector.GetComponent<Rigidbody2D>();
            _airshipArea = airshipBody.GetComponent<BoxCollider2D>();
            _liftArea = balloonCounterLiftEffector.GetComponent<BoxCollider2D>();
            _airshipRect = new Rect(_airshipArea.offset.x, _airshipArea.offset.y, _airshipArea.size.x, _airshipArea.size.y);
            _liftRect = new Rect(_liftArea.offset.x, _liftArea.offset.y, _liftArea.size.x, _liftArea.size.y);
            if (!HasRequiredValues())
            {
                Debug.LogError("Airship Flight controller missing required values", this);
            }
        }

        private void Update()
        {
            if(!HasRequiredValues())return;
           
            var targetPosition = heightTarget.position;
            var airshipPosition = airshipBody.worldCenterOfMass;
            var targetY = targetPosition.y;
            targetY = Mathf.Max(minHeight, targetY);
            var airshipY = airshipPosition.y;
            bool isTargetAbove = targetY > airshipY;
            float speed = isTargetAbove ? raiseSpeed : lowerSpeed;

            var liftPosition = _balloonCounterLiftEffectorRigidbody2D.position;
            var liftY = liftPosition.y;
            var newLiftY = Mathf.MoveTowards(liftY, targetY, speed * Time.deltaTime);
            
            Vector2 newPosition = new Vector2(airshipPosition.x, newLiftY);
            _balloonCounterLiftEffectorRigidbody2D.position = newPosition;
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