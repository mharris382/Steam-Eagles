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


        public float ElectricityStored => steamTurbineV2.CurrentElectricityStored;
        public float ElectricityStoredNormalized => steamTurbineV2.CurrentElectricityStoredNormalized;
        public float SteamStored => steamTurbineV2.CurrentSteamStored;
        public float SteamStoredNormalized => steamTurbineV2.CurrentSteamStoredNormalized;

        public float SteamConsumptionRate => steamTurbineV2.GetConsumptionRate();
        public float SteamProductionRate => steamTurbineV2.GetProductionRate();
        public float ElectricalProductionRate => steamTurbineV2.GetElectricalProductionRate();

        public bool CanProduceElectrical => ElectricalProductionRate > 0;
        public bool CanConsumeSteam => SteamConsumptionRate > 0;
        public bool CanProduceSteam => SteamProductionRate > 0;
        
        public bool HasSteam => SteamStored > 0;
        public bool HasElectricity => ElectricityStored > 0;

        public string SteamDebugInfo
        {
            get
            {
                return $"<u>Steam</u>\n{SteamStoredNormalized * 100}% <size=12>full</size>\n<color=blue>{SteamConsumptionRate} <size=12>u/s</size> <i>in</i></color>\n<color=red>{SteamProductionRate} <size=12>u/s</size><i> out</i></color>";

            }
        }
        public string ElectricityDebugInfo
        {
            get
            {
                return $"<u>Steam</u>\n{ElectricityStoredNormalized * 100}% <size=12>full</size>\n<color=red>{ElectricalProductionRate} <size=12>u/s</size><i> out</i></color>";

            }
        }


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