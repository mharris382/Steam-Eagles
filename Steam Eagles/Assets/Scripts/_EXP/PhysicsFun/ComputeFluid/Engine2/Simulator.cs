namespace _EXP.PhysicsFun.ComputeFluid.Engine2
{
    public class Simulator
    {
        private readonly SimConfigSettings _settings;
        
        

        public int SolverIterations => _settings.solverIterations;
        public float Viscosity => _settings.viscosity; 
        public float ForceStrength => _settings.forceStrength;
        public float ForceRadius => _settings.forceRadius;
        public float ForceFalloff => _settings.forceFalloff;
        public float VelocityDissipation => _settings.velocityDissipation; 
        public Simulator(RoomGasSimConfig simConfig)
        {
            _settings = simConfig.simSettings;
        }

        public void Simulate(SimResources simResources, float timeDelta)
        {
            
        }
    }
}