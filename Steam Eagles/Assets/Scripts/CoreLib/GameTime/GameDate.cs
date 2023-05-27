namespace CoreLib.GameTime
{
    [System.Serializable]
    public struct GameDate
    {
        public int gameWeeks;
        public int gameDays;
        public GameDate(int weeks, int days)
        {
            if (days > 7)
            {
                weeks += days / 7;
                days = days % 7;
                this.gameDays = days;
                this.gameWeeks = weeks;
            }
            else
            {
                this.gameDays = days;
                this.gameWeeks = weeks;
            }
        }
        public static GameDate operator +(GameDate d1, GameDate d2)
        {
            var wk = d1.gameWeeks + d2.gameWeeks;
            var day =  d1.gameDays + d2.gameDays;
            return new GameDate(wk, day);
        }
        public static GameTime operator +(GameDate d1, GameTime t1)
        {
            var wk = d1.gameWeeks;
            var day =  d1.gameDays;
            var hrs = t1.gameHour + (wk * 7 + day) * 24;
            var min = t1.gameMinute;
            return new GameTime(hrs, min);
        }

        public static bool operator ==(GameDate d1, GameDate d2) => d1.gameDays == d2.gameDays && d1.gameWeeks == d2.gameWeeks;

        public static bool operator !=(GameDate d1, GameDate d2) => d1.gameDays != d2.gameDays || d1.gameWeeks != d2.gameWeeks;
        
        public static bool operator <(GameDate d1, GameDate d2)
        {
            if (d1.gameWeeks == d2.gameWeeks)
            {
                return d1.gameDays < d2.gameDays;
            }
            else
            {
                return d1.gameWeeks < d2.gameWeeks;
            }
        }
        
        public static bool operator >(GameDate d1, GameDate d2)
        {
            if (d1.gameWeeks == d2.gameWeeks)
            {
                return d1.gameDays > d2.gameDays;
            }
            else
            {
                return d1.gameWeeks > d2.gameWeeks;
            }
        }
        public static bool operator <=(GameDate d1, GameDate d2)
        {
            if (d1.gameWeeks == d2.gameWeeks)
            {
                return d1.gameDays <= d2.gameDays;
            }
            else
            {
                return d1.gameWeeks <= d2.gameWeeks;
            }
        }
        
        public static bool operator >=(GameDate d1, GameDate d2)
        {
            if (d1.gameWeeks == d2.gameWeeks)
            {
                return d1.gameDays >= d2.gameDays;
            }
            else
            {
                return d1.gameWeeks >= d2.gameWeeks;
            }
        }

        public override string ToString()
        {
            return $"{gameWeeks} weeks {gameDays} days";
        }
    }
}