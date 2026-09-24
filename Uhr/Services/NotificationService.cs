using System.ComponentModel;
using System.Diagnostics;

namespace Uhr.Services;

public sealed class NotificationService : INotificationService
{
    private const string SoundFile = "/usr/share/sounds/freedesktop/stereo/alarm-clock-elapsed.oga";

    public void Notify(string title, string message)
    {
        Run("paplay", SoundFile);
        Run("notify-send", "--app-name=Uhr", "--urgency=critical", "--icon=appointment-soon", title, message);
    }

    private static void Run(string fileName, params string[] arguments)
    {
        try
        {
            Process.Start(new ProcessStartInfo(fileName, arguments) { UseShellExecute = false })?.Dispose();
        }
        catch (Win32Exception)
        {
            // Tool nicht installiert – die App soll deswegen nicht abstürzen.
        }
    }
}
