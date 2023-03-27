using CoreLib.Interactions;

namespace CoreLib
{
    public struct AirshipPilotChangedInfo
    {
        public readonly string prevPilotName;
        public readonly string newPilotName;
        public readonly AirshipControls controls;

        public AirshipPilotChangedInfo(string prevPilotName, string newPilotName, AirshipControls controls)
        {
            this.prevPilotName = prevPilotName;
            this.newPilotName = newPilotName;
            this.controls = controls;
        }
    }
}