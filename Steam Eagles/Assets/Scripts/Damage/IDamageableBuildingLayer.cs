using System;
using System.Collections.Generic;
using UnityEngine;

namespace Damage
{
    public interface IDamageableBuildingLayer
    {
        public int RoomCount { get; }
        public List<Vector2Int> GetDamageableTilesInRoom(int roomIndex);
        public List<TileHandle> GetDamageableTiles();
        
        public TileHandle GetHandle(Vector2Int tile);
        
        public void DamageTile(Vector2Int tile, int damage);
    }

    public class TileHandle
    {
        private bool _isDamaged;
        private readonly Action _onDamage;
        private readonly Action _onRepair;
        public event Action Damaged;
        public event Action Repaired;
        public TileHandle(Vector2Int cell, System.Action onDamage, System.Action onRepair, bool initialDamageState = false)
        {
            this.Cell = cell;
            _onDamage = onDamage;
            _onRepair = onRepair;
            _isDamaged = initialDamageState;
            Damaged = null;
            Repaired = null;
            Repaired += SetRepaired;
            Repaired += SetDamaged;

        }

        void SetDamaged()
        {
            _isDamaged = true;
        }

        void SetRepaired()
        {
            _isDamaged = false;
        }

        public Vector2Int Cell { get; }

        public bool CanDamage => !_isDamaged;

        public bool CanRepair => _isDamaged;
        public void DamageCell()
        {
            if (CanDamage)
            {
                _onDamage();
                Damaged?.Invoke();
                
            }
        }

        public void RepairCell()
        {
            if (CanRepair)
            {
                _onRepair();
                Repaired?.Invoke();
                _isDamaged = false;
            }
        }
    }
}