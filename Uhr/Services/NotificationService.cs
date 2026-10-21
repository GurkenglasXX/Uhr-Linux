using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform;
using Avalonia.Threading;
using Tmds.DBus.Protocol;

namespace Uhr.Services;

// Spricht den Benachrichtigungsdienst direkt per D-Bus an, weil notify-send 0.8.8 unter KDE Plasma keine Buttons anzeigt.
public sealed class NotificationService : INotificationService
{
    private const string Service = "org.freedesktop.Notifications";
    private const string ObjectPath = "/org/freedesktop/Notifications";
    private const string ActionKey = "action";
    private const byte UrgencyCritical = 2;

    private static readonly Lazy<string?> SoundFile = new(ExtractSound);

    private readonly ConcurrentDictionary<uint, Action> _pendingActions = new();
    private Task? _subscription;

    public void Notify(string title, string message)
    {
        PlaySound();
        _ = SendAsync(title, message, null, null);
    }

    public void Notify(string title, string message, string actionText, Action onAction)
    {
        PlaySound();
        _ = SendAsync(title, message, actionText, onAction);
    }

    private async Task SendAsync(string title, string message, string? actionText, Action? onAction)
    {
        try
        {
            var connection = DBusConnection.Session;
            if (onAction is not null)
                await (_subscription ??= SubscribeAsync(connection));

            var id = await connection.CallMethodAsync(CreateNotifyMessage(connection, title, message, actionText),
                static (Message m, object? _) => m.GetBodyReader().ReadUInt32(), null);

            if (onAction is not null)
                _pendingActions[id] = onAction;
        }
        catch (Exception)
        {
            // Kein Benachrichtigungsdienst erreichbar – die App soll deswegen nicht abstürzen.
        }
    }

    private static MessageBuffer CreateNotifyMessage(DBusConnection connection, string title, string message, string? actionText)
    {
        using var writer = connection.GetMessageWriter();
        writer.WriteMethodCallHeader(Service, ObjectPath, Service, "Notify", "susssasa{sv}i", MessageFlags.None);
        writer.WriteString("Uhr");
        writer.WriteUInt32(0);
        writer.WriteString("appointment-soon");
        writer.WriteString(title);
        writer.WriteString(message);
        writer.WriteArray(actionText is null ? [] : new[] { ActionKey, actionText });
        var hints = writer.WriteDictionaryStart();
        writer.WriteDictionaryEntryStart();
        writer.WriteString("urgency");
        writer.WriteVariantByte(UrgencyCritical);
        writer.WriteDictionaryEnd(hints);
        writer.WriteInt32(-1);
        return writer.CreateMessage();
    }

    private async Task SubscribeAsync(DBusConnection connection)
    {
        await connection.AddMatchAsync(Signal("ActionInvoked"), ReadActionInvoked,
            (Action<Notification<(uint Id, string Key)>>)OnActionInvoked, emitOnCapturedContext: false, ObserverFlags.None);
        await connection.AddMatchAsync(Signal("NotificationClosed"), ReadNotificationClosed,
            (Action<Notification<uint>>)OnNotificationClosed, emitOnCapturedContext: false, ObserverFlags.None);
    }

    private static (uint Id, string Key) ReadActionInvoked(Message message, object? state)
    {
        var reader = message.GetBodyReader();
        return (reader.ReadUInt32(), reader.ReadString());
    }

    private static uint ReadNotificationClosed(Message message, object? state) => message.GetBodyReader().ReadUInt32();

    private void OnActionInvoked(Notification<(uint Id, string Key)> notification)
    {
        if (notification.HasValue && notification.Value.Key == ActionKey
            && _pendingActions.TryRemove(notification.Value.Id, out var action))
            Dispatcher.UIThread.Post(action);
    }

    private void OnNotificationClosed(Notification<uint> notification)
    {
        if (notification.HasValue)
            _pendingActions.TryRemove(notification.Value, out _);
    }

    private static MatchRule Signal(string member) => new()
    {
        Type = MessageType.Signal,
        Interface = Service,
        Path = ObjectPath,
        Member = member,
    };

    private static void PlaySound()
    {
        if (SoundFile.Value is { } file)
            Run("paplay", file);
    }

    // paplay braucht eine echte Datei, die App liegt aber als einzelne Programmdatei vor.
    private static string? ExtractSound()
    {
        try
        {
            var cache = Environment.GetEnvironmentVariable("XDG_CACHE_HOME") is { Length: > 0 } xdg
                ? xdg
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache");
            var file = Path.Combine(cache, "Uhr", "alarm.oga");
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            using var source = AssetLoader.Open(new Uri("avares://Uhr/Assets/alarm.oga"));
            using var target = File.Create(file);
            source.CopyTo(target);
            return file;
        }
        catch (Exception)
        {
            return null;
        }
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
