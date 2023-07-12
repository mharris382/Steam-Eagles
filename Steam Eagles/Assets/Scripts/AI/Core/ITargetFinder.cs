using System.Collections.Generic;
using CoreLib.Structures;
using UnityEngine;

namespace AI.Enemies
{
    public interface ITargetFinder
    {
        IEnumerable<Target> GetTargets();
    }
}