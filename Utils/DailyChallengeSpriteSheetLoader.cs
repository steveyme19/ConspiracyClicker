using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ConspiracyClicker.Utils;

/// <summary>
/// Loads and extracts individual icons from the daily challenge sprite sheet.
/// Sprite sheet is 2528x1696 pixels, 3 columns x 2 rows = 843x848 pixels per icon.
/// </summary>
public static class DailyChallengeSpriteSheetLoader
{
    private static BitmapSource? _spriteSheet;
    private static readonly Dictionary<string, ImageSource> _iconCache = new();

    private const int Columns = 3;
    private const int Rows = 2;
    private const int IconWidth = 843;
    private const int IconHeight = 848;
    private const int IconPadding = 10;

    // Map daily challenge IDs to sprite positions (row, column) - 0-indexed
    private static readonly Dictionary<string, (int row, int col)> DailyChallengeSpritePositions = new()
    {
        // Row 0 - Click-based Challenges
        ["challenge_clicks"] = (0, 0),
        ["click_challenge"] = (0, 0),
        ["daily_clicks"] = (0, 0),
        ["challenge_crits"] = (0, 1),
        ["critical_challenge"] = (0, 1),
        ["daily_crits"] = (0, 1),
        ["challenge_combos"] = (0, 2),
        ["combo_challenge"] = (0, 2),
        ["daily_combos"] = (0, 2),

        // Row 1 - Task-based Challenges
        ["challenge_quests"] = (1, 0),
        ["quest_challenge"] = (1, 0),
        ["daily_quests"] = (1, 0),
        ["challenge_evidence"] = (1, 1),
        ["evidence_challenge"] = (1, 1),
        ["daily_evidence"] = (1, 1),
        ["challenge_time"] = (1, 2),
        ["time_challenge"] = (1, 2),
        ["daily_time"] = (1, 2),
    };

    private static void EnsureSpriteSheetLoaded()
    {
        if (_spriteSheet != null) return;

        try
        {
            var uri = new Uri("pack://application:,,,/Resources/DailyChallengeSpriteSheet.png", UriKind.Absolute);
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
            System.Diagnostics.Debug.WriteLine($"Failed to load daily challenge sprite sheet: {ex.Message}");
        }
    }

    public static ImageSource? GetDailyChallengeIcon(string iconId)
    {
        if (_iconCache.TryGetValue(iconId, out var cached))
            return cached;

        if (!DailyChallengeSpritePositions.TryGetValue(iconId, out var position))
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

            _iconCache[iconId] = croppedBitmap;
            return croppedBitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to extract icon for {iconId}: {ex.Message}");
            return null;
        }
    }

    public static bool HasSprite(string iconId) => DailyChallengeSpritePositions.ContainsKey(iconId);
    public static IEnumerable<string> GetAvailableIconIds() => DailyChallengeSpritePositions.Keys;
}
