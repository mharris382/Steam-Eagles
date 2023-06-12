using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.PauseMenus
{
    public class UISaveGameMenuInstaller : MonoInstaller
    {
        [SerializeField,ChildGameObjectsOnly] private Button saveButton;
        [SerializeField,ChildGameObjectsOnly] private TMP_InputField saveNameInputField;
        [SerializeField, ChildGameObjectsOnly] private Transform savePathButtonParent;
        [SerializeField,AssetsOnly] private Button savePathButtonPrefab;

        public override void InstallBindings()
        {
            //Container.Bind<DiContainer>().FromInstance(Container).AsSingle().NonLazy();
            Container.Bind<Button>().FromInstance(saveButton).AsCached().WhenInjectedInto<NewSaveButtonHandler>();
            Container.Bind<TMP_InputField>().FromInstance(saveNameInputField).AsSingle().NonLazy();
            Container.Bind<Transform>().FromInstance(savePathButtonParent).AsSingle();
            Container.Bind<Button>().FromInstance(savePathButtonPrefab).AsCached().WhenInjectedInto<SavePathButtonFactory>();
            
            Container.BindInterfacesAndSelfTo<NewSaveButtonHandler>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<SavePathButtonsList>().AsSingle().NonLazy();
            
            Container.BindFactory<string, Button, ButtonFactory>().FromFactory<SavePathButtonFactory>();
        }
    }

    public class ButtonFactory : PlaceholderFactory<string, Button> { }
    public class SavePathButtonsList : IInitializable
    {
        private readonly GlobalSavePath _savePath;
        private readonly ButtonFactory _savePathButtonFactory;

        public SavePathButtonsList(GlobalSavePath savePath, ButtonFactory savePathButtonFactory)
        {
            _savePath = savePath;
            _savePathButtonFactory = savePathButtonFactory;
        }
        public void Initialize()
        {
           RebuildList();
        }

        public void RebuildList()
        {
            foreach (var allValidLoadPath in _savePath.GetAllValidLoadPaths())
            {
                _savePathButtonFactory.Create(allValidLoadPath);
            }
        }
    }

    public class SavePathButtonFactory : IFactory<string, Button>
    {
        private readonly Button _savePathButtonPrefab;
        private readonly Transform _parent;
        private readonly TMP_InputField _inputField;
        private readonly DiContainer _container;
        private Dictionary<string, Button> _createdButtons = new Dictionary<string, Button>();

        public SavePathButtonFactory(Button savePathButtonPrefab,
            Transform parent, TMP_InputField inputField,
            DiContainer container)
        {
            _savePathButtonPrefab = savePathButtonPrefab;
            _parent = parent;
            _inputField = inputField;
            _container = container;
        }
        public Button Create(string path)
        {
            if (_createdButtons.TryGetValue(path, out var button))
            {
                if (button != null)
                {
                    button.transform.SetAsFirstSibling();
                    return button;
                }
                _createdButtons.Remove(path);
            }
            var btn = _container.InstantiatePrefabForComponent<Button>(_savePathButtonPrefab, _parent);
            var text = btn.GetComponentInChildren<TextMeshProUGUI>();
            btn.onClick.AsObservable().Subscribe(_ => _inputField.text = path).AddTo(btn);
            string label = path.Replace(Application.persistentDataPath, "");
            text.text = label;
            _createdButtons.Add(path, btn);
            return btn;
        }
    }
}