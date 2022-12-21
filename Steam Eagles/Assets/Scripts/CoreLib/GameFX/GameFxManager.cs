using UnityEngine;

namespace Utilities
{
    public class GameFxManager : MonoBehaviour
    {
        private static GameFxManager _instance;
        public static GameFxManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameFxManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("GameFxManager");
                        _instance = go.AddComponent<GameFxManager>();
                    }
                }
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }


        private void Awake()
        {
            if(_instance == null)
            {
                _instance.gameObject.name = "GameFxManager (Singleton)";
                _instance = this;
            }
            else
            {
                gameObject.name = "GameFxManager (Removed Duplicate)";
                Destroy(this);
            }
        }
    }
}