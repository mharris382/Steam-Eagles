using System;
using UnityEngine;

namespace CoreLib
{
    [Flags]
    public enum CardinalDirections
    {
        NONE,UP,DOWN,LEFT, RIGHT
    }


    public static class CardinalDirectionsExtensions
    {
        public static Vector2 ToVector2(this CardinalDirections direction)
        {
            switch (direction)
            {
                case CardinalDirections.UP:
                    return Vector2.up;
                case CardinalDirections.DOWN:
                    return Vector2.down;
                case CardinalDirections.LEFT:
                    return Vector2.left;
                case CardinalDirections.RIGHT:
                    return Vector2.right;
                default:
                    return Vector2.zero;
            }
        } 
        
        public static Vector2Int ToVector2Int(this CardinalDirections direction)
        {
               switch (direction)
                {
                    case CardinalDirections.UP:
                        return Vector2Int.up;
                    case CardinalDirections.DOWN:
                        return Vector2Int.down;
                    case CardinalDirections.LEFT:
                        return Vector2Int.left;
                    case CardinalDirections.RIGHT:
                        return Vector2Int.right;
                    default:
                        return Vector2Int.zero;
                }
        } 
        
        public static Vector3 ToVector3(this CardinalDirections direction)
        {
            switch (direction)
            {
                case CardinalDirections.UP:
                    return Vector3.up;
                case CardinalDirections.DOWN:
                    return Vector3.down;
                case CardinalDirections.LEFT:
                    return Vector3.left;
                case CardinalDirections.RIGHT:
                    return Vector3.right;
                default:
                    return Vector3.zero;
            }
        }
        
        public static Vector3Int ToVector3Int(this CardinalDirections direction)
        {
            switch (direction)
            {
                case CardinalDirections.UP:
                    return Vector3Int.up;
                case CardinalDirections.DOWN:
                    return Vector3Int.down;
                case CardinalDirections.LEFT:
                    return Vector3Int.left;
                case CardinalDirections.RIGHT:
                    return Vector3Int.right;
                default:
                    return Vector3Int.zero;
            }
        }
        
        public static Vector2 MoveInDirection(this Vector2 position, CardinalDirections direction) => position + direction.ToVector2();

        public static Vector2Int MoveInDirection(this Vector2Int position, CardinalDirections direction) => position + direction.ToVector2Int();

        public static Vector3 MoveInDirection(this Vector3 position, CardinalDirections direction) => position + direction.ToVector3();

        public static Vector3Int MoveInDirection(this Vector3Int position, CardinalDirections direction) => position + direction.ToVector3Int();
    }
}