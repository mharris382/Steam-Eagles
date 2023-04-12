using System.Collections.Generic;
using UnityEngine;

namespace Buildings
{
    public static class BoundsIntExtensions
    {
        public static BoundsInt Encapsulate(this BoundsInt bounds, BoundsInt other)
        {
            var min = new Vector3Int(Mathf.Min(bounds.xMin, other.xMin), Mathf.Min(bounds.yMin, other.yMin), Mathf.Min(bounds.zMin, other.zMin));
            var max = new Vector3Int(Mathf.Max(bounds.xMax, other.xMax), Mathf.Max(bounds.yMax, other.yMax), Mathf.Max(bounds.zMax, other.zMax));
            return new BoundsInt(min, max - min);
        }


        public static IEnumerable<Vector3Int> GetAllCells3D(this BoundsInt bounds)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    for (int z = bounds.zMin; z < bounds.zMax; z++)
                    {
                        yield return new Vector3Int(x, y, z);
                    }
                }
            }
        }
        public static IEnumerable<Vector3Int> GetAllCells2D(this BoundsInt bounds)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    yield return new Vector3Int(x, y, 0);
                }
            }
        }
        
        public static IEnumerable<Vector3Int> GetAllInteriorCells3D(this BoundsInt bounds)
        {
            if(bounds.size.x < 3 || bounds.size.y < 3)
                yield break;
            for (int x = bounds.xMin+1; x < bounds.xMax-1; x++)
            {
                for (int y = bounds.yMin+1; y < bounds.yMax-1; y++)
                {
                    for (int z = bounds.zMin+1; z < bounds.zMax-1; z++)
                    {
                        yield return new Vector3Int(x, y, z);
                    }
                }
            }
        }
        
        public static IEnumerable<Vector3Int> GetAllInteriorCells2D(this BoundsInt bounds)
        {
            if(bounds.size.x < 3 || bounds.size.y < 3)
                yield break;
            for (int x = bounds.xMin+1; x < bounds.xMax-1; x++)
            {
                for (int y = bounds.yMin+1; y < bounds.yMax-1; y++)
                {
                    yield return new Vector3Int(x, y, 0);
                }
            }
        }

        public static IEnumerable<Vector3Int> GetAllSideCells(this BoundsInt boundsInt)
        {
            var startTop = new Vector3Int(boundsInt.min.x, boundsInt.max.y);
            var startBottom = new Vector3Int(boundsInt.min.x, boundsInt.min.y);
           
            Vector3Int j = startBottom;
            for (Vector3Int i = startTop; i.x <= boundsInt.max.x; i.x++)
            {
                j.x++;
                yield return i;
                yield return j;
            }
            
            var startLeft = new Vector3Int(boundsInt.min.x, boundsInt.min.y);
            var startRight = new Vector3Int(boundsInt.max.x, boundsInt.min.y);
            startLeft.y += 1;
            startRight.y += 1;
            
            j = startLeft;
            for (Vector3Int i = startRight; i.y <= boundsInt.max.y-1; i.y++)
            {
                j.y++;
                yield return i;
                yield return j;
            }
        }

        public static IEnumerable<Vector3Int> GetAllLeftSideCells(this BoundsInt bounds)
        {
            var start = new Vector3Int(bounds.min.x, bounds.min.y);
            var end = new Vector3Int(bounds.min.x, bounds.max.y);
            for (Vector3Int i = start; i.y < end.y; i.y++)
            {
                yield return i;
            }
        }
        
        public static IEnumerable<Vector3Int> GetAllRightSideCells(this BoundsInt bounds)
        {
            var start = new Vector3Int(bounds.max.x, bounds.min.y);
            var end = new Vector3Int(bounds.max.x, bounds.max.y);
            for (Vector3Int i = start; i.y < end.y; i.y++)
            {
                yield return i;
            }
        }
        
        public static IEnumerable<Vector3Int> GetAllTopSideCells(this BoundsInt bounds)
        {
            var start = new Vector3Int(bounds.min.x, bounds.max.y);
            var end = new Vector3Int(bounds.max.x, bounds.max.y);
            for (Vector3Int i = start; i.x < end.x; i.x++)
            {
                yield return i;
            }
        }
        
        public static IEnumerable<Vector3Int> GetAllBottomSideCells(this BoundsInt bounds)
        {
            var start = new Vector3Int(bounds.min.x, bounds.min.y);
            var end = new Vector3Int(bounds.max.x, bounds.min.y);
            for (Vector3Int i = start; i.x < end.x; i.x++)
            {
                yield return i;
            }
        }
        
        
        public static bool IsCellOnLeftBoundary(this BoundsInt boundsInt, Vector3Int cell) => cell.x == boundsInt.min.x;
        public static bool IsCellOnRightBoundary(this BoundsInt boundsInt, Vector3Int cell) => cell.x == boundsInt.max.x;
        public static bool IsCellOnTopBoundary(this BoundsInt boundsInt, Vector3Int cell) => cell.y == boundsInt.max.y;

        public static bool IsCellOnBottomBoundary(this BoundsInt boundsInt, Vector3Int cell) => cell.y == boundsInt.min.y;

        public static bool IsCellOnBoundary(this BoundsInt boundsInt, Vector3Int cell) => IsCellOnLeftBoundary(boundsInt, cell) || IsCellOnRightBoundary(boundsInt, cell) || IsCellOnTopBoundary(boundsInt, cell) || IsCellOnBottomBoundary(boundsInt, cell);
    }
}