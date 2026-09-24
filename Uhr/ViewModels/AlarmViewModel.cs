using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uhr.Models;
using Uhr.Services;

namespace Uhr.ViewModels;

public partial class AlarmViewModel : ViewModelBase
{
    private readonly INotificationService _notifications;
    private readonly Action _changed;
    private readonly Action<AlarmViewModel> _delete;
    private DateTime _nextRing;

    public AlarmViewModel(AlarmData model, INotificationService notifications, Action changed, Action<AlarmViewModel> delete)
    {
        Model = model;
        _notifications = notifications;
        _changed = changed;
        _delete = delete;
        ScheduleNext(DateTime.Now);
        UpdateNextRingText(DateTime.Now);
    }

    public AlarmData Model { get; }

    public string Name => Model.Name;

    public string TimeText => Model.Time.ToString(@"hh\:mm");

    public bool IsEnabled
    {
        get => Model.IsEnabled;
        set
        {
            if (!SetProperty(Model.IsEnabled, value, Model, (m, v) => m.IsEnabled = v))
                return;

            var now = DateTime.Now;
            ScheduleNext(now);
            UpdateNextRingText(now);
            _changed();
        }
    }

    [ObservableProperty]
    public partial string NextRingText { get; set; } = "";

    public void Tick(DateTime now)
    {
        if (IsEnabled && now >= _nextRing)
        {
            // Wie bei Windows: ein Wecker ohne Wiederholung schaltet sich nach dem Klingeln ab.
            IsEnabled = false;
            _notifications.Notify("Wecker", $"{Name} – {TimeText} Uhr");
        }

        UpdateNextRingText(now);
    }

    [RelayCommand]
    private void Delete() => _delete(this);

    private void ScheduleNext(DateTime now)
    {
        var today = now.Date + Model.Time;
        _nextRing = today > now ? today : today.AddDays(1);
    }

    private void UpdateNextRingText(DateTime now)
    {
        if (!IsEnabled)
        {
            NextRingText = "Aus";
            return;
        }

        var minutes = (int)Math.Ceiling((_nextRing - now).TotalMinutes);
        NextRingText = minutes >= 60
            ? $"Klingelt in {minutes / 60} Std. {minutes % 60} Min."
            : $"Klingelt in {minutes} Min.";
    }
}
