namespace CoreLib.GameTime
{
    [System.Serializable]
    public struct GameDateTime
    {
        public GameDate gameDate;
        public GameTime gameTime;

        public static GameDateTime GameStart => GameTimeConfig.Instance.gameStartTime;
        
        public GameDateTime(int weeks, int days, int hour, int minute)
        {
            gameDate = new GameDate(weeks, days);
            gameTime = new GameTime(hour, minute);
        }

        public GameDateTime(GameDate date, GameTime time) : this(date.gameWeeks, date.gameDays, time.gameHour,
            time.gameMinute)
        {
            
        }
        
        public static GameDateTime operator +(GameDateTime dateTime, GameDate date)
        {
            var wk = date.gameWeeks + dateTime.gameDate.gameWeeks;
            var day =  date.gameDays + dateTime.gameDate.gameDays;
            var hrs = dateTime.gameTime.gameHour;
            var min = dateTime.gameTime.gameMinute;
            return new GameDateTime(wk, day, hrs, min);
        }

        public static GameDateTime operator +(GameDateTime dateTime, GameTime time)
        {
            var wk = dateTime.gameDate.gameWeeks;
            var day =  dateTime.gameDate.gameDays;
            var hrs = time.gameHour + dateTime.gameTime.gameHour;
            var min = time.gameMinute + dateTime.gameTime.gameMinute;
            return new GameDateTime(wk, day, hrs, min);
        }
    }
}