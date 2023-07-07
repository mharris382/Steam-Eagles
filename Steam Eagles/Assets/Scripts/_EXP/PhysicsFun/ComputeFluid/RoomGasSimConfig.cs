using System;

namespace _EXP.PhysicsFun.ComputeFluid
{
    [Serializable]
    public class RoomGasSimConfig
    {
        public float syncRate = 0.25f;
        
        public float deleteGasInSolidRate = 0.1f;
    }
}