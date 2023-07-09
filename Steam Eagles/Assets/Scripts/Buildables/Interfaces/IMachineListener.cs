namespace Buildables.Interfaces
{
    public interface IMachineListener
    {
        void OnMachineBuilt(BuildableMachineBase machineBase);
        void OnMachineRemoved(BuildableMachineBase machineBase);
    }
}