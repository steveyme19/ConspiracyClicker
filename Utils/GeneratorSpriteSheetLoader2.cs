using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ConspiracyClicker.Utils;

/// <summary>
/// Loads and extracts individual icons from the second generator sprite sheet (final tier).
/// Sprite sheet is 2 columns x 2 rows for ultimate generators.
/// </summary>
public static class GeneratorSpriteSheetLoader2
{
    private static BitmapSource? _spriteSheet;
    private static readonly Dictionary<string, ImageSource> _iconCache = new();

    private const int Columns = 2;
    private const int Rows = 2;
    private const int IconPadding = 15;

    // Map generator IDs to sprite positions (row, column) - 0-indexed
    private static readonly Dictionary<string, (int row, int col)> GeneratorSpritePositions = new()
    {
        // Row 0 - Ultimate Generators
        ["infinite_recursion"] = (0, 0),
        ["causality_loop"] = (0, 1),

        // Row 1 - Final Generators
        ["absolute_truth"] = (1, 0),
        ["reality_seed"] = (1, 1),
    };

    private static void EnsureSpriteSheetLoaded()
    {
        if (_spriteSheet != null) return;

        try
        {
            var uri = new Uri("pack://application:,,,/Resources/GeneratorSpriteSheet2.png", UriKind.Absolute);
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
            System.Diagnostics.Debug.WriteLine($"Failed to load generator sprite sheet 2: {ex.Message}");
        }
    }

    public static ImageSource? GetGeneratorIcon(string generatorId)
    {
        if (_iconCache.TryGetValue(generatorId, out var cached))
            return cached;

        if (!GeneratorSpritePositions.TryGetValue(generatorId, out var position))
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

            _iconCache[generatorId] = croppedBitmap;
            return croppedBitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to extract generator icon for {generatorId}: {ex.Message}");
            return null;
        }
    }

    public static bool HasSprite(string generatorId) => GeneratorSpritePositions.ContainsKey(generatorId);
    public static IEnumerable<string> GetAvailableGeneratorIds() => GeneratorSpritePositions.Keys;
}
