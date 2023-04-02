namespace CoreLib.Interactions
{
    public interface IPilot
    {
        string tag { get; }
        float XInput { get; }
        float YInput { get; }
        event System.Action<int> OnPowerToThrustersChanged;
        event System.Action<int> OnPowerToHeatChanged;
        void NotifyGainControls(IAirshipControls controls);
        void NotifyLostControls(IAirshipControls controls);
    }
}