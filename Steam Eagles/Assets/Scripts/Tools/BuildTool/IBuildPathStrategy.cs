using System.Collections.Generic;
using UnityEngine;

namespace Tools.BuildTool
{
    public interface IBuildPathStrategy
    {
        public bool BuildPath(BuildPathInfo info, ref List<Vector3Int> path);
    }
}