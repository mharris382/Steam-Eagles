using System.Collections.Generic;
using UnityEngine;

namespace Tools.BuildTool
{
    public class LineBuildMode : BuildToolMode
    {
        public override string ModeName => "Line";
        public override bool BuildPath(BuildPathInfo info, ref List<Vector3Int> path)
        {
            if (path == null)
            {
                path = new List<Vector3Int>();
            }
            var difference = info.end - info.start;
            var slope = difference.y / (difference.x != 0 ? difference.x : 1);
            var yIntercept = info.start.y - slope * info.start.x;
            var x = info.start.x;
            var y = info.start.y;
            while (x < info.end.x)
            {
                x++;
                y = slope * x + yIntercept;
                path.Add(new Vector3Int(x, Mathf.RoundToInt(y), 0));
            }
            return true;
        }
    }
}