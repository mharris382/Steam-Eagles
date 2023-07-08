using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Buildables
{
    public class SteamTurbine : MonoBehaviour, IMachineCustomSaveData
    {
        [Required,ChildGameObjectsOnly] public MachineCell inputCell;
        [Required,ChildGameObjectsOnly] public MachineCell outputCell;
        private SteamTurbineController _controller;

        [SerializeField] private Events events;
        private HypergasEngineConfig _config;
        private string _json;

        [Serializable]
        private class Events
        {
            public UnityEvent<bool> producerActive;
            public UnityEvent<float> amountFilled;
        }
           
        [ShowInInspector, ReadOnly, HideInEditorMode ,BoxGroup("Debugging")]
        public float ConsumeRate
        {
            get;
            set;
        }
        [ShowInInspector, ReadOnly, HideInEditorMode ,BoxGroup("Debugging")]
        public float ProduceRate
        {
            get;
            set;
        }
        [ShowInInspector, ReadOnly, HideInEditorMode ,BoxGroup("Debugging")]
        public bool IsConsuming
        {
            get;
            set;
        }
        [ShowInInspector, ReadOnly, HideInEditorMode ,BoxGroup("Debugging")]
        public bool IsProducing
        {
            get;
            set;
        }

        [ShowInInspector, ReadOnly, HideInEditorMode ,BoxGroup("Debugging"), ProgressBar(0, nameof(StorageCapacity))]
        public float AmountStored
        {
            get;
            set;
        }

        private float StorageCapacity => _config?.generatorStorageCapacity ?? 1000;
        
        
        [Inject]
        void InjectMe(SteamTurbineController.Factory controllerFactory, HypergasEngineConfig config)
        {
            _controller = controllerFactory.Create(this);
            _config = config;
            if (!string.IsNullOrEmpty(_json))
            {
                _controller.LoadFromJson(_json);
                _json = null;
            }
        }

        public void Feedback(float filled, bool producerIsActive)
        {
            events.amountFilled.Invoke(filled);
            events.producerActive.Invoke(producerIsActive);
        }

        private void OnDestroy()
        {
            _controller.Dispose();
        }

        public void LoadDataFromJson(string json)
        {
            if (_controller == null)
            {
                _json = json;
            }
            else
            {
                _controller.LoadFromJson(json);
            }
        }

        public string SaveDataToJson()
        {
            return _controller.SaveToJson();
        }

       
    }
}