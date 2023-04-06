using System;

namespace CoreLib
{
    [Flags]
    public enum AccessLevel
    {
        PASSENGERS = 1 << 0,
        ENGINEERS = 1 << 1,
        OFFICERS = 1 << 2,
        PILOTS = 1 << 3,
        CREW_MEMBERS = ENGINEERS | OFFICERS | PILOTS,
        
        EVERYONE = CREW_MEMBERS | PASSENGERS
    }
}