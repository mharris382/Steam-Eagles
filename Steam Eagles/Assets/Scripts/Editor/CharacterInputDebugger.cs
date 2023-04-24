using System.Collections.Generic;
using Characters;
using CoreLib;
using CoreLib.Entities;
using DefaultNamespace;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using SteamEagles.Characters;
using UnityEditor;
using UnityEngine;

using UnityEngine.TextCore.Text;

namespace Editor
{
    public class CharacterInputDebugger : OdinEditorWindow
    {
        [MenuItem("Tools/Debugging/Character Input Debugger")]
        private static void ShowWindow()
        {
            var window = GetWindow<CharacterInputDebugger>();
            window.titleContent = new GUIContent("Character Input Debugger");
            window.Show();
        }

        protected override IEnumerable<object> GetTargets()
        {
            var characterStates = FindObjectsOfType<CharacterState>();
            foreach (var characterState in characterStates)
            {
                yield return new CharacterWrapper(characterState);
                yield return new CharacterConfigWrapper(characterState.config);
            }
        }

        public class CharacterConfigWrapper
        {
            [Title("Character Movement Settings", "Character Config")]
            [InlineEditor(inlineEditorMode: InlineEditorModes.GUIAndHeader, Expanded = true)]
            public CharacterConfig config;
            
            public CharacterConfigWrapper(CharacterConfig config)
            {
                this.config = config;
            }
        }

        public class CharacterWrapper
        {
            private readonly CharacterState _character;

            [HorizontalGroup("main", -100), ShowInInspector, HideLabel]
            public string Name => _character.tag;

            [HorizontalGroup("main", -100), ShowInInspector, HideLabel]
            public GameObject CharacterObject => _character.gameObject;

            [HorizontalGroup("Entity/h1")]
            [FoldoutGroup("Entity", expanded: true, order: 10), ShowInInspector, HideLabel]
            public EntityInitializer EntityInitializer => _character.GetComponent<EntityInitializer>();

            [HorizontalGroup("Entity/h1")]
            [ShowInInspector, HideLabel]
            public Entity Entity => EntityInitializer.Entity;

            [FoldoutGroup("Entity"), ShowInInspector]
            public bool IsInitialized => EntityInitializer.isDoneInitializing;

            [ShowInInspector,FoldoutGroup("Inputs")]
            [HorizontalGroup("Inputs/move")]
            public float MoveX => _character.MoveX;

            [ShowInInspector,HorizontalGroup("Inputs/move")]
            public float MoveY => _character.MoveY;

            
            
            [BoxGroup("Inputs/Tool Inputs"),ShowInInspector]
            public Vector2 ToolAim => _character.Tool.Inputs.AimInputRaw;

            [BoxGroup("Inputs/Tool Inputs"),ShowInInspector]
            public Vector2 ToolAimPositionLS => _character.Tool.AimPositionLocal;

            [BoxGroup("Inputs/Tool Inputs"),ShowInInspector]
            public Vector2 ToolAimPositionWS => _character.Tool.AimPositionWorld;


            [BoxGroup("Inputs/Tool Inputs"), ShowInInspector, EnumPaging]
            public ToolStates CurrentTool
            {
                get =>_character.Tool.currentToolState;
                set => _character.Tool.currentToolState = value;
            }
            
            
            public CharacterWrapper(CharacterState character)
            {
                _character = character;
            }
        }
    }
}