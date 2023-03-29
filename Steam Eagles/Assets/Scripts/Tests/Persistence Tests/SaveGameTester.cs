using System;
using System.IO;
using CoreLib;
using UniRx;
using UnityEngine;

namespace Tests.Persistence_Tests
{
    public class SaveGameTester : MonoBehaviour
    {
        public string savePath = "Editor Test";

        bool SavePathExists()
        {
            var path = GetFullPath();
            return Directory.Exists(path);
        }

        private string GetFullPath()
        {
            return $"{Application.persistentDataPath}/{savePath}";
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                Debug.Log(SavePathExists() ? $"Overwriting Save at {GetFullPath()}" : $"Creating Save at {GetFullPath()}");
                MessageBroker.Default.Publish(new SaveGameRequestedInfo(GetFullPath()));
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                Debug.Log(SavePathExists() ? $"Loading Save at {GetFullPath()}" : $"No Save Exists at {GetFullPath()}");
                MessageBroker.Default.Publish(new LoadGameRequestedInfo(GetFullPath()));
            }
        }
    }
}