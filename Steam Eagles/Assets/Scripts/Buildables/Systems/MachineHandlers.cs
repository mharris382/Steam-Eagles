namespace Buildables
{
    public class MachineHandlers
    {
        public readonly MachineHandler<HyperPump> hyperPumps;
        public readonly MachineHandler<HypergasGenerator> generators;
        public readonly MachineHandler<SteamTurbine> turbines;
        public readonly MachineHandler<ExhaustVent> vents;


        public MachineHandlers(MachineHandler<HyperPump> hyperPumps, MachineHandler<HypergasGenerator> generators, MachineHandler<SteamTurbine> turbines, MachineHandler<ExhaustVent> vents)
        {
            this.hyperPumps = hyperPumps;
            this.generators = generators;
            this.turbines = turbines;
            this.vents = vents;
        }
    }
}