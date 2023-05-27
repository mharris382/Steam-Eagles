namespace CoreLib.GameTime
{
    [System.Serializable]
    public struct GameDateTime
    {
        public GameDate gameDate;
        public GameTime gameTime;

        public static GameDateTime GameStart => GameTimeConfig.Instance.gameStartTime;

        public int Minutes
        {
            get { return gameTime.gameMinute; }

            set => gameTime.gameMinute = value;
        }

        public int Hour
        {
            get { return gameTime.gameHour; }

            set => gameTime.gameHour = value;
        }

        public int Day
        {
            get { return gameDate.gameDays; }

            set => gameDate.gameDays = value;
        }

        public int Week
        {
            get { return gameDate.gameWeeks; }

            set => gameDate.gameWeeks = value;
        }


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
        public static bool operator !=(GameDateTime dateTime, GameDateTime dateTime2)
        {
            return !(dateTime == dateTime2);
        }
        public static bool operator ==(GameDateTime dateTime, GameDateTime dateTime2)
        {
            return dateTime.Day == dateTime2.Day 
                && dateTime.Hour == dateTime2.Hour
                && dateTime.Minutes == dateTime2.Minutes
                && dateTime.Week == dateTime2.Week;
        }
        public override string ToString()
        {
            return $"{gameTime}\t {gameDate}";
        }

        public static bool operator <(GameDateTime dateTime, GameDateTime dateTime2)
        {
            if(dateTime.gameDate == dateTime2.gameDate)
            {
                return dateTime.gameTime < dateTime2.gameTime;
            }
            else
            {
                return dateTime.gameDate < dateTime2.gameDate;
            }
        }
        public static bool operator >(GameDateTime dateTime, GameDateTime dateTime2)
        {
            if(dateTime.gameDate == dateTime2.gameDate)
            {
                return dateTime.gameTime > dateTime2.gameTime;
            }
            else
            {
                return dateTime.gameDate > dateTime2.gameDate;
            }
        }
        public static bool operator <=(GameDateTime dateTime, GameDateTime dateTime2)
        {
            if(dateTime.gameDate == dateTime2.gameDate)
            {
                return dateTime.gameTime <= dateTime2.gameTime;
            }
            else
            {
                return dateTime.gameDate <= dateTime2.gameDate;
            }
        }
        public static bool operator >=(GameDateTime dateTime, GameDateTime dateTime2)
        {
            if(dateTime.gameDate == dateTime2.gameDate)
            {
                return dateTime.gameTime >= dateTime2.gameTime;
            }
            else
            {
                return dateTime.gameDate >= dateTime2.gameDate;
            }
        }
    }
}