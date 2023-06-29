using System.Collections.Generic;
using UnityEngine;

namespace ObjectLabelMapping
{
    public class GlobalValueMap
    {
        LabelMappedDictionary<int> _layerMap = new();
        LabelMappedDictionary<string> _tagMap = new();
        
        public GlobalValueMap(
            List< IValueMapProvider<int>> layerParameters,
            List<IValueMapProvider<string>> tagParameters)
        {
            foreach (var valueMappedParameter in layerParameters)
            {
                foreach (var mappedParameter in valueMappedParameter.GetParameters())
                {
                    var res =_layerMap.Link(mappedParameter.Value, mappedParameter.ParameterName, mappedParameter.Label);
                    Debug.Assert(res, "Failed to link layer int parameter " + mappedParameter.ParameterName + " to " + mappedParameter.Label);
                }
            }

            foreach (var valueMappedParameter in tagParameters)
            {
                foreach (var mappedParameter in valueMappedParameter.GetParameters())
                {
                    var res =_tagMap.Link(mappedParameter.Value, mappedParameter.ParameterName, mappedParameter.Label);
                    Debug.Assert(res, "Failed to link layer int parameter " + mappedParameter.ParameterName + " to " + mappedParameter.Label);
                }
            }

            ObjectLabelMapExtensions.valueMap = this;
        }
        
        
        public bool TryGetLabel(GameObject value, string parameter, out string label)
        {
            if(TryGetLabelFromTag(value.tag, parameter, out label))
                return true;
            if (TryGetLabelFromLayer(value.layer, parameter, out label))
                return true;
            return false;
        }
        public bool TryGetLabelFromLayer(int value, string parameter, out string label)
        {
            if (_layerMap.TryGetLabel(value, parameter, out label)) 
                return true;
            return false;
        }
        public bool TryGetLabelFromTag(string value, string parameter, out string label)
        {
            if (_tagMap.TryGetLabel(value, parameter, out label)) 
                return true;
            return false;
        }
        
        public class LabelMappedDictionary<T>
        {
            private Dictionary<T, Dictionary<string, string>> _mappings = new();

            public bool Link(T value, string parameter, string label)
            {
                if (value == null)
                    return false;
                if (!_mappings.ContainsKey(value))
                {
                    _mappings.Add(value, new Dictionary<string, string>());
                }
                var mapping = _mappings[value];

                if (mapping.ContainsKey(parameter))
                {
                    return mapping[parameter] == label;
                }
                mapping.Add(parameter, label);
                return true;
            }
        
            public bool TryGetLabel(T obj, string parameterName, out string label)
            {
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
        }



    }
}