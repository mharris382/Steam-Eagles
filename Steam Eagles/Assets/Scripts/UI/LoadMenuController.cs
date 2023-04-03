using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoreLib.SaveLoad;
using SaveLoad;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UI
{
    public class LoadMenuController : MonoBehaviour
    {
        private const string LoadGameButtonAddress = "Load Game Button";

        private GameObject _loadGameButtonPrefab;


        [SerializeField, Required]
        private RectTransform loadGameButtonParent;

        private List<UILoadGameButton> _buttons;

        IEnumerator WaitForPrefabToLoad()
        {
            if (_loadGameButtonPrefab != null)
            {
                yield break;
            }

            var loadOp =Addressables.LoadAssetAsync<GameObject>(LoadGameButtonAddress);
            yield return loadOp;
            
            _loadGameButtonPrefab = loadOp.Result;
        }

        IEnumerator LoadAndPopulate()
        {
            yield return WaitForPrefabToLoad();
            PopulateLoadMenu();
        }
        
        void OnEnable()
        {
            if (_loadGameButtonPrefab != null)
            {
                PopulateLoadMenu();
            }
            else
            {
                StartCoroutine(LoadAndPopulate());
            }
        }

        private void OnDisable()
        {
            ClearLoadMenu();
        }

        void ClearLoadMenu()
        {
            var listOfT = new List<Transform>();
            for (int i = 0; i < loadGameButtonParent.childCount; i++)
            {
                listOfT.Add(loadGameButtonParent.GetChild(i));
            }

            for (int i = listOfT.Count-1; i >0; i--)
            {
                Destroy(listOfT[i].gameObject);
            }
        }
        
        void PopulateLoadMenu()
        {
            var allSaves = PersistenceManager.GetAllGameSaves().ToArray();
            PopulateLoadMenu(allSaves);
        }

        void PopulateLoadMenu(string[] saves)
        {
            if (loadGameButtonParent.childCount > 0)
            {
                ClearLoadMenu();
            }

            _buttons = new List<UILoadGameButton>();
            foreach (var save in saves)
            {
               _buttons.Add(CreateButtonForSave(save));
            }
        }

        UILoadGameButton CreateButtonForSave(string savePath)
        {
            var instGo = Instantiate(_loadGameButtonPrefab, loadGameButtonParent);
            var inst = instGo.GetComponent<UILoadGameButton>();
            inst.SetSavePath(savePath);
            return inst;
        }
    }
}