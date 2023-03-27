namespace CoreLib.Interactions
{
    public interface IPilot
    {
        float XInput { get; }
        float YInput { get; }
        event System.Action<int> OnPowerToThrustersChanged;
        event System.Action<int> OnPowerToHeatChanged;
        void NotifyGainControls(AirshipControls controls);
        void NotifyLostControls(AirshipControls controls);
    }
}