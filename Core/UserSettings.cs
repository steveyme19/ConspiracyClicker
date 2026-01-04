using System.IO;
using System.Text.Json;

namespace ConspiracyClicker.Core;

public class UserSettings
{
    public bool SoundEnabled { get; set; } = true;
    public double SoundVolume { get; set; } = 0.7; // 0.0 to 1.0
    public double WindowWidth { get; set; } = 1400;
    public double WindowHeight { get; set; } = 900;
    public double WindowLeft { get; set; } = -1; // -1 means center
    public double WindowTop { get; set; } = -1;
    public bool WindowMaximized { get; set; } = false;

    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ConspiracyClicker",
        "settings.json"
    );

    public static UserSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                string json = File.ReadAllText(SettingsPath);
                var settings = JsonSerializer.Deserialize<UserSettings>(json);
                if (settings != null)
                    return settings;
            }
        }
        catch
        {
            // If settings fail to load, use defaults
        }

        return new UserSettings();
    }

    public void Save()
    {
        try
        {
            string? directory = Path.GetDirectoryName(SettingsPath);
            if (directory != null)
                Directory.CreateDirectory(directory);

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Silently fail - settings are not critical
        }
    }
}
