using System.Collections;
using UnityEngine;
using Zenject;

namespace UI.PauseMenus
{
    public class UISaveGameRunner : MonoBehaviour
    {
        private NewSaveButtonHandler _saveButtonHandler;
        private SavePathButtonsList pathButtonsList;

        [Inject]
        public void InjectMe(SavePathButtonsList pathButtonsList, NewSaveButtonHandler saveButtonHandler)
        {
            this.pathButtonsList = pathButtonsList;
            this._saveButtonHandler = saveButtonHandler;
        }


        private void Start()
        {
            StartCoroutine(WaitToInit());
        }

        IEnumerator WaitToInit()
        {
            while (_saveButtonHandler == null)
            {
                Debug.Log("Waiting for save button handler to be installed");
                yield return null;
            }
            _saveButtonHandler.Initialize();
            pathButtonsList.Initialize();
        }
    }
}