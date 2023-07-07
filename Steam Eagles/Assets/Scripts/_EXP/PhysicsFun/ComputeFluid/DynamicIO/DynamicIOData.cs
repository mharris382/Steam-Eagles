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

        public DynamicIOData(DynamicIOObject ioObject)
        {
            size = ioObject.size;
            deltaIn = ioObject.GetTargetGasIOValue();
            deltaOut = 0;
            int resolution = ioObject.GasTexture.Resolution;
            var gridPos = ioObject.Building.Map.WorldToCell(ioObject.transform.position, BuildingLayers.SOLID);
            position = new Vector2Int(gridPos.x * resolution, gridPos.y * resolution);
        }
    }
}