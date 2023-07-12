using Buildings.Rooms;
using UnityEngine;
using Zenject;

namespace _EXP.PhysicsFun.ComputeFluid
{
    internal class SamplePointFactory
    {
        private readonly DiContainer _container;
        private readonly Room _room;
            
        private int _count;

        public SamplePointFactory(DiContainer container, Room room)
        {
            _container = container;
            _room = room;
        }


        public SamplePoint Create(Vector3 position)
        {
            var go = new GameObject($"SamplePoint {_count++}");
            go.transform.parent = _room.transform;
            var bounds = _room.WorldSpaceBounds;
            go.transform.position = bounds.Contains(position) ? position : bounds.ClosestPoint(position);
            return _container.InstantiateComponent<SamplePoint>(go);
        }
    }
}