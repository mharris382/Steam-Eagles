using System.Collections;
using System.Collections.Generic;
using CoreLib;
using UnityEngine;

namespace Buildings.Mechanisms
{
    public class ElevatorMover : IElevatorMechanism
    {
        private readonly ElevatorStopEvaluator _elevatorStopEvaluator;
        private readonly ElevatorMechanism _elevatorMechanism;
        private readonly CoroutineCaller _coroutineCaller;

        public ElevatorMover(CoroutineCaller coroutineCaller, ElevatorStopEvaluator elevatorStopEvaluator, ElevatorMechanism elevatorMechanism)
        {
            _coroutineCaller = coroutineCaller;
            _elevatorStopEvaluator = elevatorStopEvaluator;
            _elevatorMechanism = elevatorMechanism;
        }
        public bool MoveToFloor(int floor)
        {
            if(IsMoving)
            {
                return false;
            }
            _coroutineCaller.StartCoroutine(MoveElevatorToFloor(floor));
            return true;
            throw new System.NotImplementedException();
        }

        IEnumerator SlowElevatorToStop(int floor)
        {
            var endPosition = _elevatorStopEvaluator.GetElevatorLocalPositionForStop(floor);
            var startPosition = _elevatorMechanism.transform.localPosition;
            for (float t = 0; t < 1; t += Time.deltaTime / _elevatorMechanism.stopTime)
            {
                yield return null;
                _elevatorMechanism.transform.localPosition = Vector3.Lerp(startPosition, endPosition, t);
            }
            _elevatorMechanism.transform.localPosition = endPosition;
        }
        IEnumerator MoveElevatorToFloor(int floor)
        {
            IsMoving = true;
            var stop = _elevatorStopEvaluator.GetElevatorLocalPositionForStop(floor);
            var stopY = stop.y;
            var currentY = _elevatorMechanism.transform.localPosition.y;
            var direction = Mathf.Sign(currentY-stopY);
            var currentSpeed = 0.0f;
            var targetSpeed = _elevatorMechanism.elevatorSpeed * direction;
            float CurrentDistance() => Mathf.Abs(stopY - _elevatorMechanism.transform.localPosition.y);
            const float STOPPING_DISTANCE = 0.05f;
            
            while (CurrentDistance() > STOPPING_DISTANCE)
            {
                if(direction < 0 && _elevatorMechanism.transform.localPosition.y > stopY)
                    break;
                if(direction > 0 && _elevatorMechanism.transform.localPosition.y < stopY)
                    break;
                if (CurrentDistance() > _elevatorMechanism.slowDistance)
                {
                    yield return SlowElevatorToStop(floor);
                    break;
                }
                else
                {
                    currentSpeed += _elevatorMechanism.acceleration * Time.deltaTime * direction;
                    currentSpeed = Mathf.Clamp(currentSpeed, -_elevatorMechanism.elevatorSpeed, _elevatorMechanism.elevatorSpeed);
                    _elevatorMechanism.SliderJoint2D.motor = new JointMotor2D()
                    {
                        motorSpeed = currentSpeed * (_elevatorMechanism.speedMultiplier),
                        maxMotorTorque = 100000
                    };
                    _elevatorMechanism.SliderJoint2D.useMotor = true;
                    yield return null;
                }
            }
            _elevatorMechanism.transform.localPosition = stop;
            _elevatorMechanism.SliderJoint2D.useMotor = true;
            _elevatorMechanism.SliderJoint2D.motor = new JointMotor2D()
            {
                motorSpeed = 0,
                maxMotorTorque = 100000
            };
            IsMoving = false;
        }

        public bool IsMoving { get; private set; }
        public IEnumerable<string> GetFloorNames() => _elevatorMechanism.GetFloorNames();
    }
}