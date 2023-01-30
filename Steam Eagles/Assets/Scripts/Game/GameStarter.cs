using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class GameStarter
        : MonoBehaviour
    {
        public void OnQuitButton()
        {
            Application.Quit();
        }

        public void OnReloadSceneButton()
        {
            SceneManager.LoadScene(0);
        }
    }
}