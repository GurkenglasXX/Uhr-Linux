using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uhr.Models;
using Uhr.Services;

namespace Uhr.ViewModels;

public partial class PomodoroViewModel : CountdownViewModelBase
{
    private const int PostponeMinutes = 3;

    private readonly INotificationService _notifications;
    private readonly Action<PomodoroViewModel> _delete;

    public PomodoroViewModel(PomodoroData model, INotificationService notifications, Action<PomodoroViewModel> delete)
    {
        Model = model;
        _notifications = notifications;
        _delete = delete;
        Remaining = CurrentDuration;
    }

    public PomodoroData Model { get; }

    public string Name => Model.Name;

    public string SettingsText => $"{Model.WorkMinutes} Min. Fokus\n{BreakText} Pause\n{Model.Cycles} {(Model.Cycles == 1 ? "Zyklus" : "Zyklen")}";

    private string BreakText => Model.BreakSeconds == 0
        ? $"{Model.BreakMinutes} Min."
        : $"{Model.BreakMinutes}:{Model.BreakSeconds:00} Min.";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PhaseText), nameof(ProgressAngle))]
    public partial bool IsBreak { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PhaseText))]
    public partial int CurrentCycle { get; set; } = 1;

    public string PhaseText => $"{(IsBreak ? "Pause" : "Fokus")} · Zyklus {CurrentCycle} von {Model.Cycles}";

    protected override TimeSpan CurrentDuration =>
        IsBreak
            ? new TimeSpan(0, Model.BreakMinutes, Model.BreakSeconds)
            : TimeSpan.FromMinutes(Model.WorkMinutes);

    protected override void OnElapsed()
    {
        if (!IsBreak)
        {
            IsBreak = true;
            StartCountdown(CurrentDuration);
            var cycle = CurrentCycle;
            _notifications.Notify("Fokuszeit vorbei", $"{Name}: {BreakText} Pause (Zyklus {CurrentCycle} von {Model.Cycles})",
                $"Pause um {PostponeMinutes} Min. verschieben", () => PostponeBreak(cycle));
        }
        else if (CurrentCycle < Model.Cycles)
        {
            IsBreak = false;
            CurrentCycle++;
            StartCountdown(CurrentDuration);
            _notifications.Notify("Pause vorbei", $"{Name}: weiter mit Zyklus {CurrentCycle} von {Model.Cycles}");
        }
        else
        {
            Reset();
            _notifications.Notify("Pomodoro abgeschlossen", $"{Name}: alle {Model.Cycles} Zyklen geschafft");
        }
    }

    // Nur wirksam, solange noch die Pause dieses Zyklus läuft – nach Reset oder Phasenwechsel wird der Klick ignoriert.
    private void PostponeBreak(int cycle)
    {
        if (!IsBreak || CurrentCycle != cycle)
            return;

        IsBreak = false;
        StartCountdown(TimeSpan.FromMinutes(PostponeMinutes));
    }

    protected override void Reset()
    {
        IsBreak = false;
        CurrentCycle = 1;
        base.Reset();
    }

    [RelayCommand]
    private void Delete() => _delete(this);
}
