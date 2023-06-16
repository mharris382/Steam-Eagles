using Zenject;

namespace UI
{
    public class WindowInstaller : MonoInstaller
    {
        
        public override void InstallBindings()
        {
            Container.Bind<WindowLoader>().FromInstance(GetComponent<WindowLoader>()).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<UIPromptBuilder>().FromNew().AsSingle().NonLazy();
            Container.Bind<WindowUtil>().AsSingle().NonLazy();
            
        }
    }

    public class WindowUtil
    {
        private static WindowLoader _windowLoader;

        public WindowUtil(WindowLoader windowLoader)
        {
            _windowLoader = windowLoader;
        }
        
        public  static T GetWindow<T>() where T : Window
        {
            return _windowLoader.GetWindow<T>();
        }

        public static bool IsFinishedLoading()
        {
            return _windowLoader.IsFinishedLoading();
        }
    }
}