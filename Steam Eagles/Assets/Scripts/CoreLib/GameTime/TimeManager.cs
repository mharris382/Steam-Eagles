using System;
using System.Collections;
using System.IO;
using CoreLib.SaveLoad;
using SaveLoad;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CoreLib.GameTime
{
    public class TimeManager : Singleton<TimeManager>
    {
        public override bool DestroyOnLoad => true;
        private GameDateTime _currentDateTime;
        
        public bool isGameTimePaused;

        /// <summary>
        /// used for anything that needs to be updated continuous in real time, and corresponds to game time.
        /// For example sunlight intensity, or the position of the sun
        /// </summary>
        private float _analogTime;
        /// <summary>
        /// invoked when game time is incremented
        /// </summary>
        public event Action<GameTime> GameTimeIncremented;
        /// <summary>
        /// invoked continuously when game time is not paused
        /// </summary>
        public event Action<float> PercentOfElapsedDayUpdated;
        /// <summary>
        /// invoked when game day is incremented
        /// </summary>
        public event Action<GameDate> GameDateChanged;
        
        /// <summary>
        /// invoked when game time or date is incremented
        /// </summary>
        public static event Action<GameDateTime> GameDateTimeChanged;

        

        public GameTime CurrentGameTime => _currentDateTime.gameTime;

        public GameDate CurrentGameDate => _currentDateTime.gameDate;

        protected override void Init()
        {
            PersistenceManager.Instance.GameSaved += SaveGameTime;
            PersistenceManager.Instance.GameLoaded += LoadGameTime;
        }

        private IEnumerator Start()
        {
            while (true)
            {
                if (isGameTimePaused)
                {
                    yield return null;    
                }
                else
                {
                    var increment = new GameTime(0, GameTimeConfig.Instance.MinutesPerDisplayIncrement);
                    var timespan = increment.GetRealTimeSpan();
                    var percentOfDayPassed = _currentDateTime.gameTime.GetPercentageOfDay();
                    for (float t = 0; t < 1; t+= (Time.deltaTime/timespan.Seconds))
                    {
                        float analogTime = t * GameTimeConfig.Instance.IncrementAsPercentageOfTheDay;
                        PercentOfElapsedDayUpdated?.Invoke(percentOfDayPassed + analogTime);
                        yield return null;
                    }
                    
                    GameTimeIncremented?.Invoke(increment);
                    
                    var nextGameDateTime = _currentDateTime + increment;
                    if (nextGameDateTime.gameDate != _currentDateTime.gameDate)
                    {
                        GameDateChanged?.Invoke(nextGameDateTime.gameDate);
                    }
                    
                    _currentDateTime = nextGameDateTime;
                    GameTimeIncremented?.Invoke(increment);
                }
            }
        }


        void SaveGameTime(string savePath)
        {
            var dateTime = JsonUtility.ToJson(_currentDateTime);
            var filePath = $"{savePath}/GameDateTime.json";
            Debug.Log($"Saving game time to {filePath}",this);
            File.WriteAllText(filePath, dateTime);
        }
        void LoadGameTime(string savePath)
        {
            var filePath = $"{savePath}/GameDateTime.json";
            if (!File.Exists(filePath))
            {
                _currentDateTime = GameDateTime.GameStart;
                SaveGameTime(savePath);
            }
            Debug.Log($"Loading game time from {filePath}",this);
            var dateTime = JsonUtility.FromJson<GameDateTime>(File.ReadAllText(filePath));
            _currentDateTime = dateTime;
        }


        public static GameDateTime GetTimeSavedAtPath(string path)
        {
            var dateTime = GameDateTime.GameStart;
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException(path);
            }
            var filePath = $"{path}/GameDateTime.json";
            if (File.Exists(filePath))
            {
                dateTime = JsonUtility.FromJson<GameDateTime>(File.ReadAllText(filePath));
            }
            else
            {
                File.WriteAllText(filePath, JsonUtility.ToJson(dateTime));
            }
            return dateTime;
        }

        public void JumpToDateTime(GameDateTime gameDateTime)
        {
            var prevDateTime = _currentDateTime;
            _currentDateTime = gameDateTime;
            GameDateTimeChanged?.Invoke(gameDateTime);
            if(prevDateTime.gameDate != gameDateTime.gameDate)
                GameDateChanged?.Invoke(gameDateTime.gameDate);
            GameTimeIncremented?.Invoke(gameDateTime.gameTime);
        }

        
        [SerializeField] private UpdateFromEditor updateFromEditor;

        [System.Serializable]
        class UpdateFromEditor
        {
            public GameTime time;
            public GameDate date;
            
            [ButtonGroup()]
            [DisableInEditorMode]
            [Button("Update No Save")]
            public void Update()
            {
                TimeManager.Instance.JumpToDateTime(new GameDateTime(date, time));
            }
            [ButtonGroup()]
            [DisableInEditorMode]
            [Button, EnableIf(nameof(HasValidSavePath))]
            public void UpdateWithSave()
            {
                Update();
                MessageBroker.Default.Publish(new SaveGameRequestedInfo(PersistenceManager.SavePath));
            }
            
            bool HasValidSavePath => !string.IsNullOrEmpty(PersistenceManager.SavePath);
        }
    }
}