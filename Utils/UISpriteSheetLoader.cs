using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ConspiracyClicker.Utils;

/// <summary>
/// Loads and extracts individual icons from the UI icon sprite sheet.
/// Sprite sheet is 6 columns x 7 rows = 424x424 pixels per icon.
/// </summary>
public static class UISpriteSheetLoader
{
    private static BitmapSource? _spriteSheet;
    private static readonly Dictionary<string, ImageSource> _iconCache = new();

    private const int Columns = 6;
    private const int Rows = 7;
    private const int IconWidth = 424;
    private const int IconHeight = 424;
    private const int IconPadding = 20;

    // Map icon IDs to sprite positions (row, column) - 0-indexed
    private static readonly Dictionary<string, (int row, int col)> IconSpritePositions = new()
    {
        // Row 0 - UI Controls
        ["fullscreen"] = (0, 0),
        ["exit_fullscreen"] = (0, 1),
        ["menu"] = (0, 2),
        ["sound_on"] = (0, 3),
        ["sound_off"] = (0, 4),
        ["question_mark"] = (0, 5),

        // Row 1 - UI Controls + Risk Levels
        ["zen_mode_on"] = (1, 0),
        ["zen_mode_off"] = (1, 1),
        ["link"] = (1, 2),
        ["risk_low"] = (1, 3),
        ["risk_medium"] = (1, 4),
        ["risk_high"] = (1, 5),

        // Row 2 - Orbit Icons (first set)
        ["orbit_pin"] = (2, 0),
        ["orbit_eye"] = (2, 1),
        ["orbit_computer"] = (2, 2),
        ["orbit_blog"] = (2, 3),

        // Row 3 - Orbit Icons (continued)
        ["orbit_play"] = (3, 0),
        ["orbit_chat"] = (3, 1),
        ["orbit_radio"] = (3, 2),
        ["orbit_microphone"] = (3, 3),
        ["orbit_stage"] = (3, 4),
        ["orbit_film"] = (3, 5),

        // Row 4 - Skill Tree + more orbit icons
        ["skill_researcher"] = (4, 0),
        ["orbit_satellite"] = (4, 2),
        ["orbit_capitol"] = (4, 3),
        ["skill_influencer"] = (4, 4),
        ["skill_infiltrator"] = (4, 5),

        // Row 5 - Utility Icons
        ["binoculars"] = (5, 0),
        ["black_light"] = (5, 1),
        ["burner_phone"] = (5, 2),
        ["document"] = (5, 3),
        ["encrypted_usb"] = (5, 4),
        ["flashlight"] = (5, 5),

        // Row 6 - Utility Icons continued
        ["notebook"] = (6, 0),
        ["pin"] = (6, 1),
        ["satellite_dish"] = (6, 2),
        ["scanner"] = (6, 3),
        ["sticky_notes"] = (6, 4),
        ["trash"] = (6, 5),
    };

    private static void EnsureSpriteSheetLoaded()
    {
        if (_spriteSheet != null) return;

        try
        {
            var uri = new Uri("pack://application:,,,/Resources/IconSpriteSheet.png", UriKind.Absolute);
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
            System.Diagnostics.Debug.WriteLine($"Failed to load UI sprite sheet: {ex.Message}");
        }
    }

    public static ImageSource? GetUIIcon(string iconId)
    {
        if (_iconCache.TryGetValue(iconId, out var cached))
            return cached;

        if (!IconSpritePositions.TryGetValue(iconId, out var position))
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
            System.Diagnostics.Debug.WriteLine($"Failed to extract UI icon for {iconId}: {ex.Message}");
            return null;
        }
    }

    public static bool HasSprite(string iconId) => IconSpritePositions.ContainsKey(iconId);
    public static IEnumerable<string> GetAvailableIconIds() => IconSpritePositions.Keys;
}
