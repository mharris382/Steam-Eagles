```cs
public interface IAirshipControls  
{  
    float ThrusterPowerAsPercent { get; }  
    float HeatPowerAsPercent { get; }  
    IPilot CurrentPilot { get; }  
    void DecreasePowerToThrusters();  
    void IncreasePowerToThrusters();  
    void DecreasePowerToHeat();  
    void IncreasePowerToHeat();  
    void AssignPilot(IPilot pilot);  
}
```
