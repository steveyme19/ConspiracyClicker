using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ConspiracyClicker.Utils;

/// <summary>
/// Loads and extracts individual icons from the second upgrade sprite sheet.
/// Sprite sheet is 2048x2048 pixels, 6 columns x 7 rows = 341x292 pixels per icon.
/// </summary>
public static class UpgradeSpriteSheetLoader2
{
    private static BitmapSource? _spriteSheet;
    private static readonly Dictionary<string, ImageSource> _iconCache = new();

    private const int Columns = 6;
    private const int Rows = 7;
    private const int IconWidth = 341;
    private const int IconHeight = 292;
    private const int IconPadding = 8;

    // Map upgrade IDs to sprite positions (row, column) - 0-indexed
    private static readonly Dictionary<string, (int row, int col)> UpgradeSpritePositions = new()
    {
        // Row 0 - Basic Click Power Items
        ["sticky_notes"] = (0, 0),
        ["notebook"] = (0, 1),
        ["flashlight"] = (0, 2),
        ["binoculars"] = (0, 3),
        ["scanner"] = (0, 4),
        ["voice_recorder"] = (0, 5),

        // Row 1 - EPS to Click & More Click Power
        ["finger_on_pulse"] = (1, 0),
        ["active_investigation"] = (1, 1),
        ["clickchain_reaction"] = (1, 2),
        ["encrypted_usb"] = (1, 3),
        ["burner_phone"] = (1, 4),
        ["satellite_dish"] = (1, 5),

        // Row 2 - Advanced Click Power & Generator Boosts
        ["black_light"] = (2, 0),
        ["evidence_vault"] = (2, 1),
        ["neural_enhancer"] = (2, 2),
        ["premium_string_2"] = (2, 3),
        ["neighborhood_watch_2"] = (2, 4),
        ["ergonomic_chair_2"] = (2, 5),

        // Row 3 - Generator Boosts Tier 2
        ["seo_optimization"] = (3, 0),
        ["seo_optimization_2"] = (3, 0),
        ["clickbait_thumbnails"] = (3, 1),
        ["clickbait_thumbnails_2"] = (3, 1),
        ["discord_bots"] = (3, 2),
        ["discord_mastery_1"] = (3, 2),
        ["discord_mastery_2"] = (3, 2),
        ["discord_mastery_3"] = (3, 2),
        ["am_radio_tower"] = (3, 3),
        ["radio_mastery_1"] = (3, 3),
        ["radio_mastery_2"] = (3, 3),
        ["radio_mastery_3"] = (3, 3),
        ["podcast_sponsorships"] = (3, 4),
        ["podcast_mastery_1"] = (3, 4),
        ["podcast_mastery_2"] = (3, 4),
        ["podcast_mastery_3"] = (3, 4),
        ["conference_keynotes"] = (3, 5),
        ["conference_mastery_1"] = (3, 5),
        ["conference_mastery_2"] = (3, 5),
        ["conference_mastery_3"] = (3, 5),

        // Row 4 - High Tier Generator Boosts & Global Boosts
        ["netflix_promotion"] = (4, 0),
        ["netflix_mastery_1"] = (4, 0),
        ["netflix_mastery_2"] = (4, 0),
        ["netflix_mastery_3"] = (4, 0),
        ["satellite_network"] = (4, 1),
        ["satellite_mastery_1"] = (4, 1),
        ["satellite_mastery_2"] = (4, 1),
        ["satellite_mastery_3"] = (4, 1),
        ["shadow_government_expansion"] = (4, 2),
        ["shadow_mastery_1"] = (4, 2),
        ["shadow_mastery_2"] = (4, 2),
        ["shadow_mastery_3"] = (4, 2),
        ["collective_consciousness"] = (4, 3),
        ["great_revelation"] = (4, 4),
        ["ascended_knowledge"] = (4, 5),

        // Row 5 - Post-Ascension Generator Boosts
        ["mind_amplifier"] = (5, 0),
        ["weather_dominance"] = (5, 1),
        ["clone_perfection"] = (5, 2),
        ["temporal_mastery"] = (5, 3),
        ["inner_earth_network"] = (5, 4),
        ["cosmic_awareness"] = (5, 5),

        // Row 6 - Ultra Late Game
        ["reality_manipulation"] = (6, 0),
        ["omnipotent_understanding"] = (6, 1),
        ["universe_mastery"] = (6, 2),
        ["truth_serum"] = (6, 3),
        ["reality_distortion"] = (6, 4),
        ["premium_string"] = (6, 5),
        ["neighborhood_watch"] = (6, 5),
        ["ergonomic_chair"] = (6, 5),

        // Generator mastery upgrades - mapped to appropriate visuals
        ["red_string_quantum"] = (2, 3),
        ["red_string_infinite"] = (2, 3),
        ["red_string_mastery_1"] = (2, 3),
        ["red_string_mastery_2"] = (2, 3),
        ["red_string_mastery_3"] = (2, 3),
        ["red_string_mastery_4"] = (2, 3),
        ["neighbor_network"] = (2, 4),
        ["neighbor_hivemind"] = (2, 4),
        ["neighbor_mastery_1"] = (2, 4),
        ["neighbor_mastery_2"] = (2, 4),
        ["neighbor_mastery_3"] = (2, 4),
        ["neighbor_mastery_4"] = (2, 4),
        ["researcher_ascension"] = (2, 5),
        ["researcher_transcendence"] = (2, 5),
        ["researcher_mastery_1"] = (2, 5),
        ["researcher_mastery_2"] = (2, 5),
        ["researcher_mastery_3"] = (2, 5),
        ["researcher_mastery_4"] = (2, 5),
        ["blog_empire"] = (3, 0),
        ["blog_singularity"] = (3, 0),
        ["blog_mastery_1"] = (3, 0),
        ["blog_mastery_2"] = (3, 0),
        ["blog_mastery_3"] = (3, 0),
        ["blog_mastery_4"] = (3, 0),
        ["youtube_algorithm"] = (3, 1),
        ["youtube_monopoly"] = (3, 1),
        ["youtube_mastery_1"] = (3, 1),
        ["youtube_mastery_2"] = (3, 1),
        ["youtube_mastery_3"] = (3, 1),
        ["youtube_mastery_4"] = (3, 1),
    };

    private static void EnsureSpriteSheetLoaded()
    {
        if (_spriteSheet != null) return;

        try
        {
            var uri = new Uri("pack://application:,,,/Resources/UpgradeSpriteSheet2.png", UriKind.Absolute);
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
            System.Diagnostics.Debug.WriteLine($"Failed to load upgrade sprite sheet 2: {ex.Message}");
        }
    }

    public static ImageSource? GetUpgradeIcon(string upgradeId)
    {
        if (_iconCache.TryGetValue(upgradeId, out var cached))
            return cached;

        if (!UpgradeSpritePositions.TryGetValue(upgradeId, out var position))
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

            _iconCache[upgradeId] = croppedBitmap;
            return croppedBitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to extract icon for {upgradeId}: {ex.Message}");
            return null;
        }
    }

    public static bool HasSprite(string upgradeId) => UpgradeSpritePositions.ContainsKey(upgradeId);
    public static IEnumerable<string> GetAvailableUpgradeIds() => UpgradeSpritePositions.Keys;
}
