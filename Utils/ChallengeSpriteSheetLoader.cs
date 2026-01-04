using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ConspiracyClicker.Utils;

/// <summary>
/// Loads and extracts individual icons from the challenge/skills sprite sheet.
/// Sprite sheet is 1792x2400 pixels, 3 columns x 4 rows = 597x600 pixels per icon.
/// </summary>
public static class ChallengeSpriteSheetLoader
{
    private static BitmapSource? _spriteSheet;
    private static readonly Dictionary<string, ImageSource> _iconCache = new();

    private const int Columns = 3;
    private const int Rows = 4;
    private const int IconWidth = 597;
    private const int IconHeight = 600;
    private const int IconPadding = 8;

    // Map challenge/skill IDs to sprite positions (row, column) - 0-indexed
    private static readonly Dictionary<string, (int row, int col)> ChallengeSpritePositions = new()
    {
        // Row 0 - Challenge Modes
        ["challenge_speedrun"] = (0, 0),
        ["speedrun"] = (0, 0),
        ["challenge_click_master"] = (0, 0),  // Uses speedrun icon (speed-related)
        ["challenge_lightning"] = (0, 1),
        ["lightning_mode"] = (0, 1),
        ["challenge_observer"] = (0, 2),
        ["observer_only"] = (0, 2),

        // Row 1 - More Challenge Modes
        ["challenge_minimalist"] = (1, 0),
        ["minimalist"] = (1, 0),
        ["challenge_no_prestige"] = (1, 1),
        ["no_prestige"] = (1, 1),
        ["challenge_risky"] = (1, 2),
        ["risky_business"] = (1, 2),

        // Row 2 - Skill Tree Classes
        ["skill_researcher"] = (2, 0),
        ["researcher"] = (2, 0),
        ["skill_influencer"] = (2, 1),
        ["influencer"] = (2, 1),
        ["skill_infiltrator"] = (2, 2),
        ["infiltrator"] = (2, 2),

        // Row 3 - Risk Levels
        ["risk_low"] = (3, 0),
        ["low_risk"] = (3, 0),
        ["risk_medium"] = (3, 1),
        ["medium_risk"] = (3, 1),
        ["risk_high"] = (3, 2),
        ["high_risk"] = (3, 2),
    };

    private static void EnsureSpriteSheetLoaded()
    {
        if (_spriteSheet != null) return;

        try
        {
            var uri = new Uri("pack://application:,,,/Resources/ChallengeSpriteSheet.png", UriKind.Absolute);
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
            System.Diagnostics.Debug.WriteLine($"Failed to load challenge sprite sheet: {ex.Message}");
        }
    }

    public static ImageSource? GetChallengeIcon(string iconId)
    {
        if (_iconCache.TryGetValue(iconId, out var cached))
            return cached;

        if (!ChallengeSpritePositions.TryGetValue(iconId, out var position))
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

    public static bool HasSprite(string iconId) => ChallengeSpritePositions.ContainsKey(iconId);
    public static IEnumerable<string> GetAvailableIconIds() => ChallengeSpritePositions.Keys;
}
