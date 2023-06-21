using Cinemachine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class CinemachineCameraValidator : OdinEditorWindow
    {
        public float nearClipPlane;
        public float farClipPlane;
        static CinemachineVirtualCamera[] _cameras;


        [MenuItem("Tools/Scene/Cinemachine Camera Validator")]
        private static void ShowWindow()
        {
            var window = GetWindow<CinemachineCameraValidator>();
            window.titleContent = new GUIContent("Cinemachine Camera Validator");
             
            window.Show();
            _cameras = FindObjectsOfType<CinemachineVirtualCamera>(true);
            var cam = Camera.main;
            if (cam == null)
            {
                return;
            }
            window.nearClipPlane = cam.nearClipPlane;
            window.farClipPlane = cam.farClipPlane;
        }

        [Button("Standardize Clip Planes")]
        void StandardizeClipPlanes()
        {
            _cameras = FindObjectsOfType<CinemachineVirtualCamera>();
            foreach (var camera in _cameras)
            {
                camera.m_Lens.NearClipPlane = nearClipPlane;
                camera.m_Lens.FarClipPlane = farClipPlane;
            }
        }
    }
}