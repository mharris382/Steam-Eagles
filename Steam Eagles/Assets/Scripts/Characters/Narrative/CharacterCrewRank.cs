using System;
using CoreLib;
using UnityEngine;

namespace Characters.Narrative
{
    [Serializable]
    public class CharacterCrewRank
    {
        private const int MAX_RANK = 4;
        [SerializeField, Range(0, MAX_RANK)] private int pilotRank;
        [SerializeField, Range(0, MAX_RANK)] private int engineerRank;
        [SerializeField, Range(0, MAX_RANK)] private int officerRank;
        
        public int PilotRank => pilotRank;
        public int EngineerRank => engineerRank;
        public int OfficerRank => officerRank;
        

        public bool IsACrewMember => (PilotRank + EngineerRank + OfficerRank) > 0;

        public AccessLevel GetAccessLevel()
        {
            AccessLevel baseline = AccessLevel.CIVILIANS;
            int rankTotal = pilotRank + engineerRank + officerRank;
            if (rankTotal <= 0)
            {
                return baseline;
            }

            if (pilotRank > 0)
            {
                baseline |= AccessLevel.PILOTS;
            }
            if (engineerRank > 0)
            {
                baseline |= AccessLevel.ENGINEERS;
            }
            if (officerRank > 0)
            {
                baseline |= AccessLevel.OFFICERS;
            }
            
            return baseline;
        }
    }
}