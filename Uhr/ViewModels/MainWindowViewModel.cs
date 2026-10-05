using System;
using Avalonia.Threading;
using Uhr.Services;

namespace Uhr.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(IStorageService storage, INotificationService notifications)
    {
        var data = storage.Load();
        Alarms = new AlarmsViewModel(data, storage, notifications);
        Timers = new TimersViewModel(data, storage, notifications);
        Pomodoros = new PomodorosViewModel(data, storage, notifications);
        Stopwatch = new StopwatchViewModel();

        DispatcherTimer.Run(() =>
        {
            Tick();
            return true;
        }, TimeSpan.FromMilliseconds(200));
    }

    public AlarmsViewModel Alarms { get; }
    public TimersViewModel Timers { get; }
    public PomodorosViewModel Pomodoros { get; }
    public StopwatchViewModel Stopwatch { get; }

    private void Tick()
    {
        Alarms.Tick(DateTime.Now);
        var utcNow = DateTime.UtcNow;
        Timers.Tick(utcNow);
        Pomodoros.Tick(utcNow);
    }
}
