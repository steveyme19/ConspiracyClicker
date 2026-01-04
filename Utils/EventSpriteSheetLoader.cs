using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ConspiracyClicker.Utils;

/// <summary>
/// Loads and extracts individual icons from the event sprite sheet.
/// Sprite sheet is 1792x2400 pixels, 3 columns x 4 rows = 597x600 pixels per icon.
/// </summary>
public static class EventSpriteSheetLoader
{
    private static BitmapSource? _spriteSheet;
    private static readonly Dictionary<string, ImageSource> _iconCache = new();

    private const int Columns = 3;
    private const int Rows = 4;
    private const int IconWidth = 597;
    private const int IconHeight = 600;

    // Map event IDs to sprite positions (row, column) - 0-indexed
    private static readonly Dictionary<string, (int row, int col)> EventSpritePositions = new()
    {
        // Row 0 - Bonus Events
        ["event_golden_eye"] = (0, 0),
        ["golden_eye"] = (0, 0),
        ["event_whistleblower"] = (0, 1),
        ["whistleblower"] = (0, 1),
        ["event_mystery_package"] = (0, 2),
        ["mystery_package"] = (0, 2),

        // Row 1 - Resource Events
        ["event_tinfoil_rain"] = (1, 0),
        ["tinfoil_rain"] = (1, 0),
        ["event_deep_state_leak"] = (1, 1),
        ["deep_state_leak"] = (1, 1),
        ["event_viral"] = (1, 2),
        ["viral_moment"] = (1, 2),

        // Row 2 - Multiplier Events
        ["event_automated"] = (2, 0),
        ["automated_discovery"] = (2, 0),
        ["event_insider"] = (2, 1),
        ["insider_information"] = (2, 1),
        ["mass_hysteria"] = (2, 2),

        // Row 3 - Rare Events
        ["alien_contact"] = (3, 0),
        ["time_glitch"] = (3, 1),
        ["truth_bomb"] = (3, 2),
    };

    private static void EnsureSpriteSheetLoaded()
    {
        if (_spriteSheet != null) return;

        try
        {
            var uri = new Uri("pack://application:,,,/Resources/EventSpriteSheet.png", UriKind.Absolute);
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
            System.Diagnostics.Debug.WriteLine($"Failed to load event sprite sheet: {ex.Message}");
        }
    }

    public static ImageSource? GetEventIcon(string eventId)
    {
        if (_iconCache.TryGetValue(eventId, out var cached))
            return cached;

        if (!EventSpritePositions.TryGetValue(eventId, out var position))
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

            _iconCache[eventId] = croppedBitmap;
            return croppedBitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to extract icon for {eventId}: {ex.Message}");
            return null;
        }
    }

    public static bool HasSprite(string eventId) => EventSpritePositions.ContainsKey(eventId);
    public static IEnumerable<string> GetAvailableEventIds() => EventSpritePositions.Keys;
}
