using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class SceneOrganizationHelper : EditorWindow
    {
        private GameObject[] vents;
        private GameObject[] fgDecor;
        private GameObject[] fgWindows;
        private GameObject[] fgWalls;
        private GameObject[] wallDecor;
        private GameObject[] windows;
        private GameObject[] walls;


        [MenuItem("Tools/Scene Cleaner")]
        private static void ShowWindow()
        {
            var window = GetWindow<SceneOrganizationHelper>();
            window.titleContent = new GUIContent("Scene Cleaner");
            window.Show();
            
            
            
            
            //go into bg walls
            window.walls = GameObject.FindGameObjectsWithTag("Wall");
            
            //go into bg walls
            window. windows = GameObject.FindGameObjectsWithTag("Window");
            
            
            //go into bg decor
            window. wallDecor = GameObject.FindGameObjectsWithTag("Wall Decor");
            
            
            //go into fg wall
            window. fgWalls = GameObject.FindGameObjectsWithTag("FG Wall");
            
            //go into fg decor
            window. fgDecor = GameObject.FindGameObjectsWithTag("FG Decor");
            
            //go into fg windows
            window. fgWindows = GameObject.FindGameObjectsWithTag("FG Window");
            
            
            
                
            //go into either FG decor or BD decor
            window.vents = GameObject.FindGameObjectsWithTag("Vent");

            
        }
        
        private void OnGUI()
        {
            var rect = EditorGUILayout.GetControlRect();
            
        }
    }
}