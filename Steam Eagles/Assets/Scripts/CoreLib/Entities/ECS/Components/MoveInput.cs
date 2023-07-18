using Unity.Entities;
using Unity.Mathematics;

namespace CoreLib.MyEntities.ECS
{
    public struct MoveInput : IComponentData
    {
        public float2 Value;
    }
}