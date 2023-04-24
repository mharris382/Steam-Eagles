using System.Collections.Generic;
using UnityEngine;

namespace Tools.BuildTool
{
    public class PathBuildMode : BuildToolMode
    {
        public override string ModeName => "Path";
        public override bool BuildPath(BuildPathInfo info, ref List<Vector3Int> path)
        {
            if (path == null)
            {
                path = new List<Vector3Int>();
            }
            path.Add(info.start);
            path.AddRange(info.building.Map.GetPath((Vector2Int)info.start ,(Vector2Int)info.end, info.layer));
            return true;
        }
    }
    
    
    
}