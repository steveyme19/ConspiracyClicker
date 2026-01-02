using System.IO;
using System.Text.Json;

namespace ConspiracyClicker.Core;

public class SaveSlotInfo
{
    public int Slot { get; init; }
    public bool Exists { get; init; }
    public DateTime LastPlayed { get; init; }
    public double TotalEvidence { get; init; }
    public int AscensionCount { get; init; }
    public double PlayTimeSeconds { get; init; }
    public string? ActiveChallengeId { get; init; }
}

public class SaveManager
{
    private readonly string _saveDirectory;
    private int _currentSlot = 1;

    public int CurrentSlot => _currentSlot;

    public SaveManager()
    {
        _saveDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ConspiracyClicker"
        );

        Directory.CreateDirectory(_saveDirectory);
    }

    private string GetSaveFile(int slot) => Path.Combine(_saveDirectory, $"save{slot}.json");
    private string GetBackupFile(int slot) => Path.Combine(_saveDirectory, $"save{slot}.backup.json");

    public void SetCurrentSlot(int slot)
    {
        if (slot >= 1 && slot <= 3)
            _currentSlot = slot;
    }

    public void Save(GameState state)
    {
        SaveToSlot(_currentSlot, state);
    }

    public void SaveToSlot(int slot, GameState state)
    {
        try
        {
            state.LastSaveTime = DateTime.Now;
            string saveFile = GetSaveFile(slot);
            string backupFile = GetBackupFile(slot);

            if (File.Exists(saveFile))
            {
                File.Copy(saveFile, backupFile, true);
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(state, options);
            File.WriteAllText(saveFile, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Save failed: {ex.Message}");
        }
    }

    public GameState Load()
    {
        return LoadFromSlot(_currentSlot);
    }

    public GameState LoadFromSlot(int slot)
    {
        try
        {
            string saveFile = GetSaveFile(slot);
            if (File.Exists(saveFile))
            {
                string json = File.ReadAllText(saveFile);
                var state = JsonSerializer.Deserialize<GameState>(json);
                if (state != null)
                {
                    _currentSlot = slot;
                    return state;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Load failed: {ex.Message}");
            var backup = TryLoadBackup(slot);
            if (backup != null)
            {
                _currentSlot = slot;
                return backup;
            }
        }

        return new GameState();
    }

    private GameState? TryLoadBackup(int slot)
    {
        try
        {
            string backupFile = GetBackupFile(slot);
            if (File.Exists(backupFile))
            {
                string json = File.ReadAllText(backupFile);
                return JsonSerializer.Deserialize<GameState>(json);
            }
        }
        catch
        {
            // Backup also failed
        }

        return null;
    }

    public bool SlotExists(int slot)
    {
        return File.Exists(GetSaveFile(slot));
    }

    public SaveSlotInfo GetSlotInfo(int slot)
    {
        string saveFile = GetSaveFile(slot);
        if (!File.Exists(saveFile))
        {
            return new SaveSlotInfo { Slot = slot, Exists = false };
        }

        try
        {
            string json = File.ReadAllText(saveFile);
            var state = JsonSerializer.Deserialize<GameState>(json);
            if (state != null)
            {
                return new SaveSlotInfo
                {
                    Slot = slot,
                    Exists = true,
                    LastPlayed = state.LastSaveTime,
                    TotalEvidence = state.TotalEvidenceEarned,
                    AscensionCount = state.TimesAscended,
                    PlayTimeSeconds = state.TotalPlayTimeSeconds,
                    ActiveChallengeId = state.ActiveChallengeId
                };
            }
        }
        catch
        {
            // Corrupted save
        }

        return new SaveSlotInfo { Slot = slot, Exists = false };
    }

    public List<SaveSlotInfo> GetAllSlotInfo()
    {
        var slots = new List<SaveSlotInfo>();
        for (int i = 1; i <= 3; i++)
        {
            slots.Add(GetSlotInfo(i));
        }
        return slots;
    }

    public void DeleteSlot(int slot)
    {
        try
        {
            string saveFile = GetSaveFile(slot);
            string backupFile = GetBackupFile(slot);

            if (File.Exists(saveFile))
                File.Delete(saveFile);
            if (File.Exists(backupFile))
                File.Delete(backupFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Delete failed: {ex.Message}");
        }
    }

    public bool HasAnySave()
    {
        for (int i = 1; i <= 3; i++)
        {
            if (SlotExists(i)) return true;
        }
        return false;
    }

    public int? GetLastUsedSlot()
    {
        SaveSlotInfo? mostRecent = null;
        foreach (var info in GetAllSlotInfo())
        {
            if (info.Exists)
            {
                if (mostRecent == null || info.LastPlayed > mostRecent.LastPlayed)
                {
                    mostRecent = info;
                }
            }
        }
        return mostRecent?.Slot;
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
