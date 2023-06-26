using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ObjectLabelMapping
{
    public delegate IEnumerable<Object> GetPossibleMappedObjects<T>(T component);
    public static class ObjectLabelMapExtensions
    {
        private delegate IEnumerable<Object> GetPossibleMappedObjects(Object obj);

        private static Dictionary<Type, GetPossibleMappedObjects> _mappedObjectsMap = new();
        public class GlobalObjectMapNotAssignedException : NullReferenceException   { }

        public class ObjectNotLinkedToMapException : NullReferenceException
        {
            public ObjectNotLinkedToMapException(string message) : base(message) { }
        }
    
        internal static GlobalObjectMap globalObjectMap;
        internal static GlobalValueMap valueMap;


        public static void Link(this IParameterMapProvider parameterMapProvider)
        {
            foreach (var mappedObject in parameterMapProvider.GetParameters()) globalObjectMap.Link(mappedObject);
        }

        private static bool TryGetLabel(IEnumerable<Object> possibleObjects, string param, out string label)
        {
            label = "";
            foreach (var obj in possibleObjects)
            {
                if (globalObjectMap.TryGetLabel(obj, param, out label))
                {
                    return true;
                }
            }
            return false;
        }

        public static void RegisterFuncToType<T>(GetPossibleMappedObjects<T> possibleMappedObjects) where  T : class => _mappedObjectsMap.Add(typeof(T), o => possibleMappedObjects(o as T));

        public static bool TryGetLabel<T>(this T component, string parameter, Func<T, IEnumerable<Object>> possibleMappedObjects, out string label) where  T: Component
        {
            label = null;
            if (component == null) return false;
            if (globalObjectMap == null)
                throw new GlobalObjectMapNotAssignedException();
            
            if(TryGetLabel(possibleMappedObjects(component), parameter, out label))
                return true;
            
            if(valueMap == null)
                throw new GlobalObjectMapNotAssignedException();
            
            return component.gameObject.TryGetLabel(parameter, out label);
        }
        public static bool TryGetLabel(this SpriteRenderer spriteRenderer, string parameter, out string label) => TryGetLabel<SpriteRenderer>(spriteRenderer, parameter, GetPossibleLabeledObjects, out label);
        public static bool TryGetLabel(this Collider2D collider2D, string parameter, out string label) => TryGetLabel(collider2D, parameter, GetPossibleLabeledObjects, out label);
        public static bool TryGetLabel(this Rigidbody2D collider2D, string parameter, out string label) => TryGetLabel(collider2D, parameter, GetPossibleLabeledObjects, out label);

        public static bool TryGetLabel(this GameObject gameObject, string parameter, out string label)
        {
            if (globalObjectMap == null)
                throw new GlobalObjectMapNotAssignedException();
            
            if(valueMap == null)
                throw new GlobalObjectMapNotAssignedException();

            if (globalObjectMap.TryGetLabel(gameObject, parameter, out label))
            {
                return true;
            }

            if (valueMap.TryGetLabel(gameObject, parameter, out label))
                return true;

            return false;
            Debug.LogError($"Object {gameObject.name} is not linked to any label for parameter {parameter}", gameObject);
        }

        
        private static IEnumerable<Object> GetPossibleLabeledObjects(this SpriteRenderer value)
        {
            yield return value.sprite;
            yield return value.material;
        }
        
        private static IEnumerable<Object> GetPossibleLabeledObjects(this Renderer value)
        {
            yield return value.material;
        }
        private static IEnumerable<Object> GetPossibleLabeledObjects(this Rigidbody2D value)
        {
            yield return value.sharedMaterial;
        }
        private static IEnumerable<Object> GetPossibleLabeledObjects(this Collider2D value)
        {
            yield return value.sharedMaterial;
        }
        private static IEnumerable<Object> GetPossibleLabeledObjects(this Collider value)
        {
            yield return value.material;
        }
    }
}