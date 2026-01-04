using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ConspiracyClicker.Utils;

/// <summary>
/// Loads and extracts individual icons from the conspiracy sprite sheet.
/// Sprite sheet is 1312x3264 pixels, 2 columns x 5 rows = 656x653 pixels per icon.
/// </summary>
public static class ConspiracySpriteSheetLoader
{
    private static BitmapSource? _spriteSheet;
    private static readonly Dictionary<string, ImageSource> _iconCache = new();

    private const int Columns = 2;
    private const int Rows = 5;
    private const int IconWidth = 656;
    private const int IconHeight = 653;

    // Map conspiracy IDs to sprite positions (row, column) - 0-indexed
    private static readonly Dictionary<string, (int row, int col)> ConspiracySpritePositions = new()
    {
        // Row 0
        ["birds_arent_real"] = (0, 0),
        ["flat_earth"] = (0, 1),

        // Row 1
        ["moon_landing"] = (1, 0),
        ["lizard_people"] = (1, 1),

        // Row 2
        ["australia_fake"] = (2, 0),
        ["finland_myth"] = (2, 1),

        // Row 3
        ["mattress_laundering"] = (3, 0),
        ["time_invention"] = (3, 1),

        // Row 4
        ["simulation"] = (4, 0),
        ["you_are_conspiracy"] = (4, 1),
    };

    /// <summary>
    /// Ensures the sprite sheet is loaded.
    /// </summary>
    private static void EnsureSpriteSheetLoaded()
    {
        if (_spriteSheet != null) return;

        try
        {
            var uri = new Uri("pack://application:,,,/Resources/ConspiracySpriteSheet.png", UriKind.Absolute);
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
            System.Diagnostics.Debug.WriteLine($"Failed to load conspiracy sprite sheet: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets a conspiracy icon from the sprite sheet.
    /// </summary>
    /// <param name="conspiracyId">The conspiracy ID</param>
    /// <returns>The icon ImageSource, or null if not found</returns>
    public static ImageSource? GetConspiracyIcon(string conspiracyId)
    {
        // Check cache first
        if (_iconCache.TryGetValue(conspiracyId, out var cached))
            return cached;

        // Check if we have a sprite position for this conspiracy
        if (!ConspiracySpritePositions.TryGetValue(conspiracyId, out var position))
            return null;

        EnsureSpriteSheetLoaded();
        if (_spriteSheet == null) return null;

        try
        {
            // Calculate pixel coordinates
            int x = position.col * IconWidth;
            int y = position.row * IconHeight;

            // Ensure we don't go out of bounds
            int width = Math.Min(IconWidth, _spriteSheet.PixelWidth - x);
            int height = Math.Min(IconHeight, _spriteSheet.PixelHeight - y);

            if (width <= 0 || height <= 0) return null;

            // Create a cropped bitmap
            var croppedBitmap = new CroppedBitmap(_spriteSheet, new Int32Rect(x, y, width, height));
            croppedBitmap.Freeze();

            // Cache and return
            _iconCache[conspiracyId] = croppedBitmap;
            return croppedBitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to extract icon for {conspiracyId}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Checks if a sprite exists for the given conspiracy ID.
    /// </summary>
    public static bool HasSprite(string conspiracyId)
    {
        return ConspiracySpritePositions.ContainsKey(conspiracyId);
    }

    /// <summary>
    /// Gets all conspiracy IDs that have sprites.
    /// </summary>
    public static IEnumerable<string> GetAvailableConspiracyIds()
    {
        return ConspiracySpritePositions.Keys;
    }

    /// <summary>
    /// Preloads all conspiracy icons into cache for better performance.
    /// </summary>
    public static void PreloadAllIcons()
    {
        foreach (var conspiracyId in ConspiracySpritePositions.Keys)
        {
            GetConspiracyIcon(conspiracyId);
        }
    }
}
