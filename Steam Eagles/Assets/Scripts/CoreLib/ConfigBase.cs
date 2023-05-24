using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CoreLib
{
    public abstract class ConfigBase
    {

        private enum LogLevel { NONE, VERBOSE, TESTING }
        

        [SerializeField, PropertyOrder(1000000), EnumToggleButtons]
        private LogLevel logLevel;
        public void Log(string msg, bool isTestLog = false)
        {
            if(isTestLog && logLevel != LogLevel.TESTING || logLevel == LogLevel.NONE)
                return;
            Debug.Log(msg);
        }

        public void Log(Func<string> msg, bool isTestLog = false)
        {
            if(isTestLog && logLevel != LogLevel.TESTING || logLevel == LogLevel.NONE)
                return;
            Debug.Log(msg());
        }
    }
}