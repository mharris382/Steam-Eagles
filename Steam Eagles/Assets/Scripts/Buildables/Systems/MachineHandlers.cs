namespace Buildables
{
    public class MachineHandlers
    {
        public MachineHandler<HyperPump> hyperPumps;
        public MachineHandler<HypergasGenerator> generators;
        public MachineHandler<SteamTurbine> turbines;
        public MachineHandler<ExhaustVent> vents;


        public MachineHandlers(MachineHandler<HyperPump> hyperPumps, MachineHandler<HypergasGenerator> generators, MachineHandler<SteamTurbine> turbines, MachineHandler<ExhaustVent> vents)
        {
            this.hyperPumps = hyperPumps;
            this.generators = generators;
            this.turbines = turbines;
            this.vents = vents;
        }
    }
}