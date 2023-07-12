using System;
using UniRx;

namespace CoreLib.Extensions
{
    public static class UniRxExtensions
    {
        public static IObservable<T> WithCurrent<T>(this ReactiveProperty<T> property) => property.StartWith(property.Value);
        public static IObservable<T> WithCurrent<T>(this ReadOnlyReactiveProperty<T> property) => property.StartWith(property.Value);
        public static IObservable<T> WithCurrent<T>(this IReadOnlyReactiveProperty<T> property) => property.StartWith(property.Value);

        public static IDisposable SubscribeWithCurrent<T>(this IReadOnlyReactiveProperty<T> property, Action<T> onNext)
        {
            return property.StartWith(property.Value).Subscribe(onNext);
        }
        public static IDisposable SubscribeWithCurrent<T>(this IReadOnlyReactiveProperty<T> property, Action<T> onNext, Action<Exception> onError, Action onFinished)
        {
            return property.StartWith(property.Value).Subscribe(onNext, onError, onFinished);
        }
        public static IDisposable SubscribeWithCurrent<T>(this ReadOnlyReactiveProperty<T> property, Action<T> onNext)
        {
            return property.StartWith(property.Value).Subscribe(onNext);
        }
        public static IDisposable SubscribeWithCurrent<T>(this ReadOnlyReactiveProperty<T> property, Action<T> onNext, Action<Exception> onError, Action onFinished)
        {
            return property.StartWith(property.Value).Subscribe(onNext, onError, onFinished);
        }
        public static IDisposable SubscribeWithCurrent<T>(this ReactiveProperty<T> property, Action<T> onNext)
        {
            return property.StartWith(property.Value).Subscribe(onNext);
        }
        public static IDisposable SubscribeWithCurrent<T>(this ReactiveProperty<T> property, Action<T> onNext, Action<Exception> onError, Action onFinished)
        {
            return property.StartWith(property.Value).Subscribe(onNext, onError, onFinished);
        }



        
        /// <summary>
        /// suppresses duplicate values according to the comparer
        /// </summary>
        /// <param name="source"></param>
        /// <param name="comparer"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IObservable<T> DistinctWhere<T>(this IObservable<T> source, Func<T, T, bool> comparer)
        {
            bool emitted = false;
            T lastValue = default;
            
            return source.Where(t =>
            {
                if (emitted == false)
                {
                    emitted = true;
                    lastValue = t;
                    return true;
                }
                var res = comparer(lastValue, t) == false;
                if (res) lastValue = t;
                return res;
            });
        }
    }
}