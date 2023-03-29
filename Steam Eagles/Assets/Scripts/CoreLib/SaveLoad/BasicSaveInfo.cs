using UnityEngine;

namespace CoreLib.SaveLoad
{
    [System.Serializable]
    public class BasicSaveInfo
    {
        public bool selectedMultiplayerMode;
        public string[] playerCharacters;
        public string airshipScene;
    }
    
    
    public static class SaveUtility
    {
        public static Transform FindAirshipHullRigidBodyInCurrentScene()
        {
            var go = GameObject.FindGameObjectWithTag("Airship");
            for (int i = 0; i < go.transform.childCount; i++)
            {
                var child = go.transform.GetChild(i);
                if (child.CompareTag("Building"))
                {
                    
                    return child;
                }
            }
            return null;
        }
        
        
        
    }
}