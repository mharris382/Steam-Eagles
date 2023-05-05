using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Buildings.Mechanisms
{
    public class ElevatorStopEvaluator : ITickable
    {
        private readonly ElevatorMechanism _elevatorMechanism;
        private readonly List<ElevatorMechanism.ElevatorStop> _stops;
        private readonly Vector3 _elevatorLocalPositionStart;
        private readonly Vector3 _elevatorLocalPositionEnd;

        public ElevatorStopEvaluator(ElevatorMechanism elevatorMechanism)
        {
            _elevatorMechanism = elevatorMechanism;
            _elevatorLocalPositionStart = _elevatorMechanism.transform.localPosition;// - (Vector3)_elevatorMechanism.SliderJoint2D.connectedAnchor;
            _stops =_elevatorMechanism.GetStops().ToList();
            var maxLocalHeight = _elevatorMechanism.SliderJoint2D.limits.max;
            var minLocalHeight = _elevatorMechanism.SliderJoint2D.limits.min;
            _elevatorLocalPositionEnd = _elevatorLocalPositionStart + (Vector3.up * maxLocalHeight) - (Vector3.up * minLocalHeight);
        }
        public Vector3 GetElevatorLocalPositionForStop(int stopIndex)
        {
            var stop = _stops[stopIndex];
            return Vector3.Lerp(_elevatorLocalPositionStart, _elevatorLocalPositionEnd, stop.percent);
        }


        public void Tick()
        {
            if (_elevatorMechanism.debug)
            {
                var parentPos = _elevatorMechanism.transform.parent.position;
                Debug.DrawLine(parentPos + _elevatorLocalPositionEnd, parentPos + _elevatorLocalPositionStart, Color.red);
            }
        }
    }
}