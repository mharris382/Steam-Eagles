using System;
using System.Collections.Generic;
using CoreLib;
using QuikGraph;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Experimental
{
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D), typeof(HingeJoint2D))]
    public class Gear : MonoBehaviour, IGear2D
    {
        [Header("Gear Settings"),Min(4)]
        public int teethCount = 10;
        public bool axelConnection = false;
        
        [Header("Connections")]
        public Gear parentGear;
        public List<Gear> childGears;
        public List<Gear> axelChildGears = new  List<Gear>();
        
        private bool _disconnected;
        private CircleCollider2D _circleCollider2D;
        private Rigidbody2D _rb;
        private Gears _system;

        
        public Vector3 Center => transform.position;
        public bool IsAxelConnection { get; }
        public int TeethCount { get; }
        public float Radius => CircleCollider2D.radius * transform.lossyScale.x;
        
        
        public float AngularVelocity
        {
            get => Rb.angularVelocity;
            set
            {
                Rb.angularVelocity = value;
                foreach (var childGear in childGears)
                {
                    if (childGear == null)  continue;
                    childGear.AngularVelocity = !childGear.IsAxelConnection ? ComputeChildGearVelocity(Rb.angularVelocity, childGear) :  Rb.angularVelocity;
                }

                foreach (var gear in axelChildGears)
                {
                    if (gear == null) continue;
                    gear.AngularVelocity = Rb.angularVelocity;
                }
            }
        }


        

        public float Rotation
        {
            get => Rb.rotation;
            set => Rb.rotation = value;
        }

        


        public Gears System
        {
            get => _system;
            set => _system = value;
        }

        public Rigidbody2D Rb => _rb ? _rb : _rb = GetComponent<Rigidbody2D>();

        public CircleCollider2D CircleCollider2D =>
            _circleCollider2D ? _circleCollider2D : _circleCollider2D = GetComponent<CircleCollider2D>();

       public bool Disconnected
        {
            get => _disconnected;
            set
            {
                _disconnected = value;
            }
        }


        private float ComputeChildGearVelocity(float value, Gear childGear)
        {
            int teethDriver = this.teethCount;
            int teethDriven = childGear.teethCount;
            float ratio = (float) teethDriver / teethDriven;
            return -(value * ratio);
        }
     

        public bool IsRoot() => System != null && System.rootGear == this;


        private void Awake()
        {
            _system = GetComponent<Gears>();
            childGears.Clear();
            tag = "Gear";
        }

        private void Start()
        {
            foreach (var gear in axelChildGears)
            {
                childGears.Remove(gear);
            }
        }

        private void OnDrawGizmos()
        {
            bool hasRequired = parentGear != null && parentGear != this;
            if (!hasRequired) return;
            Gizmos.color = Color.red.SetAlpha(0.5f);

            void DrawTeeth()
            {
                var center = Center;
                var radius = Radius;
                var angle = 360f / teethCount;
                var startAngle = Rotation;
                for (var i = 0; i < teethCount; i++)
                {
                    var start = center + Quaternion.Euler(0, 0, startAngle) * Vector3.up * radius;
                    var end = center + Quaternion.Euler(0, 0, startAngle + angle) * Vector3.up * radius;
                    Gizmos.DrawWireSphere(Vector3.Lerp(start, end, 0.5f), 0.1f);
                    Gizmos.DrawLine(start, end);
                    startAngle += angle;
                }
            }
        }
    }


}