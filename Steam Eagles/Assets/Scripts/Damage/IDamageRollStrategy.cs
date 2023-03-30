using System.Collections.Generic;
using UnityEngine;

namespace Damage
{
    public interface IDamageRollStrategy
    {
        public bool RollForHit(RollParameters rollParameters, out Vector3Int cellPosition);
    }
    public interface IDamageRollsStrategy
    {
        public int RollForHits(RollParameters rollParameters, out  IEnumerable<Vector3Int> cellHits);
    }
}