using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Damage
{
    public class BasicDamageRollStrategy : IDamageRollStrategy, IDamageRollsStrategy
    {
        Vector3Int GetRandomCellPosition()
        {
            return Vector3Int.zero;
        }
        public bool RollForHit(RollParameters rollParameters, out Vector3Int cellPosition)
        {
            var percent = rollParameters.Percent;
            var chances = rollParameters.Chances;
            cellPosition = Vector3Int.zero;
            
            bool[] rolls = new bool[Mathf.Min(1, chances)];
            Vector3Int[] positions = new Vector3Int[rolls.Length];
            
            for (int i = 0; i < rolls.Length; i++)
            {
                rolls[i] = Random.value > percent;
                positions[i] = rolls[i] ? GetRandomCellPosition() : Vector3Int.zero;
            }

            var index = Random.Range(0, rolls.Length);
            cellPosition = positions[index];
            
            return rolls[index];
        }

        public int RollForHits(RollParameters rollParameters, out IEnumerable<Vector3Int> cellHits)
        {
            var percent = rollParameters.Percent;
            var chances = rollParameters.Chances;
            
            
            bool[] rolls = new bool[Mathf.Min(1, chances)];
            Vector3Int[] positions = new Vector3Int[rolls.Length];
            
            int hitCount = 0;
            for (int i = 0; i < rolls.Length; i++)
            {
                rolls[i] = Random.value > percent;
                positions[i] = rolls[i] ? GetRandomCellPosition() : Vector3Int.zero;
                if (rolls[i]) hitCount++;
            }

            if(hitCount > 0)
                cellHits = ReturnHits(rolls, positions);
            else
                cellHits = null;
            
            return hitCount;
        }

        private IEnumerable<Vector3Int> ReturnHits(bool[] rolls, Vector3Int[] hits)
        {
            for (int i = 0; i < rolls.Length; i++)
            {
                if (rolls[i])
                {
                    yield return hits[i];
                }
            }
        }
    }
}