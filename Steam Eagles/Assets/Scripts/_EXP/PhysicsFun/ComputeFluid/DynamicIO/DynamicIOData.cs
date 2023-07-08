using Buildings;
using UnityEngine;

namespace _EXP.PhysicsFun.ComputeFluid
{
    public struct DynamicIOData
    {
        public Vector2Int position;
        public Vector2Int size;
        public float deltaIn;
        public float deltaOut;
        public static int Stride() => sizeof(int) * 4 + sizeof(float) * 2;


        public DynamicIOData(Vector2Int position, Vector2Int size, float deltaIn, float deltaOut)
        {
            this.position = position;
            this.size = size;
            this.deltaIn = deltaIn;
            this.deltaOut = deltaOut;
        }
    }
}