using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Experimental
{
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Gear : MonoBehaviour
    {
        public Gear parentGear;
        public bool axelConnection = false;
        public float connectAngle = 15;
        public bool IsRoot() => System != null && System.rootGear == this;

        public Vector3 Center => transform.position;
        

        public List<Gear> childGears;
        public List<Gear> axelChildGears = new  List<Gear>();

        public Gears System
        {
            get => _system;
            set => _system = value;
        }

        public Rigidbody2D Rb => _rb ? _rb : _rb = GetComponent<Rigidbody2D>();

        public CircleCollider2D CircleCollider2D =>
            _circleCollider2D ? _circleCollider2D : _circleCollider2D = GetComponent<CircleCollider2D>();

        private bool _disconnected;
        public bool Disconnected
        {
            get => _disconnected;
            set
            {
                _disconnected = value;
            }
        }
        public float AngularVelocity
        {
            set
            {
                Rb.angularVelocity = value;
                foreach (var childGear in childGears)
                {
                    if (childGear == null)  continue;
                    childGear.AngularVelocity = ComputeChildGearVelocity(value, childGear);
                }

                foreach (var gear in axelChildGears)
                {
                    if (gear == null) continue;
                    gear.AngularVelocity = gear.Disconnected ? 0 : value;
                }
            }
        }

        public float Radius
        {
            get => CircleCollider2D.radius * transform.lossyScale.x;
        }

        private float ComputeChildGearVelocity(float value, Gear childGear)
        {
            return -value;
        }


     

        private CircleCollider2D _circleCollider2D;
        private Rigidbody2D _rb;
        private Gears _system;

        private void Awake()
        {
            _system = GetComponent<Gears>();
            childGears.Clear();
            tag = "Gear";
        }
    }



}