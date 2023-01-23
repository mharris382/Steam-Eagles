using System;
using System.Collections.Generic;
using CoreLib;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PhysicsFun.Airships
{
    [RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(FixedJoint2D)), RequireComponent(typeof(LineRenderer))]
    public class CounterWeightAttachment : MonoBehaviour, ITilemapAttachment
    {
        private Rigidbody2D _rb;
        private LineRenderer _lr;
        private FixedJoint2D _fj;
        
        public Rigidbody2D rb => _rb ? _rb : _rb = GetComponent<Rigidbody2D>();
        public LineRenderer lr => _lr ? _lr : _lr = GetComponent<LineRenderer>();
        public FixedJoint2D fj => _fj ? _fj : _fj = GetComponent<FixedJoint2D>();

        private CounterWeight _attachedWeight;
        public CounterWeight AttachedWeight
        {
            get => _attachedWeight;
            set
            {
                _attachedWeight = value;
            }
        }

        Vector3[] _points = new Vector3[2];
        public void AttachToTilemap(Tilemap tm, Vector3Int cell)
        {
           
            fj.enabled = true;
        }

        public void Disconnect()
        {
            fj.enabled = false;
        }

        private void Update()
        {
            if (AttachedWeight == null)
            {
                lr.enabled = false;
            }
            else
            {
                lr.enabled = true;
                _points[0] = transform.position;
                _points[1] = AttachedWeight.transform.position;
                lr.positionCount = _points.Length;
                lr.SetPositions(_points);
            }
        }

        public void AddWeight(CounterWeight weightInstance)
        {
            weightInstance.onWeightAttached?.Invoke(rb);
            AttachedWeight = weightInstance;
            if (_attachedWeight.DistanceJoint2D != null)
            {
                _attachedWeight.DistanceJoint2D.connectedBody= rb;
                _attachedWeight.DistanceJoint2D.enabled = true;
            }
        }
    }
}