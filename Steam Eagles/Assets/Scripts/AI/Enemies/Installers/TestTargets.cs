using System.Collections.Generic;
using System.Linq;
using CoreLib.Interfaces;
using CoreLib.Structures;
using UnityEngine;

namespace AI.Enemies.Installers
{
    public class TestTargets : ITargetProvider
    {
        private HashSet<Transform> _transforms = new HashSet<Transform>();
        public void AddTestTarget(Transform transform)
        {
            if (!_transforms.Contains(transform) ) _transforms.Add(transform);
        }

        public void RemoveTestTarget(Transform transform)
        {
            _transforms.Remove(transform);
        }
        public IEnumerable<Target> GetTargets()
        {
            return _transforms.Select(t => new Target(t));
        }
    }
}