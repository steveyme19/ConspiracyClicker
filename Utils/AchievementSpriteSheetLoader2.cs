using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ConspiracyClicker.Utils;

/// <summary>
/// Loads and extracts individual icons from the second achievement sprite sheet.
/// Sprite sheet is 8 columns x 6 rows for achievement badges.
/// </summary>
public static class AchievementSpriteSheetLoader2
{
    private static BitmapSource? _spriteSheet;
    private static readonly Dictionary<string, ImageSource> _iconCache = new();

    private const int Columns = 8;
    private const int Rows = 6;
    private const int IconPadding = 8;

    // Map achievement IDs to sprite positions (row, column) - 0-indexed
    private static readonly Dictionary<string, (int row, int col)> AchievementSpritePositions = new()
    {
        // Row 0 - Evidence Milestones
        ["evidence_100"] = (0, 0),
        ["evidence_10k"] = (0, 1),
        ["evidence_1m"] = (0, 2),
        ["evidence_1b"] = (0, 3),
        ["evidence_1t"] = (0, 4),
        ["evidence_1q"] = (0, 5),
        ["evidence_1qi"] = (0, 6),
        ["meta_reading"] = (0, 7),

        // Row 1 - Click Achievements
        ["clicks_100"] = (1, 0),
        ["clicks_1000"] = (1, 1),
        ["clicks_10000"] = (1, 2),
        ["clicks_100000"] = (1, 3),
        ["clicks_1m"] = (1, 4),
        ["meta_cookie"] = (1, 5),
        ["crit_10"] = (1, 6),
        ["crit_100"] = (1, 7),

        // Row 2 - Generator Achievements
        ["strings_100"] = (2, 0),
        ["neighbors_50"] = (2, 1),
        ["youtube_10"] = (2, 2),
        ["podcast_5"] = (2, 3),
        ["spy_satellite_1"] = (2, 4),
        ["shadow_gov_1"] = (2, 5),
        ["time_machine_1"] = (2, 6),
        ["researchers_25"] = (2, 7),

        // Row 3 - Conspiracy Progress
        ["conspiracy_1"] = (3, 0),
        ["conspiracy_5"] = (3, 1),
        ["conspiracy_10"] = (3, 2),
        ["conspiracy_15"] = (3, 3),
        ["conspiracy_20"] = (3, 4),
        ["conspiracy_all"] = (3, 5),
        ["quest_1"] = (3, 6),
        ["quest_10"] = (3, 7),

        // Row 4 - Prestige & Meta
        ["ascend_1"] = (4, 0),
        ["ascend_5"] = (4, 1),
        ["ascend_10"] = (4, 2),
        ["ascend_25"] = (4, 3),
        ["matrix_1"] = (4, 4),
        ["matrix_3"] = (4, 5),
        ["matrix_5"] = (4, 6),
        ["meta_developer"] = (4, 7),

        // Row 5 - Resources & Playtime
        ["meta_conspiracy"] = (5, 0),
        ["meta_idle"] = (5, 1),
        ["meta_addiction"] = (5, 2),
        ["meta_time"] = (5, 3),
        ["meta_prestige"] = (5, 4),
        ["playtime_1h"] = (5, 5),
        ["playtime_10h"] = (5, 6),
        ["playtime_100h"] = (5, 7),

        // Additional mappings for remaining achievements (using similar icons)
        ["quest_50"] = (3, 6),
        ["quest_100"] = (3, 7),
        ["quest_500"] = (3, 7),
        ["crit_1000"] = (1, 7),
        ["crit_10000"] = (1, 7),
        ["reality_editor_1"] = (2, 6),
        ["universe_creator_1"] = (0, 4),
    };

    private static void EnsureSpriteSheetLoaded()
    {
        if (_spriteSheet != null) return;

        try
        {
            var uri = new Uri("pack://application:,,,/Resources/AchievementSpriteSheet2.png", UriKind.Absolute);
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
            System.Diagnostics.Debug.WriteLine($"Failed to load achievement sprite sheet 2: {ex.Message}");
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
            int actualIconWidth = _spriteSheet.PixelWidth / Columns;
            int actualIconHeight = _spriteSheet.PixelHeight / Rows;

            int x = position.col * actualIconWidth + IconPadding;
            int y = position.row * actualIconHeight + IconPadding;
            int cropWidth = actualIconWidth - (IconPadding * 2);
            int cropHeight = actualIconHeight - (IconPadding * 2);

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
            System.Diagnostics.Debug.WriteLine($"Failed to extract achievement icon for {achievementId}: {ex.Message}");
            return null;
        }
    }

    public static bool HasSprite(string achievementId) => AchievementSpritePositions.ContainsKey(achievementId);
    public static IEnumerable<string> GetAvailableAchievementIds() => AchievementSpritePositions.Keys;
}
