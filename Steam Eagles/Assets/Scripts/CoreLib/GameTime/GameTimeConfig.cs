using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace CoreLib.GameTime
{
    [GlobalConfig("Resources/GameTimeConfig")]
    public class GameTimeConfig : GlobalConfig<GameTimeConfig>
    {
        [Tooltip("How many real minutes does it take to go 24 hours in game time")]
        public float realMinutesInGameDay = 20;

        [SerializeField, Tooltip("How many game hours in one game day")]
        private int hoursPerDay = 24;

        [ValueDropdown(nameof(GetValidDisplayIncrements))]
        [SerializeField] private int displayIncrementsPerHour = 6;

        ValueDropdownList<int> GetValidDisplayIncrements()
        {
            var vdl =new ValueDropdownList<int>();
            vdl.Add("Only Show Hours", 1);
            vdl.Add("Show Half Hours", 2);
            vdl.Add("Show 15 min increments", 4);
            vdl.Add("Show 10 min increments", 6);
            vdl.Add("Show 5 min increments", 12);
            vdl.Add("Show 1 min increments", 60);
            return vdl;
        }

        public GameDateTime gameStartTime;

        public float RealMinutesInGameHour => realMinutesInGameDay / (float)hoursPerDay;
        
        public float RealSecondsInGameHour => RealMinutesInGameHour * 60;
        
        
        public float RealSecondsInGameMinute => RealSecondsInGameHour / 60;
        
        /// <summary>
        /// how many seconds between UI updates
        /// </summary>
        public float RealSecondsPerDisplayIncrement => RealSecondsInGameHour / displayIncrementsPerHour;

        public int DisplayIncrementsPerHour => displayIncrementsPerHour;

        public int MinutesPerDisplayIncrement => 60 / displayIncrementsPerHour;

        public float IncrementAsPercentageOfTheDay => 1/(float)(DisplayIncrementsPerHour * 24);
    }
}