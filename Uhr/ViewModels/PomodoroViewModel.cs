using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uhr.Models;
using Uhr.Services;

namespace Uhr.ViewModels;

public partial class PomodoroViewModel : CountdownViewModelBase
{
    private const int PostponeMinutes = 3;
    private const double MinDeviation = 0.05;
    private const double MaxDeviation = 0.20;

    private readonly INotificationService _notifications;
    private readonly Action<PomodoroViewModel> _delete;
    private TimeSpan _focusDuration;

    public PomodoroViewModel(PomodoroData model, INotificationService notifications, Action<PomodoroViewModel> delete)
    {
        Model = model;
        _notifications = notifications;
        _delete = delete;
        _focusDuration = NextFocusDuration();
        Remaining = CurrentDuration;
    }

    public PomodoroData Model { get; }

    public string Name => Model.Name;

    public string SettingsText => $"{FocusText} Fokus\n{BreakText} Pause\n{Model.Cycles} {(Model.Cycles == 1 ? "Zyklus" : "Zyklen")}";

    public string FocusText => Model.RandomFocus
        ? $"ca. {Model.WorkMinutes} Min."
        : $"{Model.WorkMinutes} Min.";

    private string BreakText => Model.BreakSeconds == 0
        ? $"{Model.BreakMinutes} Min."
        : $"{Model.BreakMinutes}:{Model.BreakSeconds:00} Min.";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PhaseText), nameof(ProgressAngle), nameof(HidesRemaining))]
    public partial bool IsBreak { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PhaseText))]
    public partial int CurrentCycle { get; set; } = 1;

    public string PhaseText => $"{(IsBreak ? "Pause" : "Fokus")} · Zyklus {CurrentCycle} von {Model.Cycles}";

    // Die verschobenen 3 Minuten sind fest und laufen deshalb sichtbar ab.
    public bool HidesRemaining => Model.RandomFocus && !IsBreak && !IsPostponed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HidesRemaining))]
    private partial bool IsPostponed { get; set; }

    protected override TimeSpan CurrentDuration =>
        IsBreak
            ? new TimeSpan(0, Model.BreakMinutes, Model.BreakSeconds)
            : _focusDuration;

    // Weicht zufällig um 5 bis 20 % nach oben oder unten ab, damit man die Restzeit nicht abschätzen kann.
    private TimeSpan NextFocusDuration()
    {
        var focus = TimeSpan.FromMinutes(Model.WorkMinutes);
        if (!Model.RandomFocus)
            return focus;

        var deviation = MinDeviation + Random.Shared.NextDouble() * (MaxDeviation - MinDeviation);
        return focus * (Random.Shared.Next(2) == 0 ? 1 - deviation : 1 + deviation);
    }

    protected override void OnElapsed()
    {
        if (!IsBreak)
        {
            IsBreak = true;
            IsPostponed = false;
            StartCountdown(CurrentDuration);
            var cycle = CurrentCycle;
            _notifications.Notify("Fokuszeit vorbei", $"{Name}: {BreakText} Pause (Zyklus {CurrentCycle} von {Model.Cycles})",
                $"Pause um {PostponeMinutes} Min. verschieben", () => PostponeBreak(cycle));
        }
        else if (CurrentCycle < Model.Cycles)
        {
            IsBreak = false;
            CurrentCycle++;
            _focusDuration = NextFocusDuration();
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
        IsPostponed = true;
        StartCountdown(TimeSpan.FromMinutes(PostponeMinutes));
    }

    protected override void Reset()
    {
        IsBreak = false;
        CurrentCycle = 1;
        IsPostponed = false;
        _focusDuration = NextFocusDuration();
        base.Reset();
    }

    [RelayCommand]
    private void Delete() => _delete(this);
}
