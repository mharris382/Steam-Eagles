#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;

namespace CoreLib.Audio
{
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
            var tree = new OdinMenuTree { { "Tags", FMODTagDatabase.Instance } , { "Parameters", FMODParameterDatabase.Instance }};
            
            return tree;
        }
    }
}
#endif