using System.Linq;
using UnityEngine;
using System;
using Zenject;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif
namespace Buildables.Installers
{
    [RequireComponent(typeof(GameObjectContext), typeof(BuildableMachineBase))]
    public class BuildableMachineInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<BuildableMachineBase>().FromComponentOnRoot().AsSingle();
        }
    }
    
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(BuildableMachineInstaller))]
    public class BuildableMachineInstallerEditor : OdinEditor
    {
        private SerializedProperty _installerProp;

        protected override void OnEnable()
        {
            _installerProp =serializedObject.FindProperty("_monoInstallers");
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var installer = target as BuildableMachineInstaller;
            var goContext = installer.GetComponent<GameObjectContext>();
            if (goContext.Installers.ToList().Contains(installer) == false)
            {
                EditorGUILayout.HelpBox("Should be bound at the Project Context level", MessageType.Error);
                if (GUILayout.Button("Fix"))
                {
                    Type goContextType = typeof(GameObjectContext);
                    _installerProp.InsertArrayElementAtIndex(0);
                    _installerProp.GetArrayElementAtIndex(0).objectReferenceValue = installer;
                }
            }
            base.OnInspectorGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
    #endif
}