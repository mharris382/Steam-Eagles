using UniRx;
using System;
using UnityEngine;

namespace CoreLib.GameTime
{
    public class GameTimeState
    {
        private ReactiveProperty<TimeMode> _timeMode = new ReactiveProperty<TimeMode>();
        private ReactiveProperty<GameDateTime> _currentTime = new();



        /// <summary>
        /// certain narrative events may require that time does not progress beyond a certain point until the event is complete
        /// For example: if the next mission is supposed to take place at night, and the current mission is taking place
        /// before night, we can only progress time up until prior to nightfall until the current mission is complete 
        /// </summary>
        private GameDateTime? _currentTimeMaximum;

        private Func<bool> _continueCondition;


        public TimeMode Mode
        {
            get => _timeMode.Value;
            set => _timeMode.Value = value;
        }

        public GameTime GlobalGameTime
        {
            get => CurrentTime.gameTime;
            set
            {
                var t = CurrentTime;
                t.gameTime = value;
                CurrentTime = t;
                Debug.Log($"Global Game Time set to {value}");
            }
        }

        public GameDate GlobalGameDate
        {
            get => CurrentTime.gameDate;
            set
            {
                var t = CurrentTime;
                t.gameDate = value;
                CurrentTime = t;
                Debug.Log($"Global Game Date set to {value}");
            }
        }

        public GameDateTime CurrentTime
        {
            get => _currentTime.Value;
            set
            {
                _currentTime.Value = value;
                Debug.Log($"Current Time set to {value}");
            }
        }
        
        
        
        public IObservable<GameTime> OnGameTimeUpdated => _currentTime.Select(t => t.gameTime);
        public IObservable<GameDate> OnGameDateUpdated => _currentTime.Select(t => t.gameDate);
        
        public IObservable<GameDateTime> OnGameDateTimeUpdated => _currentTime;

        public bool HasTimeMaximumBeenSet => _currentTimeMaximum.HasValue;
        
        public GameDateTime CurrentTimeMaximum => _currentTimeMaximum ?? GameDateTime.MaxValue;
        
        public bool HasMaximumTimeBeenReached => _currentTimeMaximum.HasValue && CurrentTime >= _currentTimeMaximum.Value;
        
        
        public GameTimeState()
        {
            Mode = TimeMode.PAUSED;
        }

        public bool IsTimeMoving => Mode != TimeMode.PAUSED && !IsTimeBlockedByLimit();
        
        
        public void SetTimeMaximum(GameDateTime timeMaximum, System.Func<bool> continueCondition)
        {
            if (timeMaximum < CurrentTime)
            {
                throw new InvalidOperationException(
                    $"Cannot clamp time to a value in the past.\n current time = {CurrentTime}\n time maximum = {timeMaximum}");
            }
            _currentTimeMaximum = timeMaximum;
            _continueCondition = continueCondition;
        }

        internal bool IsTimeBlockedByLimit()
        {
            if (!HasTimeMaximumBeenSet)
            {
                return false;
            }

            if (!HasMaximumTimeBeenReached)
            {
                return false;
            }

            if (_continueCondition())
            {
                ClearTimeMaximum();
                return false;
            }

            return true;
        }

        private void ClearTimeMaximum()
        {
            _continueCondition = null;
            _currentTimeMaximum = null;
        }
    }
}