using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ConspiracyClicker.Utils;

/// <summary>
/// Loads and extracts individual icons from the Illuminati/prestige upgrade sprite sheet.
/// Sprite sheet is 1920x2240 pixels, 6 columns x 7 rows = 320x320 pixels per icon.
/// </summary>
public static class IlluminatiSpriteSheetLoader
{
    private static BitmapSource? _spriteSheet;
    private static readonly Dictionary<string, ImageSource> _iconCache = new();

    private const int Columns = 6;
    private const int Rows = 7;
    private const int IconWidth = 320;
    private const int IconHeight = 320;
    private const int IconPadding = 8;

    // Map Illuminati upgrade IDs to sprite positions (row, column) - 0-indexed
    private static readonly Dictionary<string, (int row, int col)> IlluminatiSpritePositions = new()
    {
        // Row 0 - Tier 1 Basic Powers
        ["pyramid_scheme"] = (0, 0),
        ["reptilian_dna"] = (0, 1),
        ["secret_handshake"] = (0, 2),
        ["new_world_order_discount"] = (0, 3),
        ["deep_state_connections"] = (0, 4),
        ["ancient_knowledge"] = (0, 5),

        // Row 1 - Tier 2 Advanced Powers
        ["auto_clicker"] = (1, 0),
        ["moon_base_alpha"] = (1, 1),
        ["time_manipulation"] = (1, 2),
        ["golden_eye_magnetism"] = (1, 3),
        ["believer_magnetism"] = (1, 4),
        ["mind_control_mastery"] = (1, 5),

        // Row 2 - Tier 3 Elite Powers
        ["all_seeing_investment"] = (2, 0),
        ["infinite_tinfoil"] = (2, 1),
        ["third_eye_awakening"] = (2, 2),
        ["instant_indoctrination"] = (2, 3),
        ["shadow_network"] = (2, 4),
        ["parallel_universe_access"] = (2, 5),

        // Row 3 - Tier 4 Master Powers
        ["reality_distortion"] = (3, 0),
        ["cosmic_alignment"] = (3, 1),
        ["conspiracy_cascade"] = (3, 2),
        ["global_awakening"] = (3, 3),
        ["temporal_fold"] = (3, 4),
        ["whistle_blower_network"] = (3, 5),

        // Row 4 - Tier 5 Legendary Powers
        ["illuminati_council_seat"] = (4, 0),
        ["time_dilation_field"] = (4, 1),
        ["omniscient_vision"] = (4, 2),
        ["eternal_conspiracy"] = (4, 3),
        ["reality_overwrite"] = (4, 4),
        ["entropy_mastery"] = (4, 5),

        // Row 5 - Tier 6 Transcendent Powers
        ["probability_control"] = (5, 0),
        ["tinfoil_transmutation"] = (5, 1),
        ["believer_singularity"] = (5, 2),
        ["click_transcendence"] = (5, 3),
        ["evidence_singularity"] = (5, 4),
        ["temporal_loop"] = (5, 5),

        // Row 6 - Tier 7 Omega Powers
        ["omnipresent_network"] = (6, 0),
        ["cosmic_tinfoil"] = (6, 1),
        ["final_truth"] = (6, 2),
        ["ascension_complete"] = (6, 3),
        ["universe_control"] = (6, 4),
        ["omega_symbol"] = (6, 5),
    };

    private static void EnsureSpriteSheetLoaded()
    {
        if (_spriteSheet != null) return;

        try
        {
            var uri = new Uri("pack://application:,,,/Resources/IlluminatiSpriteSheet.png", UriKind.Absolute);
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
            System.Diagnostics.Debug.WriteLine($"Failed to load Illuminati sprite sheet: {ex.Message}");
        }
    }

    public static ImageSource? GetIlluminatiIcon(string upgradeId)
    {
        if (_iconCache.TryGetValue(upgradeId, out var cached))
            return cached;

        if (!IlluminatiSpritePositions.TryGetValue(upgradeId, out var position))
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

    public static bool HasSprite(string upgradeId) => IlluminatiSpritePositions.ContainsKey(upgradeId);
    public static IEnumerable<string> GetAvailableUpgradeIds() => IlluminatiSpritePositions.Keys;
}
