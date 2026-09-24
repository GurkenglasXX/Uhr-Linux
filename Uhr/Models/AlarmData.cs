using System;

namespace Uhr.Models;

public sealed class AlarmData
{
    public string Name { get; set; } = "";
    public TimeSpan Time { get; set; }
    public bool IsEnabled { get; set; } = true;
}
