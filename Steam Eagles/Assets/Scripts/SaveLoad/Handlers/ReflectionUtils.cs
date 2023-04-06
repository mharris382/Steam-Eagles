using System;
using System.Collections.Generic;
using System.Linq;

namespace SaveLoad
{
    public static class ReflectionUtils
    {
        public static List<Type> GetConcreteTypes<T>()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var types = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface && !t.IsGenericTypeDefinition);

            return types.ToList();
        }
        
        public static List<T> GetConcreteInstances<T>()
        {
            var types = GetConcreteTypes<T>();

            var instances = new List<T>();

            foreach (var type in types)
            {
                var instance = Activator.CreateInstance(type);

                if (instance is T tInstance)
                {
                    instances.Add(tInstance);
                }
            }

            return instances;
        }
    }
}