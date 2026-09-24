using System;
using CommunityToolkit.Mvvm.Input;
using Uhr.Models;
using Uhr.Services;

namespace Uhr.ViewModels;

public partial class TimerViewModel : CountdownViewModelBase
{
    private readonly INotificationService _notifications;
    private readonly Action<TimerViewModel> _delete;

    public TimerViewModel(TimerData model, INotificationService notifications, Action<TimerViewModel> delete)
    {
        Model = model;
        _notifications = notifications;
        _delete = delete;
        Remaining = CurrentDuration;
    }

    public TimerData Model { get; }

    public string Name => Model.Name;

    protected override TimeSpan CurrentDuration => TimeSpan.FromSeconds(Model.DurationSeconds);

    protected override void OnElapsed()
    {
        Reset();
        _notifications.Notify("Timer abgelaufen", $"{Name} ({CurrentDuration:hh\\:mm\\:ss})");
    }

    [RelayCommand]
    private void Delete() => _delete(this);
}
