using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Buildables
{
    public class SteamTurbine : Machine<SteamTurbine>, IMachineCustomSaveData
    {
        public SteamTurbineV2 steamTurbineV2;
        private string _json;
// _controller?.Dispose();
        [ShowInInspector, ReadOnly, HideInEditorMode ,BoxGroup("Debugging"), ProgressBar(0, nameof(StorageCapacitySteam))]
        public float AmountStoredSteam
        {
            get;
            set;
        }

        private float StorageCapacitySteam => steamTurbineV2.CurrentSteamCapacity;


        public void Awake()
        {
            steamTurbineV2.Initialize();
        }

        public void Feedback(float filled, bool producerIsActive)
        {
            //events.amountFilled.Invoke(filled);
            //events.producerActive.Invoke(producerIsActive);
        }

        private void OnDestroy()
        {
            steamTurbineV2.Dispose();
        }

        public void LoadDataFromJson(string json)
        {
            try
            {
                SteamTurbineSaveData saveData = JsonUtility.FromJson<SteamTurbineSaveData>(json);
                steamTurbineV2.CurrentElectricityStored = saveData.amountStored;
            }
            catch (Exception e)
            {
                Debug.LogError("failed to load steam turbine from json");
            }
        }

        public string SaveDataToJson()
        {
            var saveData = new SteamTurbineSaveData();
            saveData.amountStored = AmountStoredSteam;
            return JsonUtility.ToJson(saveData);
        }
        public void LoadFromJson(string json)
        {
         
        }
  
        [System.Serializable]
        class SteamTurbineSaveData
        {
            public float amountStored;
        }
    }
}