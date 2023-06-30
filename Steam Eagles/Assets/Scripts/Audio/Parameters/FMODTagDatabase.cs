using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ObjectLabelMapping;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace CoreLib.Audio
{
    [GlobalConfig("Resources/Audio/FMODTagDatabase")]
    public class FMODTagDatabase : GlobalConfig<FMODTagDatabase>, IValueMapProvider<string>
    {
        [TableList,ValidateInput(nameof(IsValid)),InlineButton(nameof(Sort))] [CanBeNull]
        public List<LinkedTag> tags;


  
        [System.Serializable]
        public class LinkedTag : IValueMappedParameter<string>, IComparable<LinkedTag>
        {
            [ValueDropdown(nameof(GetParameterDropdown)),ValidateInput(nameof(IsParameterValid))]  public string parameter;
            [ValueDropdown(nameof(GetTagDropdown)), ValidateInput(nameof(IsTagValid))] public string tag;
            [ValueDropdown(nameof(GetLabelDropdown)),ValidateInput(nameof(IsLabelValid))] public string label;
            
            public string ParameterName => parameter;
            public string Label => label;
            public string Value => tag;
           
            #region [Value Dropdown Helpers]

            ValueDropdownList<string> GetLabelDropdown() => FMODParameterDatabase.GetLabelsFor(parameter);

            ValueDropdownList<string> GetParameterDropdown()
            {
                return FMODParameterDatabase.GetParameters();
            }
            ValueDropdownList<string> GetTagDropdown()
            {
                var vdl =  new ValueDropdownList<string>();
                
#if UNITY_EDITOR
                var serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

                // Find the property representing the tags array
                var tagsProperty = serializedObject.FindProperty("tags");
                for (int i = 0; i < tagsProperty.arraySize; i++)
                {
                    var tagProperty = tagsProperty.GetArrayElementAtIndex(i);
                    var tagName = tagProperty.stringValue;
                    vdl.Add(tagName);
                }
                
                
                
                var usedTagsForParameter =  from linkedTag in Instance.tags where linkedTag != this && linkedTag.IsValid() && linkedTag.parameter == parameter select new ValueDropdownItem<string>(linkedTag.tag, linkedTag.tag);
                foreach (var usedTag in usedTagsForParameter)
                {
                    vdl.Remove(usedTag);
                }
#endif
                return vdl;
            }

            #endregion
            #region [Validation Helpers]

            bool IsTagValid() => !string.IsNullOrEmpty(tag);
            bool IsParameterValid() => !string.IsNullOrEmpty(parameter);
            bool IsLabelValid() => !string.IsNullOrEmpty(label);

            public bool IsValid()
            {
                return IsTagValid() && IsParameterValid() && IsLabelValid();
            }

            #endregion

            public int CompareTo(LinkedTag other)
            {
                if (ReferenceEquals(this, other)) return 0;
                if (ReferenceEquals(null, other)) return 1;
                var parameterComparison = string.Compare(parameter, other.parameter, StringComparison.Ordinal);
                if (parameterComparison != 0) return parameterComparison;
                var labelComparison = string.Compare(label, other.label, StringComparison.Ordinal);
                if (labelComparison != 0) return labelComparison;
                var tagComparison = string.Compare(tag, other.tag, StringComparison.Ordinal);
                return tagComparison;
            }
        }

        

        public IEnumerable<IValueMappedParameter<string>> GetParameters() => from linkedTag in tags where linkedTag.IsValid() select linkedTag;

        #region [Helpers]


        private bool IsValid(List<LinkedTag> dbTags, ref string error) => CheckForOverlaps(ref error);

        private bool CheckForOverlaps(ref string error)
        {
            Dictionary<string, Dictionary<string, string>> parameterToTag = new Dictionary<string, Dictionary<string, string>>();
            foreach (var tag in tags)
            {
                if(tag.IsValid() == false)
                    continue;
                if (parameterToTag.TryGetValue(tag.parameter, out var tagToLabel) == false)
                {
                    tagToLabel = new Dictionary<string, string>();
                    parameterToTag.Add(tag.parameter, tagToLabel);
                    
                }

                if (tagToLabel.ContainsKey(tag.tag))
                {
                    error = $"Duplicate tag {tag.tag} for parameter {tag.parameter}";
                    return false;
                }
                else
                {
                    tagToLabel.Add(tag.tag, tag.label);
                }
            }
            return true;
        }

        private void Sort()
        {
            tags?.Sort();
        }
        #endregion
        
    }
}