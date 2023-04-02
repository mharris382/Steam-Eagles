using UnityEngine;

namespace Puzzles.GasNetwork
{
    public class PipeNode
    {
        public Vector3Int Position { get; set; }
        public int GasStored { get; set; }
        public int GasCapacity { get; set; }
        
        
        public PipeNode(Vector3Int position, int gasCapacity=10)
        {
            Position = position;
            GasCapacity = gasCapacity;
            GasStored = 0;
        }

        public void AddGas(int amount)
        {
            GasStored += amount;
            if (GasStored > GasCapacity)
            {
                GasStored = GasCapacity;
            }
        }

        public void RemoveGas(int amount)
        {
            GasStored -= amount;
            if (GasStored < 0)
            {
                GasStored = 0;
            }
        }
        
        
    }
}