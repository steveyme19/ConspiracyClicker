using System.IO;
using System.Text.Json;

namespace ConspiracyClicker.Core;

public class SaveManager
{
    private readonly string _saveDirectory;
    private readonly string _saveFile;
    private readonly string _backupFile;

    public SaveManager()
    {
        _saveDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ConspiracyClicker"
        );
        _saveFile = Path.Combine(_saveDirectory, "save.json");
        _backupFile = Path.Combine(_saveDirectory, "save.backup.json");

        Directory.CreateDirectory(_saveDirectory);
    }

    public void Save(GameState state)
    {
        try
        {
            state.LastSaveTime = DateTime.Now;

            if (File.Exists(_saveFile))
            {
                File.Copy(_saveFile, _backupFile, true);
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(state, options);
            File.WriteAllText(_saveFile, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Save failed: {ex.Message}");
        }
    }

    public GameState Load()
    {
        try
        {
            if (File.Exists(_saveFile))
            {
                string json = File.ReadAllText(_saveFile);
                var state = JsonSerializer.Deserialize<GameState>(json);
                if (state != null)
                {
                    return state;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Load failed: {ex.Message}");
            TryLoadBackup();
        }

        return new GameState();
    }

    private GameState? TryLoadBackup()
    {
        try
        {
            if (File.Exists(_backupFile))
            {
                string json = File.ReadAllText(_backupFile);
                return JsonSerializer.Deserialize<GameState>(json);
            }
        }
        catch
        {
            // Backup also failed
        }

        return null;
    }

    public (double offlineEvidence, TimeSpan offlineTime) CalculateOfflineProgress(GameState state, double currentEps)
    {
        var offlineTime = DateTime.Now - state.LastSaveTime;
        double offlineSeconds = Math.Min(offlineTime.TotalSeconds, 86400); // Cap at 24 hours

        // 50% offline efficiency (100% if matrix upgrade unlocked)
        double offlineMultiplier = state.MatrixUpgrades.Contains("time_manipulation") ? 1.0 : 0.5;
        double offlineEvidence = currentEps * offlineSeconds * offlineMultiplier;

        return (offlineEvidence, offlineTime);
    }
}
