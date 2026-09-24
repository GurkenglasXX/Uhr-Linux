using System.Collections.Generic;

namespace Uhr.Models;

public sealed class AppData
{
    public List<AlarmData> Alarms { get; set; } = [];
    public List<TimerData> Timers { get; set; } = [];
    public List<PomodoroData> Pomodoros { get; set; } = [];
}
