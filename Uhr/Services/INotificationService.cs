using System;

namespace Uhr.Services;

public interface INotificationService
{
    void Notify(string title, string message);

    void Notify(string title, string message, string actionText, Action onAction);
}
