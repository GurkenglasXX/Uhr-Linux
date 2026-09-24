using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uhr.Models;
using Uhr.Services;

namespace Uhr.ViewModels;

public partial class TimersViewModel : ViewModelBase
{
    private readonly AppData _data;
    private readonly IStorageService _storage;
    private readonly INotificationService _notifications;

    public TimersViewModel(AppData data, IStorageService storage, INotificationService notifications)
    {
        _data = data;
        _storage = storage;
        _notifications = notifications;

        foreach (var model in data.Timers)
            Timers.Add(Create(model));
    }

    public ObservableCollection<TimerViewModel> Timers { get; } = [];

    [ObservableProperty]
    public partial string NewName { get; set; } = "Timer";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    public partial decimal? NewHours { get; set; } = 0;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    public partial decimal? NewMinutes { get; set; } = 5;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    public partial decimal? NewSeconds { get; set; } = 0;

    private int NewDurationSeconds => (int)((NewHours ?? 0) * 3600 + (NewMinutes ?? 0) * 60 + (NewSeconds ?? 0));

    public void Tick(DateTime utcNow)
    {
        foreach (var timer in Timers)
            timer.Tick(utcNow);
    }

    private bool CanAdd() => NewDurationSeconds > 0;

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add()
    {
        var model = new TimerData
        {
            Name = string.IsNullOrWhiteSpace(NewName) ? "Timer" : NewName.Trim(),
            DurationSeconds = NewDurationSeconds,
        };
        _data.Timers.Add(model);
        Timers.Add(Create(model));
        Save();
    }

    private TimerViewModel Create(TimerData model) => new(model, _notifications, Delete);

    private void Delete(TimerViewModel timer)
    {
        _data.Timers.Remove(timer.Model);
        Timers.Remove(timer);
        Save();
    }

    private void Save() => _storage.Save(_data);
}
