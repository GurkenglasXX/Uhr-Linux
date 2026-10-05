namespace Uhr.Models;

public sealed class PomodoroData
{
    public string Name { get; set; } = "";
    public int WorkMinutes { get; set; }
    public int BreakMinutes { get; set; }
    public int BreakSeconds { get; set; }
    public int Cycles { get; set; }
}
