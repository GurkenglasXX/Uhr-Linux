using System;
using System.IO;
using System.Text.Json;
using Uhr.Models;

namespace Uhr.Services;

public sealed class JsonStorageService : IStorageService
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Uhr", "settings.json");

    public AppData Load()
    {
        if (!File.Exists(_path))
            return new AppData();

        using var stream = File.OpenRead(_path);
        return JsonSerializer.Deserialize<AppData>(stream, Options) ?? new AppData();
    }

    public void Save(AppData data)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path, JsonSerializer.Serialize(data, Options));
    }
}
