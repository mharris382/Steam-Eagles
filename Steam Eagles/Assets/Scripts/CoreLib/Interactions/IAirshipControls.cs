namespace CoreLib.Interactions
{
    public interface IAirshipControls
    {
        string name { get; }
        float ThrusterPowerAsPercent { get; }
        float HeatPowerAsPercent { get; }
        IPilot CurrentPilot { get; }
        void DecreasePowerToThrusters();
        void IncreasePowerToThrusters();
        void DecreasePowerToHeat();
        void IncreasePowerToHeat();
        void AssignPilot(IPilot pilot);
    }
}