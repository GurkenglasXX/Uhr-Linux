using System;
using System.Diagnostics;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Uhr.ViewModels;

public partial class StopwatchViewModel : ViewModelBase
{
    private readonly Stopwatch _stopwatch = new();
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(100) };

    public StopwatchViewModel()
    {
        _timer.Tick += (_, _) => Elapsed = _stopwatch.Elapsed;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ElapsedText), nameof(ProgressAngle))]
    public partial TimeSpan Elapsed { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
    public partial bool IsRunning { get; set; }

    public string ElapsedText => $"{(int)Elapsed.TotalHours:00}:{Elapsed:mm\\:ss}";

    // Der Ring zeigt die Sekunden der laufenden Minute.
    public double ProgressAngle => Math.Min(359.99, 6.0 * (Elapsed.TotalSeconds % 60));

    [RelayCommand]
    private void StartPause()
    {
        if (IsRunning)
        {
            _stopwatch.Stop();
            _timer.Stop();
            Elapsed = _stopwatch.Elapsed;
            IsRunning = false;
        }
        else
        {
            _stopwatch.Start();
            _timer.Start();
            IsRunning = true;
        }
    }

    private bool CanReset() => IsRunning || Elapsed > TimeSpan.Zero;

    [RelayCommand(CanExecute = nameof(CanReset))]
    private void Reset()
    {
        _stopwatch.Reset();
        _timer.Stop();
        Elapsed = TimeSpan.Zero;
        IsRunning = false;
        ResetCommand.NotifyCanExecuteChanged();
    }
}
