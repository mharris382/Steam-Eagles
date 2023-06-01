using CoreLib.Interactions;
using PhysicsFun.Airships.Installer;
using UnityEngine;
using Zenject;

public class FlightControlsV2 : MonoBehaviour
{
    public AirshipControls controls;
    public Rigidbody2D balloonBody;
    private IHeatPowerToGravityScale heaPowerToGravityScale;
    private IThrusters thrusters;

    [Inject]
    public void Inject(IHeatPowerToGravityScale heatPowerToGravityScale, IThrusters thrusters)
    {
        this.heaPowerToGravityScale = heatPowerToGravityScale;
        this.thrusters = thrusters;
    }

    private void Update()
    {
        balloonBody.gravityScale = heaPowerToGravityScale.GetGravityScaleAtHeatPower(controls.HeatPowerAsPercent, transform.position.y);
        thrusters.SetDirection(new Vector2(controls.xInput, controls.yInput));
        thrusters.SetPower(controls.ThrusterPowerAsPercent);
    }
}