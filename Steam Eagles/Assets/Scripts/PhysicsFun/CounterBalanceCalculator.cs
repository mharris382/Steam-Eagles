using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhysicsFun
{
    public class CounterBalanceCalculator : MonoBehaviour
    {
        public Rigidbody2D[] balloonBodies;
        public Rigidbody2D connectedBody;
        public Rigidbody2D[] leftCounterBalances;
        public Rigidbody2D[] rightCounterBalances;

        [LabelWidth(50)][HorizontalGroup("Mass")]
        public float balloonMass;
        
        
        [LabelWidth(50)][VerticalGroup("Mass/Weight")]  public float bodyMass;
        [LabelWidth(50)][VerticalGroup("Mass/Weight")]  public float leftMass;
        [LabelWidth(50)][VerticalGroup("Mass/Weight")]  public float rightMass;
        public float massTotal;
        public float massDifferenceVertical;
        public float massDifferenceHorizontal;
        Vector2[] _lastFrameVelocity = new Vector2[2];
        
        Vector2 ConnectedBodyCenterOfMass
        {
            get
            {
                return connectedBody.transform.TransformPoint(connectedBody.centerOfMass);
            }
        }


        [Button]
        void CalculateMasses()
        {
            float massTotal = 0;
            foreach (var leftCounterBalance in leftCounterBalances)
            {
                if (leftCounterBalance == null) continue;
                massTotal += leftCounterBalance.mass;
            }

            leftMass = massTotal;
            massTotal = 0;
            foreach (var rightCounterBalance in rightCounterBalances)
            {
                if (rightCounterBalance == null) continue;
                massTotal += rightCounterBalance.mass;
            }

            rightMass = massTotal;
            
            bodyMass = connectedBody.mass;

            massTotal = 0;
            foreach (var balloonBody in balloonBodies)
            {
                if (balloonBody == null) continue;
                var rbs = balloonBody.GetComponentsInChildren<Rigidbody2D>();
                foreach (var rb in rbs)
                {
                    massTotal += rb.mass;
                }
            }

            balloonMass = massTotal;
            this.massTotal = bodyMass + rightMass + leftMass;
            massDifferenceVertical = balloonMass - this.massTotal;
            massDifferenceHorizontal = leftMass - rightMass;
        }


        public bool useAutoMass = true;
        [InlineButton(nameof(SetDensity))]
        public float density = 1;

        IEnumerable<Rigidbody2D> GetCounterBalances()
        {
            foreach (var leftCounterBalance in leftCounterBalances)
            {
                if (leftCounterBalance == null) continue;
                yield return leftCounterBalance;
            }

            foreach (var rightCounterBalance in rightCounterBalances)
            {
                if (rightCounterBalance == null) continue;
                yield return rightCounterBalance;
            }
        }
        public void SetDensity()
        {
            var counterBalances = GetCounterBalances().ToArray();
            foreach (var counterBalance in counterBalances)
            {
                counterBalance.useAutoMass = useAutoMass;
                counterBalance.GetComponent<Collider2D>().density = density;
            }
            CalculateMasses();
        }
    }
}