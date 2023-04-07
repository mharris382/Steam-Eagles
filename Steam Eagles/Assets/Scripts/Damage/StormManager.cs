using System;
using Buildings;
using CoreLib;
using UnityEngine;
using UniRx;
namespace Damage
{
    public class StormManager : Singleton<StormManager>
    {
        private DamageController _damageController;
        public BuildingDamage damageableBuildingLayer;

        public override bool DestroyOnLoad => true;

        private CompositeDisposable _disposable;
        private StormInstance _activeStorm;

        protected override void Init()
        {
            _damageController = GetComponent<DamageController>();
            _damageController.enabled = false;
            base.Init();
        }
        
        public StormInstance StartStorm(int intensity)
        {
            var storm = StormDatabase.Instance.GetStorm(intensity);
            return StartStorm(storm);
        }

        public StormInstance StartStorm(Storm storm)
        {
            if (_activeStorm != null)
            {
                StopStorm(_activeStorm);
            }
            _activeStorm = new StormInstance(storm);
            
            _activeStorm.CreateStorm();
            _disposable = new CompositeDisposable();
            var damageSystem = new StormDamageSystem(storm, _damageController , damageableBuildingLayer.GetDamageableLayer(BuildingLayers.WALL));
            damageSystem.Start(_activeStorm, _disposable);
            return _activeStorm;
        }

        public void StopStorm(StormInstance storm)
        {
            if (_activeStorm == storm)
            {
                _disposable?.Dispose();
                _activeStorm?.Dispose();
                _activeStorm = null;
            }
        }
    }
}