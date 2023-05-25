using CoreLib;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Zenject;

namespace Weather.Storms.Views
{
    public class StormViewTester : MonoBehaviour
    {
        
        
        private StormView.Factory _stormViewFactory;

        private TestStorm _testStorm;
        private Storm.Factory _stormFactory;
        private StormView _stormView;
        private Storm _storm;
        private GlobalStormConfig _config;

        [Inject]
        public void Install(StormView.Factory stormViewFactory, GlobalStormConfig config, TestStorm testStorm, Storm.Factory stormFactory)
        {
            this._config = config;
            this._stormViewFactory = stormViewFactory;
            this._stormFactory = stormFactory;
            this._testStorm = testStorm;
        }


        [DisableInEditorMode]
        [Button(ButtonSizes.Gigantic)]
        public void SpawnStorm()
        {
            _stormView = _stormViewFactory.Create();
            var creationParams = _testStorm.GetStormCreationRequest();
            creationParams.StormCreatedSubject.First().Subscribe(_stormView.AssignStorm, () => _config.Log("StormViewTester: Storm Created Subject Completed".Bolded)).AddTo(this);
            
            MessageBroker.Default.Publish(creationParams);
            _config.Log("StormViewTester: Storm Creation Request Sent".Bolded());
        }
        
        
    }
}