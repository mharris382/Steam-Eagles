using System;
using UniRx;
using UnityEngine;

namespace Damage
{
    public class StormDamageSystem
    {
        private readonly Storm _storm;
        private readonly DamageController _damageController;
        private readonly IDamageableBuildingLayer _damageable;

        public StormDamageSystem(Storm storm,
            DamageController damageController, IDamageableBuildingLayer damageable)
        {
            _storm = storm;
            _damageController = damageController;
            _damageable = damageable;
        }

        public void Start(StormInstance activeStorm, CompositeDisposable disposable)
        {
            Debug.Log("Storm Damage System Started");
            activeStorm.StormProgress.Buffer(TimeSpan.FromSeconds(_storm.DamageCheckIntervalInSeconds)).Subscribe(_ =>
            {
                Debug.Log("Storm Damage Check");
            });
        }
    }
}