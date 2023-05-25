using System;
using System.Collections;
using System.Threading;
using UniRx;
using UnityEngine;
using Zenject;


// /// <summary>
// /// runs the non-physical storm systems.
// /// the entire world's weather systems should be one complete system, the way our own weather system is a full system.
// /// however efficiency wise, simulating the entire world's weather systems while we can only see a tiny part of the world
// /// is a waste of resources.  So instead we have two groupings of weather systems: physical and non-physical.
// /// this allows us to group weather systems into realtime or non-realtime systems.  Non-realtime systems may still be updated
// /// but at a much slower rate.  The main purpose of the non-realtime systems is to:
// ///     (1) allow weather to effect parts of the world so that when the player returns to previous locations,
// ///         they can see how that location has changed due to weather
// ///     (2) allow us to monitor weather systems that are far away from the player, so that the player can choose a route
// ///         based on weather conditions
// ///     (3) allow us to implement weather systems that are interdependent so that they can effect each other
// ///     (4) decouple the rendering and physics logic from the core storm logic
// /// </summary>
public class WeatherRunner : IInitializable, IDisposable


{
    private readonly WeatherConfig _config;
    private readonly CoroutineCaller _coroutineCaller;
    private Coroutine _gameWeatherCoroutine;
    private IDisposable _disposable;
    private Subject<int> _onWeatherUpdate = new Subject<int>();
    private Subject<int> _onWeatherLateUpdate = new Subject<int>();


    public IObservable<int> OnWeatherUpdate => _onWeatherUpdate;
    public IObservable<int> OnWeatherLateUpdate => _onWeatherLateUpdate;
    
    public WeatherRunner(WeatherConfig config, CoroutineCaller coroutineCaller)
    {
        _config = config;
        _coroutineCaller = coroutineCaller;
    }
    
    public void Initialize()
    {
        Debug.Log("WeatherRunner initialized");
       _disposable = Observable.FromCoroutine<int>((observer, token) => RunGameWeather(observer, token)).Subscribe(_onWeatherUpdate);
    }

    IEnumerator RunGameWeather(IObserver<int> weatherUpdate, CancellationToken ct)
    {
        int cnt = 0;
        while (true)
        {
            if(ct.IsCancellationRequested)
            {
                weatherUpdate.OnCompleted();
                yield break;
            }
            weatherUpdate.OnNext(cnt);
            yield return new WaitForSeconds(_config.updateRate);
            _onWeatherLateUpdate.OnNext(cnt);
            cnt++;
        }
    }

    public void Dispose()
    {
        Debug.Log("WeatherRunner disposed");
        if (_gameWeatherCoroutine != null)
            _coroutineCaller.StopCoroutine(_gameWeatherCoroutine);
    }
}