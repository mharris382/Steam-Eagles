using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoreLib;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Buildings.Mechanisms
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SliderJoint2D))]
    public class ElevatorMechanism : SliderJointMechanism, IElevatorMechanism
    {
        public int speedMultiplier = -1;
        public bool debug;
        public float elevatorSpeed = 1;
        public float acceleration = 1;
        public float deceleration = 1;
        public float stopTime = 1;
        public float slowDistance = 0.1f;
        public AnimationCurve stopCurve = AnimationCurve.Linear(0, 0, 1, 1);
        private Rigidbody2D _rigidbody2D;
        public Rigidbody2D Rigidbody2D => _rigidbody2D != null ? _rigidbody2D : _rigidbody2D = GetComponent<Rigidbody2D>();

        public ElevatorStop GetStop(int floor) => stops[floor];

        public override float[] GetSaveState()
        {
            throw new System.NotImplementedException();
        }

        public override void LoadSaveState(float[] saveState)
        {
            throw new System.NotImplementedException();
        }

        private float _percent;
         
        [ShowInInspector, PropertyRange(0,1)]
        public float Percent
        {
            get
            {
                return _percent;
                // var connectedAnchorWS = SliderJoint2D.connectedBody.transform.TransformPoint(SliderJoint2D.connectedAnchor);
                // var anchorWS = SliderJoint2D.attachedRigidbody.transform.TransformPoint(SliderJoint2D.anchor);
                // var connectedAnchorY = connectedAnchorWS.y;
                // var anchorY = anchorWS.y;
                // var minY = SliderJoint2D.limits.min + anchorY;
                // var maxY = SliderJoint2D.limits.max + anchorY;
                // var curY = transform.position.y;
                // curY = Mathf.Clamp(minY, maxY, curY);
                // return Mathf.InverseLerp(minY, maxY, curY);
            }
            set
            {
                _percent = Mathf.Clamp(value, 0, 1);
                // var connectedAnchorWS = SliderJoint2D.connectedBody.transform.TransformPoint(SliderJoint2D.connectedAnchor);
                // var anchorWS = SliderJoint2D.attachedRigidbody.transform.TransformPoint(SliderJoint2D.anchor);
                // var connectedAnchorY = connectedAnchorWS.y;
                // var anchorY = anchorWS.y;
                // var minY = SliderJoint2D.limits.min + anchorY;
                // var maxY = SliderJoint2D.limits.max + anchorY;
                // var curY = Mathf.Lerp(minY, maxY, value);
                // var pos = transform.position;
                // pos.y = curY;
                // transform.position = pos;
            }
        }

        public float TotalDistance => SliderJoint2D.limits.max - SliderJoint2D.limits.min;
        public bool IsMoving => isMoving;
        public IEnumerable<string> GetFloorNames() => GetStops().Select(t => t.name);


        [Button] void AddStop()
        {
            stops.Add(new ElevatorStop()
            {
                percent = _percent,
                name = ""
            });
            stops.Sort();
        }

        [Serializable]
        public class ElevatorStop : IComparable<ElevatorStop>
        {
            [Range(0,1)]
            public float percent;

            public string name;

            public int CompareTo(ElevatorStop other)
            {
                if (ReferenceEquals(this, other)) return 0;
                if (ReferenceEquals(null, other)) return 1;
                return percent.CompareTo(other.percent);
            }

            private bool IsPlaying => Application.isPlaying;

            [Button, DisableInEditorMode]
            void GoToStop()
            {
                if (IsPlaying)
                {
                    #if UNITY_EDITOR
                    var elevator = (ElevatorMechanism)UnityEditor.Selection.activeGameObject.GetComponent<ElevatorMechanism>();
                    elevator.GoToStop(this);
                    #endif
                }
            }
        }

        [TableList] [ValidateInput(nameof(ValidateStops))] [SerializeField]
        private List<ElevatorStop> stops;

        bool ValidateStops(List<ElevatorStop> elevatorStops)
        {
            elevatorStops.Sort();
            if (elevatorStops.Count < 2)
            {
                elevatorStops.Clear();
                elevatorStops.Add(new ElevatorStop()
                {
                    percent = 0,
                    name="Bottom"
                });
                elevatorStops.Add(new ElevatorStop()
                {
                    percent = 1,
                    name = "Top"
                });
            }

            return true;
        }

        private void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider2D>();
            var bounds = box.bounds;
            var points = bounds.GetPoints().ToArray();
            Vector3 displayedOffset = transform.position+((Vector3)box.offset);
            var maxOffset = SliderJoint2D.limits.max;
            Gizmos.matrix = transform.worldToLocalMatrix;
            displayedOffset = Vector3.Lerp(displayedOffset, displayedOffset + (Vector3.up * maxOffset), _percent);
            for (int i = 1; i < points.Length; i++)
            {
                var pnt0 = points[i - 1] + displayedOffset;
                var pnt1 = points[i] + displayedOffset ;
                Gizmos.color = Color.red;
                Gizmos.DrawLine(pnt0, pnt1);
            }
            
            var connectedAnchorWS = SliderJoint2D.connectedBody.transform.TransformPoint(SliderJoint2D.connectedAnchor);
            var anchorWS = SliderJoint2D.attachedRigidbody.transform.TransformPoint(SliderJoint2D.anchor);
            var connectedAnchorY = connectedAnchorWS.y;
            var anchorY = anchorWS.y;
            var minY = SliderJoint2D.limits.min + anchorY;
            var maxY = SliderJoint2D.limits.max + anchorY;
            var p0 = transform.position;
            var p1 = p0;
            p1.y += minY;
            p0.y += maxY;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(p0, 0.24f);
            Gizmos.color = Color.Lerp(Color.yellow,Color.white, 0.45f);
            Gizmos.DrawWireSphere(p1, 0.24f);
        }

        public IEnumerable<ElevatorStop> GetStops() => stops;

        public void GoToStop(ElevatorStop stop)
        {
            if (isMoving)
            {
                Debug.LogError("Elevator is already moving");
                throw new UnityException("Elevator is already moving");
            }
            StartCoroutine(MoveElevatorToStop(stop));
        }

        float PercentToHeight(float percent)
        {
            var anchor = SliderJoint2D.anchor;
            float maxHeight = TotalDistance + anchor.y;
            float minHeight = anchor.y;
            return Mathf.Lerp(minHeight, maxHeight, percent);
        }

        float HeightToPercent(float height)
        {
            var anchor = SliderJoint2D.anchor;
            float maxHeight = TotalDistance + anchor.y;
            float minHeight = anchor.y;
            height = Mathf.Clamp(height, minHeight, maxHeight);
            return Mathf.InverseLerp(minHeight, maxHeight, height);
        }

        private float CurrentHeight => Rigidbody2D.position.y;

        IEnumerator StopElevatorOnFloor(float targetHeight, float speed)
        {
            for (float t = 0; t < 1; t += Time.deltaTime / stopTime)
            {
                float curSpeed = Mathf.Lerp(speed, 0, stopCurve.Evaluate(t));
                Rigidbody2D.velocity = new Vector2(0, curSpeed);
                yield return null;
            }
            Rigidbody2D.position = new Vector2(Rigidbody2D.position.x, targetHeight);
        }
        IEnumerator MoveElevatorToStop(ElevatorStop stop)
        {
            isMoving = true;
            var pos = Rigidbody2D.position;
            var curPercent = HeightToPercent(pos.x);
            var speed = Rigidbody2D.velocity.y;
            SliderJoint2D.useMotor = false;
            var targetPercent = stop.percent;
            var targetHeight = PercentToHeight(targetPercent);
            
            const float STOP_DISTANCE = 0.01f;
            while (Mathf.Abs(targetHeight - CurrentHeight) > STOP_DISTANCE)
            {
                var dir = Mathf.Sign(targetHeight - CurrentHeight);
                if (Mathf.Abs(targetHeight - CurrentHeight) < slowDistance)
                {
                    yield return StopElevatorOnFloor(targetHeight, speed);
                }
                else
                {
                    speed += acceleration * Time.deltaTime * dir;
                    speed = Mathf.Clamp(speed, -elevatorSpeed, elevatorSpeed);
                    Rigidbody2D.velocity = new Vector2(0, speed);
                    yield return null;    
                }
                
            }

            isMoving = false;
            SliderJoint2D.useMotor = true;
            SliderJoint2D.motor = new JointMotor2D();
        }

        public bool MoveToFloor(int floor)
        {
            if (isMoving)
            {
                return false;
            }
            var stop = stops[floor];
            StartCoroutine(MoveElevatorToStop(stop));
            return true;
        }
        private bool isMoving;
    }

    public static class BoundsExtensions
    {
        public static IEnumerable<Vector3> GetPoints(this Bounds bounds)
        {
            yield return  bounds.min;
            yield return new Vector3(bounds.min.x, bounds.max.y);
            yield return bounds.max;
            yield return new Vector3(bounds.max.x, bounds.max.y);
            yield return new Vector3(bounds.max.x, bounds.min.y);
            yield return bounds.min;
        }
    }
}