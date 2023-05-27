using System;
using System.Collections;
using UnityEngine;
using Zenject;

namespace CoreLib.GameTime
{
    public class TimeRunner : IInitializable, IDisposable
    {
        private readonly IGameTimeConfig _config;
        private readonly GameTimeState _gameTimeState;
        private readonly CoroutineCaller _coroutineCaller;
        private Coroutine _timeUpdate;



        public bool Running
        {
            get;
            private set;
        }
        public int Increments
        {
            get;
            private set;
        }
        public int IncrementsMin
        {
            get;
            private set;
        }
        public int IncrementsMinMoved
        {
            get;
            private set;
        }
        public int IncrementsHours
        {
            get;
            private set;
        }
        public int IncrementsHoursMoved
        {
            get;
            private set;
        }
        private GameTime GlobalGameTime
        {
            get => _gameTimeState.GlobalGameTime;
            set => _gameTimeState.GlobalGameTime = value;
        }
        public GameDate GlobalGameDate
        {
            get => _gameTimeState.GlobalGameDate;
            set => _gameTimeState.GlobalGameDate = value;
        }
        public TimeRunner(IGameTimeConfig config, GameTimeState gameTimeState, CoroutineCaller coroutineCaller)
        {
            _config = config;
            _gameTimeState = gameTimeState;
            _coroutineCaller = coroutineCaller;
        }
        
        public void Initialize()
        {
            
          _timeUpdate =  _coroutineCaller.StartCoroutine(UpdateGameDate());
        }
        
        IEnumerator UpdateGameDate()
        {
            Increments = 0;
            IncrementsHours = 0;
            IncrementsMin = 0;
            IncrementsHoursMoved = 0;
            IncrementsMinMoved = 0;
            while (true)
            {
                Running = true;
                Debug.Log("Updating Game Date");
                yield return WaitUntilNotPaused();
                yield return UpdateGameTime();
                IncrementGameDay();
                Increments++;
                var newDateTime = _gameTimeState.CurrentTime;
                newDateTime.Minutes = 0;
                newDateTime.Hour = 0;
                
                if (newDateTime.Day % 7 == 0) 
                    newDateTime.Week += 1;
                
                newDateTime.Day += 1;
                _gameTimeState.CurrentTime = newDateTime;
            }
        }
        
        IEnumerator UpdateGameTime()
        {
            int hoursRemainingInDay = 24;
            
            while (hoursRemainingInDay > 0)
            {
                Debug.Log($"Updating Game Hours\t{hoursRemainingInDay.ToString()} hours remaining in day");
                yield return WaitUntilNotPaused();
                yield return UpdateGameMinute();
                if(IncrementGameHour(24 - hoursRemainingInDay))
                {
                    var newDateTime = _gameTimeState.CurrentTime;
                    newDateTime.Hour += 1;
                    newDateTime.Minutes = 0;
                    _gameTimeState.CurrentTime = newDateTime;
                    
                    hoursRemainingInDay -= 1;
                    IncrementsHoursMoved++;
                    Debug.Log($"Updating Game Hours\t{hoursRemainingInDay.ToString()} hours remaining in day");
                }

                IncrementsHours++;
            }
        }

        IEnumerator UpdateGameMinute()
        {
            int minRemainingInHour = 60;
            while (minRemainingInHour > 0)
            {
                Debug.Log($"Updating Game Minutes:\t {minRemainingInHour.ToString()} minutes remaining in hour");
                yield return WaitUntilNotPaused();
                yield return new WaitForSeconds(_config.RealSecondsInGameMinute);
                if (IncrementGameMinutes(60 - minRemainingInHour))
                {
                    var newDateTime = _gameTimeState.CurrentTime;
                    newDateTime.Minutes += 1;
                    _gameTimeState.CurrentTime = newDateTime;
                    minRemainingInHour -= 1;
                    IncrementsMinMoved++;
                }
                IncrementsMin++;
            }
        }

        IEnumerator WaitUntilNotPaused()
        {
            while (_gameTimeState.Mode == TimeMode.PAUSED)
            {
                yield return null;
            }
        }

        private void IncrementGameDay()
        {
            if (_gameTimeState.IsTimeBlockedByLimit())
            {
                LogTimeBlocked();
                return;
            }
            var gameDate = GlobalGameDate;
            gameDate.gameDays += 1;
            GlobalGameDate = gameDate;
            Debug.Assert(_gameTimeState.GlobalGameDate == gameDate);
            LogDateChanged();
        }

        private bool IncrementGameHour(int hoursRemainingInDay)
        {
            if (_gameTimeState.IsTimeBlockedByLimit())
            {
                return false;
            }
            var gameTime = GlobalGameTime;
            gameTime.gameHour += 1;
            GlobalGameTime = gameTime;
            LogTimeChanged();
            return true;
        }

        private bool IncrementGameMinutes(int minutes)
        {
            if (_gameTimeState.IsTimeBlockedByLimit())
            {
                LogTimeBlocked();
                return false;
            }
            GlobalGameTime = new GameTime(GlobalGameTime.gameHour, minutes);
            LogTimeChanged();
            return true;
        }

        void LogTimeBlocked() => _config.Log($"Time has reached the maximum time limit: {_gameTimeState.CurrentTime} and will no longer advance until condition has been met.");
        void LogTimeChanged() => _config.Log($"Time updated: {GlobalGameTime.ToString()}");
        void LogDateChanged() => _config.Log($"Time updated: {GlobalGameDate.ToString()}");

        public void Dispose()
        {
            if(_timeUpdate != null)
                _coroutineCaller.StopCoroutine(_timeUpdate);
        }
    }
}