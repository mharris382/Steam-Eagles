using System;
using Buildings.Rooms;
using UnityEngine;
using Zenject;
using Object = System.Object;

namespace _EXP.PhysicsFun.ComputeFluid
{
    public class SamplePointHandle : IDisposable
    {
        public class Factory : PlaceholderFactory<Vector3, SamplePointHandle> { }
        private readonly SamplePoint _samplePoint;
        private Bounds _bounds;
        private bool _disposed;
        public Vector3 Position
        {
            get => _samplePoint.transform.position;
            set => _samplePoint.transform.position = _bounds.Contains(value) ? value : _bounds.ClosestPoint(value);
        }

        internal SamplePointHandle(Vector3 vector3, SamplePointFactory samplePoint, Room room)
        {
            _samplePoint = samplePoint.Create(vector3);
            _bounds = room.WorldSpaceBounds;
        }
        public void Dispose()
        {
            if(_disposed)
                return;
            _disposed = true;
            if(_samplePoint != null)
                GameObject.Destroy(_samplePoint.gameObject);
        }
    }
}