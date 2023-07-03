using System.Collections;
using UnityEngine;

namespace Damage.Traps
{
    public class ForceEffect : TrapTriggerEffect
    {
        public float maxRadius = 5f;
        public AnimationCurve radiusCurve = AnimationCurve.Linear(0, 0, 1, 1);

        public float minForce = 10;
        public float maxForce = 100;
        public AnimationCurve forceCurve = AnimationCurve.Linear(0, 1, 1, 0);
        public float blowupTime = 0.25f;

        public CircleCollider2D circleCollider2D;
        public PointEffector2D pointEffector2D;

        public IEnumerator DoEffect()
        {
            for (float t = 0; t < 1; t+=Time.deltaTime/blowupTime)
            {
                var f = forceCurve.Evaluate(t ) ;
                circleCollider2D.radius = radiusCurve.Evaluate(t) * maxRadius;
                var force = Mathf.Lerp(minForce, maxForce, f);
                pointEffector2D.forceMagnitude = force;
                pointEffector2D.enabled = true;
                circleCollider2D.enabled = true;
                circleCollider2D.usedByEffector = true;
                yield return null;
            }   
            pointEffector2D.enabled = false;
            circleCollider2D.enabled = false;
        }
    }
}