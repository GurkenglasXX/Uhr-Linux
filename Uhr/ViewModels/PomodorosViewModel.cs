using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uhr.Models;
using Uhr.Services;

namespace Uhr.ViewModels;

public partial class PomodorosViewModel : ViewModelBase
{
    private readonly AppData _data;
    private readonly IStorageService _storage;
    private readonly INotificationService _notifications;

    public PomodorosViewModel(AppData data, IStorageService storage, INotificationService notifications)
    {
        _data = data;
        _storage = storage;
        _notifications = notifications;

        foreach (var model in data.Pomodoros)
            Pomodoros.Add(Create(model));
    }

    public ObservableCollection<PomodoroViewModel> Pomodoros { get; } = [];

    [ObservableProperty]
    public partial string NewName { get; set; } = "Pomodoro";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    public partial decimal? NewWorkMinutes { get; set; } = 25;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    public partial decimal? NewBreakMinutes { get; set; } = 5;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    public partial decimal? NewCycles { get; set; } = 4;

    public void Tick(DateTime utcNow)
    {
        foreach (var pomodoro in Pomodoros)
            pomodoro.Tick(utcNow);
    }

    private bool CanAdd() => NewWorkMinutes > 0 && NewBreakMinutes > 0 && NewCycles > 0;

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add()
    {
        var model = new PomodoroData
        {
            Name = string.IsNullOrWhiteSpace(NewName) ? "Pomodoro" : NewName.Trim(),
            WorkMinutes = (int)NewWorkMinutes!.Value,
            BreakMinutes = (int)NewBreakMinutes!.Value,
            Cycles = (int)NewCycles!.Value,
        };
        _data.Pomodoros.Add(model);
        Pomodoros.Add(Create(model));
        Save();
    }

    private PomodoroViewModel Create(PomodoroData model) => new(model, _notifications, Delete);

    private void Delete(PomodoroViewModel pomodoro)
    {
        _data.Pomodoros.Remove(pomodoro.Model);
        Pomodoros.Remove(pomodoro);
        Save();
    }

    private void Save() => _storage.Save(_data);
}
