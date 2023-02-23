using UnityEngine.SceneManagement;

namespace UI.Core
{
    public class MainMenuState : UIState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.buildIndex != 0)
            {
                SceneManager.LoadScene(0);
            }
        }
    }
}