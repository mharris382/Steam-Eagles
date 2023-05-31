using UnityEngine;

namespace CoreLib.Extensions
{
    public static class GameObjectExtensions
    {
        public static string GetNameOrNull(this GameObject gameObject)
        {
            if (gameObject == null)
            {
                return "NULL";
            }
            return gameObject.name;
        }
    }
}