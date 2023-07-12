using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace MyEditor.Global
{
    
    public class ProjectContextWindow : OdinMenuEditorWindow
    {
        const string PROJECT_CONTEXT_PATH = "ProjectContext";

        private ProjectContext _projectContext;
        private ProjectContext ProjectContext => _projectContext ??= ProjectContextGameObject.GetComponent<ProjectContext>();
        private GameObject _projectContextGameObject;
        private GameObject ProjectContextGameObject => _projectContextGameObject ??= Resources.Load<GameObject>(PROJECT_CONTEXT_PATH);
        [MenuItem("Tools/ProjectContextWindow")]
        private static void ShowWindow()
        {
            var window = GetWindow<ProjectContextWindow>();
            window.titleContent = new GUIContent("Project Context Window");
            window.Show();
        }
        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();
            if (ProjectContext == null)
            {
                return tree;
            }
            tree.Add("Project Context", ProjectContext);
            tree.Add("Project Context GameObject", new GameObjectWrapper(ProjectContext.gameObject));
            var monoInstallers = ProjectContext.Installers.ToList();
            var soInstallers = ProjectContext.ScriptableObjectInstallers.ToList();
            var prefabInstallers = ProjectContext.InstallerPrefabs.Select(t => t.gameObject).ToList();
            var installerList = new InstallerList(monoInstallers, soInstallers, prefabInstallers);
            
            
            int nullMonoInstallers = monoInstallers.Count(t => t == null);
            int nullSoInstallers = soInstallers.Count(t => t == null);
            int nullPrefabInstallers = prefabInstallers.Count(t => t == null);
            if(nullMonoInstallers + nullSoInstallers + nullPrefabInstallers > 0)
            {
                tree.Add("Warning", new WarningMessage(ProjectContext, nullMonoInstallers, nullSoInstallers, nullPrefabInstallers));
            }
            
            tree.Add("Installers", installerList);
            AddMonoInstallers(monoInstallers, tree);
            AddPrefabInstallers(prefabInstallers, tree);
            AddSoInstallers(soInstallers, tree);
            return tree;
        }

        private void AddMonoInstallers(List<MonoInstaller> monoInstallers, OdinMenuTree tree)
        {
            foreach (var installer in monoInstallers)
            {
                if (installer == null)
                {
                    continue;
                }

                AddInstaller(tree, "MonoInstallers", installer);
            }
        }
        private void AddPrefabInstallers(List<GameObject> prefabInstallers, OdinMenuTree tree)
        {
            if(prefabInstallers.Count < 0) return;
            foreach (var prefab in prefabInstallers)
            {
                if (prefab == null)
                {
                    continue;
                }

                AddInstaller(tree, "PrefabInstallers", prefab.name, prefab);
            }
        }
        private void AddSoInstallers(List<ScriptableObjectInstaller> soInstallers, OdinMenuTree tree)
        {
            if (soInstallers.Count < 0) return;
            foreach (var scriptableObjectInstaller in soInstallers)
            {
                if (scriptableObjectInstaller == null)
                {
                    continue;
                }

                AddInstaller(tree, "ScriptableObjectInstallers", scriptableObjectInstaller);
            }
        }

        void AddInstaller(OdinMenuTree tree, string group, object installer)
        {
            tree.Add($"Installer/{group}/{installer.GetType().Name}", installer);
        }
        void AddInstaller(OdinMenuTree tree, string group, string itemName, object installer)
        {
            tree.Add($"Installer/{group}/{itemName}-{installer.GetType().Name}", installer);
        }
    }

    public class GameObjectWrapper
    {
        [ShowInInspector, ReadOnly]
        private GameObject gameObject;

        [Button(ButtonSizes.Medium), PropertyOrder(-1)]
        void Inspect()
        {
            Selection.activeGameObject = gameObject;
        }
        public GameObjectWrapper(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }
    }
    public class WarningMessage
    {
        [BoxGroup("Null Installers")]
        [InfoBox("@ErrorMessage", InfoMessageType.Error)]
        [ShowInInspector, InlineEditor()]
        private  ProjectContext _context;

        public WarningMessage(ProjectContext context, int nullMonoInstallers, int nullSoInstallers, int nullPrefabInstallers)
        {
            _context = context;
            NullMonoInstallers = nullMonoInstallers;
            NullSoInstallers = nullSoInstallers;
            NullPrefabInstallers = nullPrefabInstallers;
        }

        public string ErrorMessage
        {
            get
            {
                return
                    $"Found {NullMonoInstallers} Null Mono Installers, {NullSoInstallers} Null Scriptable Object Installers, {NullPrefabInstallers} Null Prefab Installers";
            }
        }
        
        public int NullMonoInstallers { get; }

        public int NullSoInstallers { get; }

        public int NullPrefabInstallers { get; }
    }

    public class InstallerList
    {
        public InstallerList(List<MonoInstaller> monoInstallers, List<ScriptableObjectInstaller> soInstallers, List<GameObject> prefabInstallers)
        {
            MonoInstallers = monoInstallers;
            SoInstallers = soInstallers;
            PrefabInstallers = prefabInstallers;
        }

        [ShowInInspector, ReadOnly, ListDrawerSettings(ShowFoldout = false)]
        public List<MonoInstaller> MonoInstallers { get; set; }
        
        [ShowInInspector, ReadOnly, ListDrawerSettings(ShowFoldout = false)]
        public List<ScriptableObjectInstaller> SoInstallers { get; set; }

        [ShowInInspector, ReadOnly, ListDrawerSettings(ShowFoldout = false)]
        public List<GameObject> PrefabInstallers { get; set; }


        private string Info
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine($"MonoInstallers: {MonoInstallers.Count}");
                sb.AppendLine($"Scriptable Object Installers: {SoInstallers.Count}");
                sb.AppendLine($"Prefab Installers: {PrefabInstallers.Count}");
                return sb.ToString();
            }
        }
    }
}