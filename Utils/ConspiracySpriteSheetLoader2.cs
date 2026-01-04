using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ConspiracyClicker.Utils;

/// <summary>
/// Loads and extracts individual icons from the second conspiracy sprite sheet.
/// Sprite sheet is 3 columns x 5 rows for advanced conspiracies.
/// </summary>
public static class ConspiracySpriteSheetLoader2
{
    private static BitmapSource? _spriteSheet;
    private static readonly Dictionary<string, ImageSource> _iconCache = new();

    private const int Columns = 3;
    private const int Rows = 5;
    private const int IconWidth = 656;
    private const int IconHeight = 653;
    private const int IconPadding = 10;

    // Map conspiracy IDs to sprite positions (row, column) - 0-indexed
    private static readonly Dictionary<string, (int row, int col)> ConspiracySpritePositions = new()
    {
        // Row 0 - Hidden Places
        ["denver_airport"] = (0, 0),
        ["antarctica_treaty"] = (0, 1),
        ["tartaria"] = (0, 2),

        // Row 1 - Cosmic Secrets
        ["hollow_moon"] = (1, 0),
        ["mandela_effect"] = (1, 1),
        ["breakaway_civilization"] = (1, 2),

        // Row 2 - Reality Breaking
        ["cern_portal"] = (2, 0),
        ["matrix_source_code"] = (2, 1),
        ["akashic_records"] = (2, 2),

        // Row 3 - Ultimate Powers
        ["god_committee"] = (3, 0),
        ["universe_experiment"] = (3, 1),
        ["multiverse_conspiracy"] = (3, 2),

        // Row 4 - Final Tier
        ["consciousness_prison"] = (4, 0),
        ["truth_singularity"] = (4, 1),
        ["final_revelation"] = (4, 2),
    };

    private static void EnsureSpriteSheetLoaded()
    {
        if (_spriteSheet != null) return;

        try
        {
            var uri = new Uri("pack://application:,,,/Resources/ConspiracySpriteSheet2.png", UriKind.Absolute);
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
            System.Diagnostics.Debug.WriteLine($"Failed to load conspiracy sprite sheet 2: {ex.Message}");
        }
    }

    public static ImageSource? GetConspiracyIcon(string conspiracyId)
    {
        if (_iconCache.TryGetValue(conspiracyId, out var cached))
            return cached;

        if (!ConspiracySpritePositions.TryGetValue(conspiracyId, out var position))
            return null;

        EnsureSpriteSheetLoaded();
        if (_spriteSheet == null) return null;

        try
        {
            // Calculate actual icon dimensions from sprite sheet
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

            _iconCache[conspiracyId] = croppedBitmap;
            return croppedBitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to extract conspiracy icon for {conspiracyId}: {ex.Message}");
            return null;
        }
    }

    public static bool HasSprite(string conspiracyId) => ConspiracySpritePositions.ContainsKey(conspiracyId);
    public static IEnumerable<string> GetAvailableConspiracyIds() => ConspiracySpritePositions.Keys;
}
