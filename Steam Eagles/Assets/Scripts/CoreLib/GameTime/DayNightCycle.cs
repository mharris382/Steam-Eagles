using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;
using UniRx;
namespace CoreLib.GameTime
{
    public interface IDayNightListener
    {
        void OnTimeChanged(GameTime gameTime);
        void OnDayStarted();
        void OnNightStarted();
    }

    public class DayNightListeners : Registry<IDayNightListener>
    {
        
    }
    public class DayNightCycle : MonoBehaviour
    {
        private GameTimeState _gameTimeState;
        private DayNightListeners _dayNightListeners;

        [ShowInInspector, BoxGroup("Debugging"), HideInEditorMode] public  GameTime Time => _gameTimeState == null ? default : _gameTimeState.GlobalGameTime;
        [ShowInInspector, BoxGroup("Debugging"), HideInEditorMode]  public float PercentOfDay => _gameTimeState == null ? -1 : _gameTimeState.GlobalGameTime.GetPercentageOfDay();
        [ShowInInspector, BoxGroup("Debugging"), HideInEditorMode]  public TimeOfDay TimeOfDay =>  _gameTimeState == null  ? TimeOfDay.NIGHT : _gameTimeState.GlobalGameTime.TimeOfDay;

        [Inject] void Install(GameTimeState gameTimeState, DayNightListeners dayNightListeners)
       {
           _gameTimeState = gameTimeState;
           _dayNightListeners = dayNightListeners;
           _gameTimeState.OnGameTimeUpdated.Subscribe(OnTimeChanged).AddTo(this);
       }

        void OnTimeChanged(GameTime gameTime)
        {
            foreach (var dayNightListener in _dayNightListeners.Values)
            {
                dayNightListener?.OnTimeChanged(gameTime);
            }
        }

        private void OnEnable()
        {
            if (_gameTimeState != null) _gameTimeState.Mode = TimeMode.NORMAL;
        }

        private void OnDisable()
        {
            if (_gameTimeState != null) _gameTimeState.Mode = TimeMode.PAUSED;
        }
    }
}