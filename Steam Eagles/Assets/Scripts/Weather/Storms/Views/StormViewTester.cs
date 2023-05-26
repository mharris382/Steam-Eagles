using System;
using CoreLib;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Zenject;

namespace Weather.Storms.Views
{
    public class StormViewTester : MonoBehaviour
    {
        
        
        private StormView.AltFactory _stormViewFactory;

        private TestStorm _testStorm;
        private Storm.Factory _stormFactory;
        private StormView _stormView;
        private Storm _storm;
        private GlobalStormConfig _config;
        private GlobalPCInfo _pcRegistry;

        [Inject]
        public void Install(StormView.AltFactory stormViewFactory,
            GlobalStormConfig config, 
            TestStorm testStorm,
            Storm.Factory stormFactory, GlobalPCInfo pcRegistry)
        {
            this._config = config;
            this._stormViewFactory = stormViewFactory;
            this._stormFactory = stormFactory;
            this._testStorm = testStorm;
            _pcRegistry = pcRegistry;
        }


        private void Update()
        {
            if (_storm == null)
            {
                return;
            }
            var pc1 = _pcRegistry.GetInstance(0);
            if (pc1 == null)
            {
                Debug.LogError("StormViewTester: PC1 is null");
                return;
            }
            var bounds = _storm.InnerBoundsWs;
            bounds.center = pc1.character.transform.position;
            _storm.InnerBoundsWs = bounds;
        }

        [DisableInEditorMode]
        [Button(ButtonSizes.Gigantic)]
        public void SpawnStorm()
        {
            
            var creationParams = _testStorm.GetStormCreationRequest();
            var pc1 = _pcRegistry.GetInstance(0);
            if (pc1 == null)
            {
                Debug.LogError("StormViewTester: PC1 is null");
                return;
            }

            var bounds = creationParams.StormBounds;
            bounds.center = pc1.character.transform.position;
            creationParams.StormBounds = bounds;
            creationParams.StormCreatedSubject.First().Subscribe(t =>
            {
                _storm = t;
                _stormView = _stormViewFactory.Create();
                _stormView.AssignStorm(t);
            }, () => _config.Log("StormViewTester: Storm Created Subject Completed".Bolded)).AddTo(this);
            
            MessageBroker.Default.Publish(creationParams);
            _config.Log("StormViewTester: Storm Creation Request Sent".Bolded());
        }

        private void OnDrawGizmos()
        {
            if (_storm == null) return;
            var bounds = _storm.InnerBoundsWs;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
            bounds.Expand((Vector3)_storm.Falloff);
            Gizmos.color = Color.red.SetAlpha(0.5f);
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}