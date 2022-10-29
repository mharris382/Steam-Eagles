using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace CoreLib
{
    public static class UDiagnostics
    {
        
        private static Stopwatch sw;

        private static Dictionary<MonoBehaviour, Stopwatch> _stopwatches = new Dictionary<MonoBehaviour, Stopwatch>();
        public static void TimerStart(this MonoBehaviour script)
        {
            if (!_stopwatches.TryAdd(script, Stopwatch.StartNew()))
            {
                _stopwatches[script].Restart();
            }  
        }

        public static void TimerPrintout(this MonoBehaviour script, string message)
        {
            UnityEngine.Debug.Log($"{message} took {_stopwatches[script].ElapsedMilliseconds}");
        }
        
        public static void TimerStop(this MonoBehaviour script, string message)
        {
            if (!_stopwatches.ContainsKey(script))
            {
                _stopwatches[script].Stop();
                UnityEngine.Debug.Log($"{message} took {_stopwatches[script].ElapsedMilliseconds}");
            }  
        }
    }
}