#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Core.SharedVariables
{
    public class SharedBoolListener : MonoBehaviour
    {
        
        public SharedBool variable;
        
        [Header("Events")] public UnityEvent onTrue;
        public UnityEvent onFalse;
        public bool fireOnAwake;
        private void Awake()
        {
            variable.onValueChanged.AddListener(FireEvent);
            if (fireOnAwake)
            {
                FireEvent(variable.Value);
            }
        }

        private void FireEvent(bool t)
        {
            if (t)
                onTrue.Invoke();
            else
                onFalse.Invoke();
        }
    }
//
// #if UNITY_EDITOR
//     
//     [CustomEditor(typeof(SharedBoolListener))]
//     public class SharedBoolListenerEditor : Editor
//     {
//         private SerializedProperty _variable;
//         private string _newVariableName;
//         private string newVariableName => string.IsNullOrEmpty(_newVariableName) ? target.name : _newVariableName;
//         private void OnEnable()
//         {
//             this._variable = serializedObject.FindProperty("variable");
//         }
//
//         public override void OnInspectorGUI()
//         {
//             if (_variable.objectReferenceValue == null)
//             {
//                 EditorGUILayout.HelpBox("Variable is not set", MessageType.Warning);
//                 EditorGUILayout.BeginHorizontal();
//                 {
//                     serializedObject.Update();
//                     EditorGUILayout.PrefixLabel(new GUIContent(_variable.name, _variable.tooltip));
//                     EditorGUILayout.PropertyField(_variable, GUIContent.none, GUILayout.MinWidth(150), GUILayout.MaxWidth(300), GUILayout.ExpandWidth(true));
//                     
//                     if (GUILayout.Button("+", GUILayout.Width(20)))
//                     {
//                         var boolListener = target as SharedBoolListener;
//                         
//                         _variable.objectReferenceValue = CreateSharedBoolAsset(newVariableName);
//                         
//                         _newVariableName = "";
//                     }
//                     serializedObject.ApplyModifiedProperties();
//                 }
//                 EditorGUILayout.EndHorizontal();
//                 EditorGUILayout.BeginHorizontal();
//                 GUILayout.FlexibleSpace();
//                 _newVariableName = EditorGUILayout.TextField(_newVariableName, GUILayout.MinWidth(170), GUILayout.MaxWidth(320), GUILayout.ExpandWidth(false));
//                 EditorGUILayout.EndHorizontal();
//             }
//             else
//             {
//                 base.OnInspectorGUI();
//             }
//         }
//         const string ASSET_SAVE_PATH = "Assets/Data/Shared Variables/Shared Values/{0}.asset";
//         public static SharedBool CreateSharedBoolAsset(string name)
//         {
//             var asset = ScriptableObject.CreateInstance<SharedBool>();
//             asset.name = name;
//             AssetDatabase.CreateAsset(asset, string.Format(ASSET_SAVE_PATH, name));
//             AssetDatabase.SaveAssets();
//             return asset;
//         }
//         
//     }
// #endif
}