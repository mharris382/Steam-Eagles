using System;

namespace Characters.Narrative
{
    [Flags]
    public enum CrewMemberStatus
    {
        CIVILIAN = 0,
        PILOT = 1,
        ENGINEER = 2,
        OFFICER = 4
    }
}