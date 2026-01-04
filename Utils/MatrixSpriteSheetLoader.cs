using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ConspiracyClicker.Utils;

/// <summary>
/// Loads and extracts individual icons from the matrix/prestige sprite sheet.
/// Sprite sheet is 1856x2304 pixels, 4 columns x 5 rows = 464x461 pixels per icon.
/// </summary>
public static class MatrixSpriteSheetLoader
{
    private static BitmapSource? _spriteSheet;
    private static readonly Dictionary<string, ImageSource> _iconCache = new();

    private const int Columns = 4;
    private const int Rows = 5;
    private const int IconWidth = 464;
    private const int IconHeight = 461;

    // Map prestige/matrix IDs to sprite positions (row, column) - 0-indexed
    private static readonly Dictionary<string, (int row, int col)> MatrixSpritePositions = new()
    {
        // Row 0 - Reality Manipulation
        ["matrix_reality_warp"] = (0, 0),
        ["reality_warp"] = (0, 0),
        ["matrix_neo_clicking"] = (0, 1),
        ["neo_clicking"] = (0, 1),
        ["matrix_agent"] = (0, 2),
        ["agent_program"] = (0, 2),
        ["matrix_source_code"] = (0, 3),
        ["source_code"] = (0, 3),

        // Row 1 - Time & Space
        ["matrix_bullet_time"] = (1, 0),
        ["bullet_time"] = (1, 0),
        ["matrix_architect"] = (1, 1),
        ["architect_access"] = (1, 1),
        ["matrix_red_pill"] = (1, 2),
        ["red_pill"] = (1, 2),
        ["matrix_oracle"] = (1, 3),
        ["oracle_vision"] = (1, 3),

        // Row 2 - Resistance
        ["matrix_zion"] = (2, 0),
        ["zion_connection"] = (2, 0),
        ["matrix_the_one"] = (2, 1),
        ["the_one"] = (2, 1),
        ["unplugged"] = (2, 2),
        ["operator_support"] = (2, 3),

        // Row 3 - Illuminati/Prestige
        ["pyramid_scheme"] = (3, 0),
        ["secret_handshake"] = (3, 1),
        ["new_world_order"] = (3, 2),
        ["reptilian_dna"] = (3, 3),

        // Row 4 - Ultimate Power
        ["moon_base"] = (4, 0),
        ["time_manipulation"] = (4, 1),
        ["cosmic_awareness"] = (4, 2),
        ["final_truth"] = (4, 3),
    };

    private static void EnsureSpriteSheetLoaded()
    {
        if (_spriteSheet != null) return;

        try
        {
            var uri = new Uri("pack://application:,,,/Resources/MatrixSpriteSheet.png", UriKind.Absolute);
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
            System.Diagnostics.Debug.WriteLine($"Failed to load matrix sprite sheet: {ex.Message}");
        }
    }

    public static ImageSource? GetMatrixIcon(string iconId)
    {
        if (_iconCache.TryGetValue(iconId, out var cached))
            return cached;

        if (!MatrixSpritePositions.TryGetValue(iconId, out var position))
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

            _iconCache[iconId] = croppedBitmap;
            return croppedBitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to extract icon for {iconId}: {ex.Message}");
            return null;
        }
    }

    public static bool HasSprite(string iconId) => MatrixSpritePositions.ContainsKey(iconId);
    public static IEnumerable<string> GetAvailableIconIds() => MatrixSpritePositions.Keys;
}
