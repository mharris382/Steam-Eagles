using System.Collections.Generic;
using UnityEngine;

namespace CoreLib.Extensions
{
    public static class VectorExtensions
    {
        private static Vector2Int[] neighborDirections2 = new[] {Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left};
        private static Vector3Int[] neighborDirections3 = new[] {Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left};
        
        public static IEnumerable<Vector2Int> Neighbors(this Vector2Int position)
        {
            foreach (var direction in neighborDirections2)
                yield return position + direction;
        }
        
        public static IEnumerable<Vector3Int> Neighbors(this Vector3Int position)
        {
            foreach (var direction in neighborDirections3)
                yield return position + direction;
        }



        public static IEnumerable<Vector2Int> GetPositionsInRadius(this Vector2Int center, int radius)
        {
            int x = radius;
            int y = 0;
            int decisionOver2 = 1 - x;
            while (y <= x)
            {
                yield return new Vector2Int(center.x + x, center.y + y);
                yield return new Vector2Int(center.x + y, center.y + x);
                yield return new Vector2Int(center.x - x, center.y + y);
                yield return new Vector2Int(center.x - y, center.y + x);
                yield return new Vector2Int(center.x - x, center.y - y);
                yield return new Vector2Int(center.x - y, center.y - x);
                yield return new Vector2Int(center.x + x, center.y - y);

                y++;
                
                if (decisionOver2 <= 0)
                {
                    decisionOver2 += 2 * y + 1;
                }
                else
                {
                    x--;
                    decisionOver2 += 2 * (y - x) + 1;
                }
            }
        }
    }
    
    
}