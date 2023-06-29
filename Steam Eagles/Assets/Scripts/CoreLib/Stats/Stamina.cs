using System;
using UniRx;

public class Stamina : StatBase
{
    public override string GetStatID() => "Stamina";
}

public interface IStatValue
{
    int MaxValue { get; }
    int Value { get; set; }
}

public interface IReactiveStatValue : IStatValue
{
    IObservable<(int prevValue, int newValue)> OnValueChanged { get; }
}

public interface IRegenStatValue : IReactiveStatValue
{
    float RegenRate { get; }
    float RegenResetDelay { get; }
}

public class RegenHandler : IDisposable
{
    private IRegenStatValue _statValue;
    private readonly CoroutineCaller _coroutineCaller;
    
    public int MaxValue => _statValue.MaxValue;
    public int Value
    {
        get => _statValue.Value;
        set => _statValue.Value = value;
    }

    public float RegenerationRate => _statValue.RegenRate;
    public float RegenerationDelay => _statValue.RegenResetDelay;

    private CompositeDisposable disposables = new CompositeDisposable();
    private bool isRegenerationPaused = false;
    private bool isResetTimerRunning = false;

    public RegenHandler(IRegenStatValue statValue)
    {
        _statValue = statValue;

        statValue.OnValueChanged
            .Where(t => t.prevValue < t.newValue)
            .Subscribe(_ => OnStatReduced())
            .AddTo(disposables);
        
        Observable.Interval(TimeSpan.FromSeconds(RegenerationRate))
            .Where(_ => !isRegenerationPaused && Value < MaxValue)
            .Subscribe(_ => Regenerate())
            .AddTo(disposables);
    }

    public void OnStatReduced()
    {
        if (!isRegenerationPaused)
        {
            PauseRegeneration();
            StartResetTimer();
        }
        else
        {
            RestartResetTimer();
        }
    }

    private void PauseRegeneration()
    {
        isRegenerationPaused = true;
        disposables.Clear(); // Clear ongoing regeneration subscriptions
    }

    private void ResumeRegeneration()
    {
        isRegenerationPaused = false;
        disposables.Clear(); // Clear any previous regeneration subscriptions
        Observable.Interval(TimeSpan.FromSeconds(1))
            .Where(_ => !isRegenerationPaused && Value < MaxValue)
            .Subscribe(_ => Regenerate())
            .AddTo(disposables);
    }

    private void StartResetTimer()
    {
        if (!isResetTimerRunning)
        {
            isResetTimerRunning = true;
            Observable.Timer(TimeSpan.FromSeconds(RegenerationDelay))
                .Subscribe(_ =>
                {
                    isResetTimerRunning = false;
                    ResumeRegeneration();
                })
                .AddTo(disposables);
        }
    }

    private void RestartResetTimer()
    {
        disposables.Clear(); // Clear ongoing reset timer subscription
        StartResetTimer();
    }

    private void Regenerate()
    {
        Value += 1;
        if (Value > MaxValue)
            Value = MaxValue;
    }

    public void Dispose()
    {
        disposables?.Dispose();
    }
}