```cs
public interface IPilot  
{  
    string tag { get; }  
    float XInput { get; }  
    float YInput { get; }  
    event System.Action<int> OnPowerToThrustersChanged;  
    event System.Action<int> OnPowerToHeatChanged;  
    void NotifyGainControls(AirshipControls controls);  
    void NotifyLostControls(AirshipControls controls);  
}
```
%% see [[New Character Systems]] %%