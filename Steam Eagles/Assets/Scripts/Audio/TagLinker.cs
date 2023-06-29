using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif
namespace CoreLib.Audio
{
    [GlobalConfig("Resources/TagLinker")]
    public class TagLinker : GlobalConfig<TagLinker>
    {
        public LinkedTag[] tags;
        [System.Serializable]
        public class LinkedTag
        {
            public string parameter;
            public string tag;
            public string label;



            bool IsTagValid() => !string.IsNullOrEmpty(tag);
            bool IsParameterValid() => !string.IsNullOrEmpty(parameter);
            bool IsLabelValid() => !string.IsNullOrEmpty(label);
            ValueDropdownList<string> GetLabelDropdown()
            {
                var vdl =  new ValueDropdownList<string>();
#if UNITY_EDITOR
                if(!IsTagValid() || !IsParameterValid()) return vdl;
                
#endif
                return vdl;
            }
            ValueDropdownList<string> GetParameterDropdown()
            {
                var vdl =  new ValueDropdownList<string>();
#if UNITY_EDITOR
                
#endif
                return vdl;
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
#endif
                return vdl;
            }
        }
        
    }
    
    
    
#if UNITY_EDITOR
    public class AudioWizard : OdinMenuEditorWindow
    {
        const string FMOD_PARAMETERS_PATH = "Assets/Scripts/CoreLib/Audio/Parameters";
        [MenuItem("Tools/Audio Wizard")]
        public static void Open()
        {
            var window = GetWindow<AudioWizard>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree { { "Tags", TagLinker.Instance } };
            
            return tree;
        }
    }
#endif
}