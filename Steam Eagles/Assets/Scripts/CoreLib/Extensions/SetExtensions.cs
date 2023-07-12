using System.Collections.Generic;

namespace CoreLib.Extensions
{
    public static class SetExtensions
    {
        public static bool SafeAdd<T>(this HashSet<T> set, T value)
        {
            if (set.Contains(value)) return false;
            set.Add(value);
            return true;
        }
    }
}