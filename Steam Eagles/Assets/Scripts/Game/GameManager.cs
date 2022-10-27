using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class GameManager : MonoBehaviour
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