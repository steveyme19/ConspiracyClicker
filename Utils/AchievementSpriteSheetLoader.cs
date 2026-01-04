using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ConspiracyClicker.Utils;

/// <summary>
/// Loads and extracts individual icons from the achievement sprite sheet.
/// Sprite sheet is 2048x2048 pixels, 4 columns x 4 rows = 512x512 pixels per icon.
/// </summary>
public static class AchievementSpriteSheetLoader
{
    private static BitmapSource? _spriteSheet;
    private static readonly Dictionary<string, ImageSource> _iconCache = new();

    private const int Columns = 4;
    private const int Rows = 4;
    private const int IconWidth = 512;
    private const int IconHeight = 512;
    private const int IconPadding = 8;

    // Map achievement IDs to sprite positions (row, column) - 0-indexed
    private static readonly Dictionary<string, (int row, int col)> AchievementSpritePositions = new()
    {
        // Row 0 - Progress Achievements
        ["achievement_evidence"] = (0, 0),
        ["evidence_collector"] = (0, 0),
        ["achievement_clicks"] = (0, 1),
        ["click_champion"] = (0, 1),
        ["achievement_generators"] = (0, 2),
        ["generator_master"] = (0, 2),
        ["achievement_conspiracies"] = (0, 3),
        ["conspiracy_theorist"] = (0, 3),

        // Row 1 - Milestone Achievements
        ["achievement_playtime"] = (1, 0),
        ["time_invested"] = (1, 0),
        ["achievement_prestige"] = (1, 1),
        ["prestige_elite"] = (1, 1),
        ["achievement_quests"] = (1, 2),
        ["quest_completer"] = (1, 2),
        ["achievement_meta"] = (1, 3),
        ["meta_gamer"] = (1, 3),

        // Row 2 - Special Achievements
        ["first_blood"] = (2, 0),
        ["first_click"] = (2, 0),
        ["millionaire"] = (2, 1),
        ["wealth_accumulated"] = (2, 1),
        ["speed_runner"] = (2, 2),
        ["fast_completion"] = (2, 2),
        ["perfectionist"] = (2, 3),
        ["hundred_percent"] = (2, 3),

        // Row 3 - Legendary Achievements
        ["ultimate_truth"] = (3, 0),
        ["final_revelation"] = (3, 0),
        ["world_domination"] = (3, 1),
        ["global_mastery"] = (3, 1),
        ["immortal_legacy"] = (3, 2),
        ["eternal_flame"] = (3, 2),
        ["the_awakened"] = (3, 3),
        ["transcendence"] = (3, 3),
    };

    private static void EnsureSpriteSheetLoaded()
    {
        if (_spriteSheet != null) return;

        try
        {
            var uri = new Uri("pack://application:,,,/Resources/AchievementSpriteSheet.png", UriKind.Absolute);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = uri;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            _spriteSheet = bitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load achievement sprite sheet: {ex.Message}");
        }
    }

    public static ImageSource? GetAchievementIcon(string achievementId)
    {
        if (_iconCache.TryGetValue(achievementId, out var cached))
            return cached;

        if (!AchievementSpritePositions.TryGetValue(achievementId, out var position))
            return null;

        EnsureSpriteSheetLoaded();
        if (_spriteSheet == null) return null;

        try
        {
            int x = position.col * IconWidth + IconPadding;
            int y = position.row * IconHeight + IconPadding;
            int cropWidth = IconWidth - (IconPadding * 2);
            int cropHeight = IconHeight - (IconPadding * 2);

            int width = Math.Min(cropWidth, _spriteSheet.PixelWidth - x);
            int height = Math.Min(cropHeight, _spriteSheet.PixelHeight - y);

            if (width <= 0 || height <= 0) return null;

            var croppedBitmap = new CroppedBitmap(_spriteSheet, new Int32Rect(x, y, width, height));
            croppedBitmap.Freeze();

            _iconCache[achievementId] = croppedBitmap;
            return croppedBitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to extract icon for {achievementId}: {ex.Message}");
            return null;
        }
    }

    public static bool HasSprite(string achievementId) => AchievementSpritePositions.ContainsKey(achievementId);
    public static IEnumerable<string> GetAvailableAchievementIds() => AchievementSpritePositions.Keys;
}
