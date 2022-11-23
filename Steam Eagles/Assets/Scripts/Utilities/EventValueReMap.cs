using System;
using UnityEngine;
using UnityEngine.Events;

namespace Utilities
{
    [CreateAssetMenu(fileName = "EventValueReMap", menuName = "EventValueReMap", order = 0)]
    public class EventValueReMap : ScriptableObject
    {
        [Header("Float Values")]
        public Vector2 fInputRange;
        public Vector2 fOutputRange;
        
        [Header("Int Values")]
        public Vector2Int iInputRange;
        public Vector2Int iOutputRange;
        
        
        public float RemapFloat(float value)
        {
            return Mathf.Lerp(fOutputRange.x, fOutputRange.y, Mathf.InverseLerp(fInputRange.x, fInputRange.y, value));
        }
        public float RemapIntToFloat(int value)
        {
            return Mathf.Lerp(fOutputRange.x, fOutputRange.y, Mathf.InverseLerp(iInputRange.x, iInputRange.y, value));
        }
        
        public int RemapFloatToInt(float value)
        {
            return Mathf.RoundToInt(Mathf.Lerp(iOutputRange.x, iOutputRange.y, Mathf.InverseLerp(fInputRange.x, fInputRange.y, value)));
        }

        public int RemapInt(int value)
        {
            return Mathf.RoundToInt(Mathf.Lerp(iOutputRange.x, iOutputRange.y, Mathf.InverseLerp(iInputRange.x, iInputRange.y, value)));
        }
    }

    public static class RemapExtensions
    {
        public static void RemapEvent_Float(this UnityEvent<float> inputEvent, Action<float> outputAction, EventValueReMap remap)
        {
            UnityAction<float> del = (value) => outputAction(remap.RemapFloat(value));
            inputEvent.AddListener(del);
        }
        
        public static void RemapEvent_IntToFloat(this UnityEvent<int> inputEvent, Action<float> outputAction, EventValueReMap remap)
        {
            UnityAction<int> del = (value) => outputAction(remap.RemapIntToFloat(value));
            inputEvent.AddListener(del);
        }
        
        public static void RemapEvent_FloatToInt(this UnityEvent<float> inputEvent, Action<int> outputAction, EventValueReMap remap)
        {
            UnityAction<float> del = (value) => outputAction(remap.RemapFloatToInt(value));
            inputEvent.AddListener(del);
        }

        public static IDisposable RemapEvent_Int(this UnityEvent<int> inputEvent, Action<int> outputAction, EventValueReMap remap)
        {
            UnityAction<int> del = (value) => outputAction(remap.RemapInt(value));
            inputEvent.AddListener(del);
            return new DisposalAction(() => inputEvent.RemoveListener(del));
        }

        struct DisposalAction : IDisposable
        {
            private readonly Action _onDispose;
            private bool disposed;
            public DisposalAction(Action onDispose)
            {
                disposed= false;
                _onDispose = onDispose;
            }
            public void Dispose()
            {
                if (disposed) return;
                _onDispose();
                disposed = true;
            }
        }
    }
}