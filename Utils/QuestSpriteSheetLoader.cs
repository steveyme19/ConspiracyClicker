using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ConspiracyClicker.Utils;

/// <summary>
/// Loads and extracts individual icons from the quest sprite sheet.
/// Sprite sheet is 1792x2400 pixels, 3 columns x 4 rows = 597x600 pixels per icon.
/// </summary>
public static class QuestSpriteSheetLoader
{
    private static BitmapSource? _spriteSheet;
    private static readonly Dictionary<string, ImageSource> _iconCache = new();

    private const int Columns = 3;
    private const int Rows = 4;
    private const int IconWidth = 597;
    private const int IconHeight = 600;

    // Map quest IDs to sprite positions (row, column) - 0-indexed
    private static readonly Dictionary<string, (int row, int col)> QuestSpritePositions = new()
    {
        // Row 0 - Reconnaissance
        ["recon_mission"] = (0, 0),
        ["document_recovery"] = (0, 1),
        ["signal_intercept"] = (0, 2),
        ["chemtrail_sample"] = (0, 0),
        ["haarp_investigation"] = (0, 2),

        // Row 1 - Infiltration
        ["facility_infiltration"] = (1, 0),
        ["underground_bunker"] = (1, 1),
        ["underground_bunker_survey"] = (1, 1),
        ["underground_network"] = (1, 1),
        ["corporate_mole"] = (1, 2),
        ["corporate_mole_placement"] = (1, 2),
        ["deep_state_mole"] = (1, 2),
        ["crop_circle_decryption"] = (1, 0),
        ["antarctic_expedition"] = (1, 0),

        // Row 2 - Advanced Operations
        ["data_extraction"] = (2, 0),
        ["server_farm_hack"] = (2, 0),
        ["witness_protection"] = (2, 1),
        ["whistleblower_extraction"] = (2, 1),
        ["asset_recruitment"] = (2, 2),
        ["bigfoot_alliance"] = (2, 2),
        ["mind_control_frequency_jam"] = (2, 0),
        ["satellite_hijack"] = (2, 2),

        // Row 3 - Elite Missions
        ["black_site_raid"] = (3, 0),
        ["conspiracy_takedown"] = (3, 1),
        ["shadow_council"] = (3, 1),
        ["truth_revelation"] = (3, 2),
        ["reality_breach"] = (3, 2),
        ["ancient_archive"] = (3, 0),
        ["celebrity_clone_hunt"] = (3, 1),
        ["time_anomaly"] = (3, 2),
        ["hollow_moon"] = (3, 0),
        ["reptilian_gala"] = (3, 1),
        ["alien_embassy"] = (3, 2),
        ["simulation_glitch"] = (3, 2),
        ["multiverse_bridge"] = (3, 2),
        ["akashic_download"] = (3, 0),
        ["god_committee_meeting"] = (3, 1),
        ["entropy_reversal"] = (3, 2),
        ["universe_source_code"] = (3, 0),
        ["cosmic_consciousness_merge"] = (3, 1),
        ["omega_point_expedition"] = (3, 2),
        ["reality_rewrite"] = (3, 2),
    };

    private static void EnsureSpriteSheetLoaded()
    {
        if (_spriteSheet != null) return;

        try
        {
            var uri = new Uri("pack://application:,,,/Resources/QuestSpriteSheet.png", UriKind.Absolute);
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
            System.Diagnostics.Debug.WriteLine($"Failed to load quest sprite sheet: {ex.Message}");
        }
    }

    public static ImageSource? GetQuestIcon(string questId)
    {
        if (_iconCache.TryGetValue(questId, out var cached))
            return cached;

        if (!QuestSpritePositions.TryGetValue(questId, out var position))
            return null;

        EnsureSpriteSheetLoaded();
        if (_spriteSheet == null) return null;

        try
        {
            int x = position.col * IconWidth;
            int y = position.row * IconHeight;

            int width = Math.Min(IconWidth, _spriteSheet.PixelWidth - x);
            int height = Math.Min(IconHeight, _spriteSheet.PixelHeight - y);

            if (width <= 0 || height <= 0) return null;

            var croppedBitmap = new CroppedBitmap(_spriteSheet, new Int32Rect(x, y, width, height));
            croppedBitmap.Freeze();

            _iconCache[questId] = croppedBitmap;
            return croppedBitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to extract icon for {questId}: {ex.Message}");
            return null;
        }
    }

    public static bool HasSprite(string questId) => QuestSpritePositions.ContainsKey(questId);
    public static IEnumerable<string> GetAvailableQuestIds() => QuestSpritePositions.Keys;
}
