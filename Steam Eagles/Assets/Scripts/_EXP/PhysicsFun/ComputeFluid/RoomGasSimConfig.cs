using System;

namespace _EXP.PhysicsFun.ComputeFluid
{
    [Serializable]
    public class RoomGasSimConfig
    {
        public float syncRate = 0.25f;
        public float dynamicIORate = 0.5f;
        public float deleteGasInSolidRate = 0.1f;
        
        
        public SimConfigSettings simSettings = new SimConfigSettings();
    }


    [Serializable]
    public class SimConfigSettings
    {
        public int solverIterations = 20;
        public float viscosity = 0.1f;
        public float forceStrength = 1f;
        public float forceRadius = 1;
        public float forceFalloff = 2;
        public float velocityDissipation = 0.99f;
    }
}