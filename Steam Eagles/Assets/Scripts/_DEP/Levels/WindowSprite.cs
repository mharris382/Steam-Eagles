using UnityEngine;
#if UNITY_EDITOR
    
using UnityEditor;  
    
#endif
namespace Levels
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class WindowSprite : MonoBehaviour
    {
        private SpriteRenderer _sr;
        public SpriteRenderer Sr => (_sr != null) ? _sr : (_sr = GetComponent<SpriteRenderer>());

        [SerializeField]
        private SpriteMask windowMask;

        

        public bool HasWindowMask()
        {
            return windowMask != null;
        }

        public void CreateWindowMask()
        {
            if (windowMask == null)
            {
                windowMask = GetComponentInChildren<SpriteMask>();
            }
            if (windowMask == null)
            {
                var go = new GameObject($"{name} (MASK)", typeof(SpriteMask));
                go.transform.SetParent(transform, false);
                windowMask = go.GetComponent<SpriteMask>();
            }
            windowMask.alphaCutoff = 1;
            windowMask.transform.SetParent(transform, false);
            windowMask.backSortingLayerID = Sr.sortingLayerID;
            windowMask.frontSortingLayerID = Sr.sortingLayerID;
        }
   }
    
    #if UNITY_EDITOR
    
    [CustomEditor(typeof(WindowSprite))]
    public class WindowSpriteEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            
            var window = target as WindowSprite;
            if (!window.HasWindowMask())
            {
                if (GUILayout.Button("Create Window Sprite Mask"))
                {
                    window.CreateWindowMask();
                }
            }
            base.OnInspectorGUI();
        }
    }
    
    
    #endif
}