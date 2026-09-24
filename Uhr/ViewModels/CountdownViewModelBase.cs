using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Uhr.ViewModels;

public abstract partial class CountdownViewModelBase : ViewModelBase
{
    private DateTime _endUtc;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RemainingText), nameof(ProgressAngle))]
    public partial TimeSpan Remaining { get; set; }

    [ObservableProperty]
    public partial bool IsRunning { get; set; }

    protected abstract TimeSpan CurrentDuration { get; }

    public string RemainingText => TimeSpan.FromSeconds(Math.Ceiling(Remaining.TotalSeconds)).ToString(@"hh\:mm\:ss");

    // Ein voller 360°-Bogen hat Start- gleich Endpunkt und würde nicht gezeichnet.
    public double ProgressAngle => CurrentDuration > TimeSpan.Zero
        ? Math.Min(359.99, 360.0 * (Remaining / CurrentDuration))
        : 0;

    public void Tick(DateTime utcNow)
    {
        if (!IsRunning)
            return;

        var remaining = _endUtc - utcNow;
        if (remaining > TimeSpan.Zero)
            Remaining = remaining;
        else
            OnElapsed();
    }

    [RelayCommand]
    private void StartPause()
    {
        if (IsRunning)
        {
            Remaining = _endUtc - DateTime.UtcNow;
            IsRunning = false;
        }
        else
        {
            StartCountdown(Remaining);
        }
    }

    [RelayCommand]
    protected virtual void Reset()
    {
        IsRunning = false;
        Remaining = CurrentDuration;
    }

    protected void StartCountdown(TimeSpan duration)
    {
        Remaining = duration;
        _endUtc = DateTime.UtcNow + duration;
        IsRunning = true;
    }

    protected abstract void OnElapsed();
}
