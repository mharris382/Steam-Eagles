using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UI.PlayerGUIs;
using UI.PlayerGUIs.CharacterWindows;
using UnityEditor;

namespace UI.MyEditor
{
    public class WindowNavigatorWindow : OdinMenuEditorWindow
    {
        [MenuItem("Tools/WindowNavigatorWindow")]
        private static void ShowWindow()
        {
            var window = GetWindow<WindowNavigatorWindow>();
            window.titleContent = new UnityEngine.GUIContent("GUI Window Navigator");
            window.Show();
        }


        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();

            var playerGUIController = FindObjectsOfType<PlayerCharacterGUIController>();
            foreach (PlayerCharacterGUIController controller in playerGUIController)
            {
                tree.Add($"{controller.name}", new PlayerGUIEditorUtility(controller));
            }

            return tree;
        }
    }

    public class PlayerGUIEditorUtility
    {
        [ShowInInspector, InlineButton(nameof(ShowHUD))]
        private readonly PlayerCharacterHUD _hud;
        private readonly PlayerCharacterGUIWindowRoot _windowRoot;
        
        [ShowInInspector, HideLabel]
        public PanelSwitcherUtility panelSwitcherUtility;

        private bool HasHUD => _hud != null;
        private bool HasWindowRoot => _windowRoot != null;

        void ShowHUD()
        {
            _hud.Open();
            panelSwitcherUtility.CloseAll();
            Selection.activeGameObject = _hud.gameObject;
        }
        
        public PlayerGUIEditorUtility(PlayerCharacterGUIController controller)
        {
            _windowRoot = controller.GetComponentInChildren<PlayerCharacterGUIWindowRoot>();
            _hud = controller.GetComponentInChildren<PlayerCharacterHUD>();
            var windows =_windowRoot.GetComponentsInChildren<UICharacterWindowBase>().Select(t => t as Window).ToList();
            panelSwitcherUtility = new PanelSwitcherUtility(_windowRoot, windows.ToArray());
        }
    }

    [InlineProperty]
    public class PanelSwitcherUtility
    {
        private readonly Window _root;

        [TableList]
        public List<WindowWrapper> wrappers;

        public void CloseAll()
        {
            _root.Close();
            foreach (var windowWrapper in wrappers)
            {
                windowWrapper._window.Close();
            }
        }
        public PanelSwitcherUtility(Window root, Window[] panels)
        {
            _root = root;
            wrappers = new List<WindowWrapper>();
            foreach (var window in panels)
            {
                wrappers.Add(new WindowWrapper(this, window));
            }
        }
        
        [ShowInInspector]
        public class WindowWrapper
        {
            private readonly PanelSwitcherUtility _panelSwitcherUtility;

            [ShowInInspector]
            internal readonly Window _window;

            [Button]
            public void Open()
            {
                _window.Open();
                _panelSwitcherUtility._root.Open();
                foreach (var windowWrapper in _panelSwitcherUtility.wrappers)
                {
                    if (windowWrapper._window != _window)
                    {
                        windowWrapper._window.Close();
                    }
                }
                Selection.activeGameObject = _window.gameObject;
            }
            
            public WindowWrapper(PanelSwitcherUtility panelSwitcherUtility, Window window)
            {
                _panelSwitcherUtility = panelSwitcherUtility;
                _window = window;
            }
        }
    }
}