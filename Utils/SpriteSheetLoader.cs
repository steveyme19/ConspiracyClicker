using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ConspiracyClicker.Utils;

/// <summary>
/// Loads and extracts individual icons from the generator sprite sheet.
/// Sprite sheet is 2208x1920 pixels, 8 columns x 7 rows = 276x274 pixels per icon.
/// </summary>
public static class SpriteSheetLoader
{
    private static BitmapSource? _spriteSheet;
    private static readonly Dictionary<string, ImageSource> _iconCache = new();

    private const int Columns = 8;
    private const int Rows = 7;
    private const int IconWidth = 276;
    private const int IconHeight = 274;
    private const int IconPadding = 12; // Reduce size to avoid bleeding from adjacent cells

    // Map generator IDs to sprite positions (row, column) - 0-indexed
    private static readonly Dictionary<string, (int row, int col)> GeneratorSpritePositions = new()
    {
        // Row 0 - Early conspiracy (green accents)
        ["red_string"] = (0, 0),
        ["suspicious_neighbor"] = (0, 1),
        ["basement_researcher"] = (0, 2),
        ["blogspot_blog"] = (0, 3),
        ["youtube_channel"] = (0, 4),
        ["discord_server"] = (0, 5),
        ["am_radio"] = (0, 6),
        ["podcast"] = (0, 7),

        // Row 1 - Media & surveillance (green-gold accents)
        ["truth_conference"] = (1, 0),
        ["netflix_doc"] = (1, 1),
        ["spy_satellite"] = (1, 2),
        ["shadow_government"] = (1, 3),
        ["mind_control_tower"] = (1, 4),
        ["weather_machine"] = (1, 5),
        ["clone_facility"] = (1, 6),
        ["time_machine"] = (1, 7),

        // Row 2 - Underground & space (gold accents)
        ["hollow_earth_base"] = (2, 0),
        ["moon_base"] = (2, 1),
        ["alien_alliance"] = (2, 2),
        ["dimension_portal"] = (2, 3),
        ["simulation_admin"] = (2, 4),
        ["reality_editor"] = (2, 5),
        ["void_whispers"] = (2, 6),
        ["stargate_array"] = (2, 7),

        // Row 3 - Quantum & cosmic (purple accents)
        ["quantum_entangler"] = (3, 0),
        ["multiverse_network"] = (3, 1),
        ["cosmic_consciousness"] = (3, 2),
        ["truth_singularity_gen"] = (3, 3),
        ["omniscience_engine"] = (3, 4),
        ["universe_creator"] = (3, 5),
        ["akashic_terminal"] = (3, 6),
        ["probability_weaver"] = (3, 7),

        // Row 4 - Reality manipulation (purple-gold accents)
        ["paradox_engine"] = (4, 0),
        ["entropy_reverser"] = (4, 1),
        ["probability_matrix"] = (4, 2),
        ["existence_compiler"] = (4, 3),
        ["causality_loop"] = (4, 4),
        ["absolute_truth"] = (4, 5),
        ["omega_point"] = (4, 6),
        ["void_architect"] = (4, 7),

        // Row 5 - Transcendent (white-gold accents)
        ["cosmic_forge"] = (5, 0),
        ["dimension_weaver"] = (5, 1),
        ["eternity_engine"] = (5, 2),
        ["primordial_truth"] = (5, 3),
        ["consciousness_merger"] = (5, 4),
        ["reality_seed"] = (5, 5),
        ["omni_fabricator"] = (5, 6),
        ["infinity_conduit"] = (5, 7),

        // Row 6 - Final tier (brilliant)
        ["existence_core"] = (6, 0),
        ["final_revelation_gen"] = (6, 1),
        ["information_nexus"] = (6, 2),
        ["timeline_harvester"] = (6, 3),
    };

    /// <summary>
    /// Ensures the sprite sheet is loaded.
    /// </summary>
    private static void EnsureSpriteSheetLoaded()
    {
        if (_spriteSheet != null) return;

        try
        {
            var uri = new Uri("pack://application:,,,/Resources/GeneratorSpriteSheet.png", UriKind.Absolute);
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
            System.Diagnostics.Debug.WriteLine($"Failed to load sprite sheet: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets a generator icon from the sprite sheet.
    /// </summary>
    /// <param name="generatorId">The generator ID</param>
    /// <returns>The icon ImageSource, or null if not found</returns>
    public static ImageSource? GetGeneratorIcon(string generatorId)
    {
        // Check cache first
        if (_iconCache.TryGetValue(generatorId, out var cached))
            return cached;

        // Check if we have a sprite position for this generator
        if (!GeneratorSpritePositions.TryGetValue(generatorId, out var position))
            return null;

        EnsureSpriteSheetLoaded();
        if (_spriteSheet == null) return null;

        try
        {
            // Calculate pixel coordinates with padding to avoid bleeding from adjacent cells
            int x = position.col * IconWidth + IconPadding;
            int y = position.row * IconHeight + IconPadding;
            int cropWidth = IconWidth - (IconPadding * 2);
            int cropHeight = IconHeight - (IconPadding * 2);

            // Ensure we don't go out of bounds
            int width = Math.Min(cropWidth, _spriteSheet.PixelWidth - x);
            int height = Math.Min(cropHeight, _spriteSheet.PixelHeight - y);

            if (width <= 0 || height <= 0) return null;

            // Create a cropped bitmap
            var croppedBitmap = new CroppedBitmap(_spriteSheet, new Int32Rect(x, y, width, height));
            croppedBitmap.Freeze();

            // Cache and return
            _iconCache[generatorId] = croppedBitmap;
            return croppedBitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to extract icon for {generatorId}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Checks if a sprite exists for the given generator ID.
    /// </summary>
    public static bool HasSprite(string generatorId)
    {
        return GeneratorSpritePositions.ContainsKey(generatorId);
    }

    /// <summary>
    /// Gets all generator IDs that have sprites.
    /// </summary>
    public static IEnumerable<string> GetAvailableGeneratorIds()
    {
        return GeneratorSpritePositions.Keys;
    }

    /// <summary>
    /// Preloads all generator icons into cache for better performance.
    /// Call this during app startup.
    /// </summary>
    public static void PreloadAllIcons()
    {
        foreach (var generatorId in GeneratorSpritePositions.Keys)
        {
            GetGeneratorIcon(generatorId);
        }
    }
}
