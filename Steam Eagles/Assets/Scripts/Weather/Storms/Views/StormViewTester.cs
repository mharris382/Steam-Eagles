using Sirenix.OdinInspector;
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

        [Inject]
        public void Install(StormView.Factory stormViewFactory, TestStorm testStorm, Storm.Factory stormFactory)
        {
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
            _stormView.AssignStorm(_storm = _stormFactory.Create(creationParams.StormBounds,  creationParams.StormVelocity, creationParams.StormFalloff));
        }
        
        
    }
}