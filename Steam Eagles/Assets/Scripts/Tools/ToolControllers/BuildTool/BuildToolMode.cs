using System.Collections.Generic;
using UnityEngine;

namespace Tools.BuildTool
{
    public abstract class BuildToolMode : IBuildPathStrategy
    {
        public abstract string ModeName { get; }
        public abstract bool BuildPath(BuildPathInfo info, ref List<Vector3Int> path);
    }
}