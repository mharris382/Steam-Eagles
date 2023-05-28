using UniRx;
using System;
using UnityEngine;

namespace CoreLib.GameTime
{
    public class GameTimeState
    {
        private ReactiveProperty<TimeMode> _timeMode = new ReactiveProperty<TimeMode>();
        
        
        /// <summary>
        /// certain narrative events may require that time does not progress beyond a certain point until the event is complete
        /// For example: if the next mission is supposed to take place at night, and the current mission is taking place
        /// before night, we can only progress time up until prior to nightfall until the current mission is complete 
        /// </summary>
        private GameDateTime? _currentTimeMaximum;
        private Func<bool> _continueCondition;
        
        private GameDateTime _currentTime;
        
        
        public TimeMode Mode
        {
            get => _timeMode.Value;
            set => _timeMode.Value = value;
        }

        public GameTime GlobalGameTime
        {
            get => _currentTime.gameTime;
            set
            {
                _currentTime.gameTime = value;
                Debug.Log($"Global Game Time set to {value}");
            }
        }

        public GameDate GlobalGameDate
        {
            get => _currentTime.gameDate;
            set
            {
                _currentTime.gameDate = value;
                Debug.Log($"Global Game Date set to {value}");
            }
        }

        public GameDateTime CurrentTime
        {
            get => _currentTime;
            set
            {
                _currentTime = value;
                Debug.Log($"Current Time set to {value}");
            }
        }

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