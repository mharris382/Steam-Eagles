using System;

namespace CoreLib.GameTime
{
    [System.Serializable]
    public struct GameTime
    {
        public int gameHour;
        public int gameMinute;
        public GameTime(int hour, int minute)
        {
            if (minute >= 60)
            {
                hour += minute / 60;
                minute = minute % 60;
                gameHour = hour;
                gameMinute = minute;
            }
            else
            {
                gameHour = hour;
                gameMinute = minute;
            }
        }
        
        public static GameTime operator +(GameTime gameTime1, GameTime gameTime2)
        {
            int hrs = gameTime1.gameHour + gameTime2.gameHour;
            int min = gameTime1.gameMinute + gameTime2.gameMinute;
            return new GameTime(hrs, min);
        }
        
        public static bool operator ==(GameTime gameTime1, GameTime gameTime2) => gameTime1.gameHour == gameTime2.gameHour && gameTime1.gameMinute == gameTime2.gameMinute;
        public static bool operator !=(GameTime gameTime1, GameTime gameTime2) => gameTime1.gameHour != gameTime2.gameHour || gameTime1.gameMinute != gameTime2.gameMinute;


        public float GetPercentageOfDay()
        {
            var minInDay = (24 * 60);
            var minFromHours = gameHour * 60;
            var totalMinutes = minFromHours + gameMinute;
            return totalMinutes / (float)minInDay;
        }
        public TimeSpan GetRealTimeSpan()
        {
            var secFromMinutes = GameTimeConfig.Instance.RealSecondsInGameMinute * gameMinute;
            var secFromHours = GameTimeConfig.Instance.RealSecondsInGameHour * gameHour;
            var totalSeconds = secFromMinutes + secFromHours;
            return TimeSpan.FromSeconds(totalSeconds);
        }
    }
}