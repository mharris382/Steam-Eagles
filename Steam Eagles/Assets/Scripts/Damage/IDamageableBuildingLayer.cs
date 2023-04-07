using System.Collections.Generic;
using UnityEngine;

namespace Damage
{
    public interface IDamageableBuildingLayer
    {
        public List<Vector2Int> GetDamageableTiles();
        
        public void DamageTile(Vector2Int tile, int damage);
    }
}