﻿using System;
using System.Collections.Generic;
using Damage.Signals;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Damage
{
    [System.Obsolete("This is no good")]
    public class StormDamagePositionPicker : IDisposable
    {
        List<TileHandle> _damageableTiles;
        List<TileHandle> _repairableTiles;
        private readonly CompositeDisposable disposable;

        public StormDamagePositionPicker(IDamageableBuildingLayer damageable)
        {
            Damageable = damageable;
            _damageableTiles = damageable.GetDamageableTiles();
            _repairableTiles = new List<TileHandle>();
            CompositeDisposable compositeDisposable = new CompositeDisposable();
            foreach (var damageableTile in _damageableTiles)
            {
                void OnTileDamaged()
                {
                    _repairableTiles.Add(damageableTile);
                    _damageableTiles.Remove(damageableTile);
                }
                void OnTileRepaired()
                {
                    _damageableTiles.Add(damageableTile);
                    _repairableTiles.Remove(damageableTile);
                }
                damageableTile.Damaged += OnTileDamaged;
                damageableTile.Repaired += OnTileRepaired;
                compositeDisposable.Add(Disposable.Create(() => damageableTile.Damaged -= OnTileDamaged));
                compositeDisposable.Add(Disposable.Create(() => damageableTile.Repaired -= OnTileRepaired));
            }

            this.disposable = compositeDisposable;
        }

        public IDamageableBuildingLayer Damageable { get;  }
        
        public (Vector3Int pos, TileHandle handle) PickRandomDamageableTile()
        {
            var index = UnityEngine.Random.Range(0, _damageableTiles.Count);
            if (_damageableTiles.Count == 0)
                throw new Exception("No Damageable Tiles Found");
            var handle = _damageableTiles[index];
            return ((Vector3Int)handle.Cell, handle);
        }


        public void Dispose()
        {
            disposable?.Dispose();
        }
    }
    
    
    [System.Obsolete("This is no good")]
    public class StormDamageSystem
    {
        private readonly Storm _storm;
        private readonly DamageController _damageController;
        private readonly IDamageableBuildingLayer _damageable;
        private readonly StormDamagePositionPicker _posPicker;

        public StormDamageSystem(Storm storm,
            DamageController damageController, IDamageableBuildingLayer damageable)
        {
            _storm = storm;
            _damageController = damageController;
            _damageable = damageable;
            var basicStrategy = new BasicDamageRollStrategy();
            storm.damageCalculator.RollsStrategy = basicStrategy;
            storm.damageCalculator.RollStrategy = basicStrategy;
            this._posPicker = new StormDamagePositionPicker(damageable);
        }

        public void Start(StormInstance activeStorm, CompositeDisposable disposable)
        {
            Debug.Log("Storm Damage System Started");
           
            activeStorm.StormProgress.Buffer(TimeSpan.FromSeconds(_storm.DamageCheckIntervalInSeconds))
                    .Subscribe(_ =>
                    {
                        MessageBroker.Default.Publish(new RandomlyPlacedDamageRequest());
                    },
                    () => _posPicker.Dispose());
        }
    }
}