using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uhr.Models;
using Uhr.Services;

namespace Uhr.ViewModels;

public partial class AlarmsViewModel : ViewModelBase
{
    private readonly AppData _data;
    private readonly IStorageService _storage;
    private readonly INotificationService _notifications;

    public AlarmsViewModel(AppData data, IStorageService storage, INotificationService notifications)
    {
        _data = data;
        _storage = storage;
        _notifications = notifications;

        foreach (var model in data.Alarms.OrderBy(a => a.Time))
            Alarms.Add(Create(model));
    }

    public ObservableCollection<AlarmViewModel> Alarms { get; } = [];

    [ObservableProperty]
    public partial string NewName { get; set; } = "Wecker";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    public partial TimeSpan? NewTime { get; set; } = new TimeSpan(7, 0, 0);

    public void Tick(DateTime now)
    {
        foreach (var alarm in Alarms)
            alarm.Tick(now);
    }

    private bool CanAdd() => NewTime.HasValue;

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add()
    {
        var model = new AlarmData
        {
            Name = string.IsNullOrWhiteSpace(NewName) ? "Wecker" : NewName.Trim(),
            Time = new TimeSpan(NewTime!.Value.Hours, NewTime.Value.Minutes, 0),
        };
        _data.Alarms.Add(model);

        var index = Alarms.TakeWhile(a => a.Model.Time <= model.Time).Count();
        Alarms.Insert(index, Create(model));
        Save();
    }

    private AlarmViewModel Create(AlarmData model) => new(model, _notifications, Save, Delete);

    private void Delete(AlarmViewModel alarm)
    {
        _data.Alarms.Remove(alarm.Model);
        Alarms.Remove(alarm);
        Save();
    }

    private void Save() => _storage.Save(_data);
}
