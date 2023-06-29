using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ObjectLabelMapping
{

    



    public class GlobalObjectMap
    {
        private readonly Dictionary<object, Dictionary<string, string>> _mappings =
            new Dictionary<object, Dictionary<string, string>>();

        public GlobalObjectMap()
        {
            ObjectLabelMapExtensions.globalObjectMap = this;
        }
        public bool Link(IParameterMappedObject parameterMappedObject)
        {
            if (parameterMappedObject == null || parameterMappedObject.LinkedObject == null)
                return false;

            var obj = parameterMappedObject.LinkedObject;
            if (!_mappings.ContainsKey(obj))
            {
                _mappings.Add(obj, new Dictionary<string, string>());
            }

            var mapping = _mappings[obj];
            if (mapping.ContainsKey(parameterMappedObject.ParameterName))
            {
                LogKeyFound();
                return mapping[parameterMappedObject.ParameterName] == parameterMappedObject.Label;
            }

            mapping.Add(parameterMappedObject.ParameterName, parameterMappedObject.Label);

            void LogKeyFound()
            {
                if (mapping[parameterMappedObject.ParameterName] != parameterMappedObject.Label)
                    Debug.LogError(
                        $"Object {obj.name} already has a mapping for parameter {parameterMappedObject.ParameterName}");
                else
                    Debug.LogWarning(
                        $"Object {obj.name} already has a mapping for parameter {parameterMappedObject.ParameterName}");
            }

            return true;
        }

        public bool Unlink(object obj) => _mappings.Remove(obj);


        public bool IsLinked(object obj)
        {
            return _mappings.ContainsKey(obj);
        }

        public bool IsLinked(object obj, string parameterName)
        {
            if (!_mappings.ContainsKey(obj))
            {
                return false;
            }

            var mapping = _mappings[obj];
            return mapping.ContainsKey(parameterName);
        }

        public bool TryGetLabel(Object obj, string parameterName, out string label)
        {
            label = null;
            if (obj == null) return false;
            if (!_mappings.ContainsKey(obj))
            {
                label = null;
                return false;
            }

            var mapping = _mappings[obj];
            if (!mapping.ContainsKey(parameterName))
            {
                label = null;
                return false;
            }

            label = mapping[parameterName];
            return true;
        }

        public void Reset()
        {
            _mappings.Clear();
        }

    }
   
        
    public struct ValueMapParameter<T> : IValueMappedParameter<T>
    {
        public ValueMapParameter(string parameterName, string label, T value)
        {
            ParameterName = parameterName;
            Label = label;
            Value = value;
        }

        public string ParameterName { get; }
        public string Label { get; }
        public T Value { get; }
    }
    
    public struct ParameterMappedObject : IParameterMappedObject
    {
        public ParameterMappedObject(string parameterName, string label, Object value)
        {
            ParameterName = parameterName;
            Label = label;
            LinkedObject = value;
        }

        public string ParameterName { get; }
        public Object LinkedObject { get; }
        public string Label { get; }
    }

    /// <summary>
    /// helper class to wrap all bindings related to a specific parameter 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ParameterMapBase<T> : IValueMapProvider<T>
    {
        protected abstract string GetParameterName();
        public IEnumerable<IValueMappedParameter<T>> GetParameters()
        {
            foreach (var labelMapping in GetLabelMappings())
                yield return new ValueMapParameter<T>(GetParameterName(), labelMapping.label, labelMapping.value);
        }
        
        protected abstract IEnumerable<(T value, string label)> GetLabelMappings();
     


    }
    

    /// <summary>
    /// currently used to map string tag and layer to surface type.  
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IValueMapProvider<T>
    {
        IEnumerable<IValueMappedParameter<T>> GetParameters();
    }
    
    /// <summary>
    ///generic version of <see cref="IParameterMappedObject"/>
    /// </summary>
    public interface IValueMappedParameter<T>
    {
        string ParameterName { get; }
        string Label { get; }
        T Value { get; }
    }
    
    public interface IParameterMapProvider
    {
        IEnumerable<IParameterMappedObject> GetParameters();
    }

    /// <summary>
    /// establishes a link between a parameter and a label for a given object
    /// </summary>
    public interface IParameterMappedObject
    {
        public string Label { get; }
        public string ParameterName { get; }
        public Object LinkedObject { get; }
    }
}


