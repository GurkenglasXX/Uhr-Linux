using Uhr.Models;

namespace Uhr.Services;

public interface IStorageService
{
    AppData Load();
    void Save(AppData data);
}
