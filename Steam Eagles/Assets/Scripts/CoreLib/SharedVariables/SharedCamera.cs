using UnityEngine;

namespace CoreLib
{
    [CreateAssetMenu(menuName = "Shared Variables/Create Shared Camera", fileName = "Shared Camera", order = 0)]
    public class SharedCamera : SharedVariable<Camera>{}
    
    
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SharedCamera))]
    public class SharedCamerasEditor : SharedVariableEditor<Camera, SharedCamera>
    {
    }
#endif
}