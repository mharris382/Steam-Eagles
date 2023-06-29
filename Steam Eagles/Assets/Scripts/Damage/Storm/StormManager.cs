using System;
using Buildings;
using CoreLib;
using UnityEngine;
using UniRx;
namespace Damage
{
    [Obsolete("Use Storm View System")]
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
            throw new NotImplementedException();
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