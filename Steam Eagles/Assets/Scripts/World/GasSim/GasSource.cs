using System.Collections.Generic;
using UnityEngine;
using Rand = UnityEngine.Random;
namespace World.GasSim
{
    public class GasSource : CellHelper, IGasSource
    {
        public Vector2Int size = new Vector2Int(4, 4);

        [Tooltip("Slower numbers are faster")]
        [Range(16, 1)]
        [SerializeField] private int slowdown = 1;


        [SerializeField] private bool useConstantAmount;
        [Range(0, 16)] 
        [SerializeField] private int constantAmount = 1;
        
        [Range(0, 16)] public int amountMin = 1;
        [Range(1, 16)] public int amountMax = 1;


        private int _count;
        
        public IEnumerable<(Vector2Int coord, int amount)> GetSourceCells()
        {
            _count++;
            if ((_count % slowdown) != 0) yield break;
            
            Vector2Int c0 = (Vector2Int)CellCoordinate;
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    int amt = useConstantAmount ? constantAmount : Rand.Range(amountMin, amountMax);
                    if (amt > 0)
                    {
                        Vector2Int offset = new Vector2Int(x, y);
                        yield return (c0 + offset, amt);
                    }
                }
            }
        }
    }
}