using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;
using ConspiracyClicker.Core;
using ConspiracyClicker.Data;
using ConspiracyClicker.Models;
using ConspiracyClicker.Utils;

namespace ConspiracyClicker;

public partial class MainWindow : Window
{
    // Dark title bar support
    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    private readonly GameEngine _engine;
    private readonly UserSettings _settings;
    private readonly Random _random = new();
    private readonly Dictionary<string, Button> _generatorButtons = new();
    private readonly Dictionary<string, StackPanel> _generatorContainers = new();
    private readonly Dictionary<string, WrapPanel> _generatorUpgradePanels = new();
    private readonly Dictionary<string, Button> _upgradeButtons = new();
    private readonly Dictionary<string, Button> _conspiracyButtons = new();
    private readonly Dictionary<string, Button> _questButtons = new();
    private readonly Dictionary<string, Button> _tinfoilButtons = new();
    private readonly List<string> _notificationLog = new();
    private const int MAX_NOTIFICATIONS = 10;

    // Track data for dirty checking
    private int _lastUpgradeCount = -1;
    private int _lastPurchasedUpgradeCount = -1;
    private int _lastProvenConspiracyCount = -1;
    private int _currentPyramidLevel = -1;
    private string _currentRankId = "";
    private readonly Dictionary<string, (double cost, int owned, double prod, double mult)> _lastGenState = new();
    private readonly Dictionary<string, string> _lastUpgradePanelState = new(); // generatorId -> state hash
    private int _lastTinfoilCount = -1;
    private int _lastQuestCount = -1;
    private int _lastAchievementCount = -1;
    private string _lastOwnedGenState = "";
    private string _lastSkillTreeState = "";
    private string _lastPrestigeState = "";
    private bool _challengeComboPopulated = false;

    // Frozen brushes for performance
    private static readonly SolidColorBrush GreenBrush;
    private static readonly SolidColorBrush GoldBrush;
    private static readonly SolidColorBrush SilverBrush;
    private static readonly SolidColorBrush LightBrush;
    private static readonly SolidColorBrush DimBrush;
    private static readonly SolidColorBrush DarkBrush;
    private static readonly SolidColorBrush RedBrush;
    private static readonly SolidColorBrush OrangeBrush;
    private static readonly SolidColorBrush PurpleBrush;
    private static readonly SolidColorBrush CritBrush;
    private static readonly SolidColorBrush AmbientBrush;
    private static readonly FontFamily EmojiFont;

    static MainWindow()
    {
        // Create and freeze brushes for thread-safe, faster rendering
        GreenBrush = new SolidColorBrush(Color.FromRgb(0, 255, 65)); GreenBrush.Freeze();
        GoldBrush = new SolidColorBrush(Color.FromRgb(255, 215, 0)); GoldBrush.Freeze();
        SilverBrush = new SolidColorBrush(Color.FromRgb(192, 192, 192)); SilverBrush.Freeze();
        LightBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224)); LightBrush.Freeze();
        DimBrush = new SolidColorBrush(Color.FromRgb(136, 136, 136)); DimBrush.Freeze();
        DarkBrush = new SolidColorBrush(Color.FromRgb(42, 42, 62)); DarkBrush.Freeze();
        RedBrush = new SolidColorBrush(Color.FromRgb(255, 68, 68)); RedBrush.Freeze();
        OrangeBrush = new SolidColorBrush(Color.FromRgb(255, 165, 0)); OrangeBrush.Freeze();
        PurpleBrush = new SolidColorBrush(Color.FromRgb(153, 51, 255)); PurpleBrush.Freeze();
        CritBrush = new SolidColorBrush(Color.FromRgb(255, 100, 100)); CritBrush.Freeze();
        AmbientBrush = new SolidColorBrush(Color.FromArgb(80, 0, 255, 65)); AmbientBrush.Freeze();
        EmojiFont = new FontFamily("Segoe UI Emoji");
    }

    // === PARTICLE POOLING ===
    private readonly Queue<Ellipse> _particlePool = new();
    private readonly List<(Ellipse particle, TranslateTransform transform, double endTime)> _activeParticles = new();
    private const int MAX_POOL_SIZE = 100;

    // Reusable lists to avoid allocations
    private readonly List<AmbientParticle> _ambientToRemove = new();
    private readonly List<LuckyDrop> _dropsToRemove = new();

    // Buy mode: 1, 10, 100, or -1 for max
    private int _buyMode = 1;

    // === ORBIT SYSTEM ===
    private class OrbitIcon
    {
        public required string GeneratorId { get; init; }
        public required TextBlock Element { get; init; }
        public double Angle { get; set; }
        public required double Radius { get; init; }
        public required double Speed { get; init; }
        public double BobPhase { get; set; }
    }

    private readonly List<OrbitIcon> _orbitIcons = new();
    private DateTime _lastRenderTime = DateTime.Now;

    // Generator orbit configurations: icon, radius, speed (farther = slower)
    private static readonly Dictionary<string, (string Icon, double Radius, double Speed)> OrbitConfigs = new()
    {
        // Inner orbit (110-130) - moderate speed
        ["red_string"] = ("📌", 110, 0.4),
        ["suspicious_neighbor"] = ("👀", 120, 0.35),
        ["basement_researcher"] = ("💻", 130, 0.3),
        // Middle orbit (160-190) - slower
        ["blogspot_blog"] = ("📝", 160, 0.25),
        ["youtube_channel"] = ("▶️", 175, 0.22),
        ["discord_server"] = ("💬", 190, 0.2),
        // Outer orbit (220-260) - even slower
        ["am_radio"] = ("📻", 220, 0.15),
        ["podcast"] = ("🎙️", 240, 0.13),
        ["truth_conference"] = ("🏨", 260, 0.11),
        // Far orbit (290-330) - slowest, majestic
        ["netflix_doc"] = ("🎬", 290, 0.09),
        ["spy_satellite"] = ("🛰️", 310, 0.08),
        ["shadow_government"] = ("🏛️", 330, 0.06)
    };

    // === AMBIENT PARTICLES ===
    private class AmbientParticle
    {
        public required UIElement Element { get; init; }
        public required RotateTransform CachedTransform { get; init; }
        public double X { get; set; }
        public double Y { get; set; }
        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public double Life { get; set; }
        public double MaxLife { get; init; }
        public double Rotation { get; set; }
        public double RotationSpeed { get; set; }
    }

    private readonly List<AmbientParticle> _ambientParticles = new();
    private double _ambientSpawnTimer = 0;
    private static readonly string[] AmbientIconKeys = { "Icon_document", "Icon_triangle", "Icon_all_seeing_eye_small", "Icon_question", "Icon_pin", "Icon_link" };

    // === STARFIELD ===
    private class Star
    {
        public required Ellipse Element { get; init; }
        public double TwinklePhase { get; set; }
        public double TwinkleSpeed { get; set; }
        public double BaseOpacity { get; set; }
    }
    private readonly List<Star> _stars = new();
    private bool _starfieldInitialized = false;

    // === MYSTICAL SPARKS ===
    private class MysticalSpark
    {
        public required Ellipse Element { get; init; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Angle { get; set; }
        public double Distance { get; set; }
        public double Speed { get; set; }
        public double Life { get; set; }
        public double PulsePhase { get; set; }
    }
    private readonly List<MysticalSpark> _mysticalSparks = new();
    private readonly List<MysticalSpark> _sparksToRemove = new();
    private double _sparkSpawnTimer = 0;

    // === ORBITING RUNES ===
    private class OrbitingRune
    {
        public required TextBlock Element { get; init; }
        public double Angle { get; set; }
        public double Radius { get; set; }
        public double Speed { get; set; }
        public double PulsePhase { get; set; }
    }
    private readonly List<OrbitingRune> _orbitingRunes = new();
    private bool _runesInitialized = false;
    private static readonly string[] RuneSymbols = { "☉", "☽", "★", "✧", "⚶", "◈", "⬡", "△", "⊛", "⍟", "✦", "◇" };

    // === MENU BACKGROUND EFFECTS ===
    private class MenuStar
    {
        public required Ellipse Element { get; init; }
        public double TwinklePhase { get; set; }
        public double TwinkleSpeed { get; set; }
        public double BaseOpacity { get; set; }
    }
    private class MenuParticle
    {
        public required UIElement Element { get; init; }
        public double X { get; set; }
        public double Y { get; set; }
        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public double Life { get; set; }
        public double MaxLife { get; set; }
        public double PulsePhase { get; set; }
    }
    private readonly List<MenuStar> _menuStars = new();
    private readonly List<MenuParticle> _menuParticles = new();
    private readonly List<MenuParticle> _menuParticlesToRemove = new();
    private bool _menuBackgroundInitialized = false;
    private double _menuParticleSpawnTimer = 0;
    private double _menuPyramidAngle = 0;

    // === NEWS TICKER ===
    private double _newsTickerX = 0;
    private int _currentNewsIndex = 0;
    private static readonly string[] NewsHeadlines = {
        "Local man connects 47th red string, claims 'it all makes sense now'",
        "Birds seen recharging on power lines - experts baffled",
        "Finland still suspiciously absent from most satellite photos",
        "Area 51 gift shop reports record sales of 'I was never here' t-shirts",
        "Mattress store on every corner - coincidence? We think not",
        "Man who 'did his own research' now mass of red string and coffee",
        "Government denies existence of government denial department",
        "Flat Earth Society reports members 'all around the globe'",
        "Lizard person spotted blinking sideways at press conference",
        "Local basement dweller achieves enlightenment, still won't go outside",
        "Big Tinfoil suppressing hat industry innovations, insiders claim",
        "Moon landing director admits 'we should have used better lighting'",
        "Chemtrails taste like conspiracy, reports man who licked sky",
        "Time traveler warns about future, refuses to give lottery numbers",
        "Simulation theory proven true, developers refuse to patch bugs"
    };

    // === LUCKY DROPS ===
    private class LuckyDrop
    {
        public required Button Element { get; init; }
        public double X { get; set; }
        public double Y { get; set; }
        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public double Life { get; set; }
        public required string Type { get; init; }
        public bool IsFading { get; set; } = false;
        public double FadeTime { get; set; } = 1.0; // 1 second fade
    }
    private readonly List<LuckyDrop> _luckyDrops = new();
    private double _luckyDropTimer = 0;

    // Temporary boosts from lucky drops
    private DateTime _clickBoostEnd = DateTime.MinValue;
    private double _clickBoostMultiplier = 1.0;
    private DateTime _goldenDropEnd = DateTime.MinValue;

    // === DEBUNKERS ===
    private class Debunker
    {
        public required Border Element { get; init; }
        public double TimeLeft { get; set; }
        public int ClicksRequired { get; set; }
        public int ClicksReceived { get; set; }
    }
    private Debunker? _activeDebunker = null;
    private double _debunkerSpawnTimer = 0;

    // === HOSTILE EVENTS ===
    // Evidence Thief: Moves around the screen, click to catch (single click but moving target)
    private class EvidenceThief
    {
        public required Border Element { get; init; }
        public double X { get; set; }
        public double Y { get; set; }
        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public double TimeLeft { get; set; }
        public double StealAmount { get; set; }
        public double Speed { get; set; }
        public double DirectionChangeTimer { get; set; }
    }
    private EvidenceThief? _activeEvidenceThief = null;
    private double _evidenceThiefTimer = 0;

    // Tinfoil Thief: Progress bar fills up, click to push it back, scales with tinfoil amount
    private class TinfoilThief
    {
        public required Border Element { get; init; }
        public required Border ProgressBar { get; init; }
        public double StealProgress { get; set; } // 0 to 1, steals at 1
        public double ProgressRate { get; set; } // How fast it fills
        public double PushbackAmount { get; set; } // How much each click reduces progress
        public int StealAmount { get; set; }
        public double TimeLeft { get; set; } // Max time before it leaves (even if not full)
    }
    private TinfoilThief? _activeTinfoilThief = null;
    private double _tinfoilThiefTimer = 0;

    // === SPECIAL EVENTS ===
    private double _specialEventTimer = 0;

    // Event 1: Escaped Document - click it multiple times before it flies away
    private class EscapedDocument
    {
        public required Border Element { get; init; }
        public double X { get; set; }
        public double Y { get; set; }
        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public int ClicksNeeded { get; set; }
        public int ClicksReceived { get; set; }
        public double TimeLeft { get; set; }
    }
    private EscapedDocument? _escapedDocument = null;

    // Event 2: Evidence Trail - hover over orbs to collect them
    private class EvidenceOrb
    {
        public required Ellipse Element { get; init; }
        public bool Collected { get; set; }
    }
    private readonly List<EvidenceOrb> _evidenceTrail = new();
    private int _trailCollected = 0;
    private double _trailTimeLeft = 0;

    // Event 3: Red String Connection - click two pins in sequence
    private class ConnectionPin
    {
        public required Border Element { get; init; }
        public int Order { get; set; }
        public bool Clicked { get; set; }
    }
    private readonly List<ConnectionPin> _connectionPins = new();
    private int _nextPinToClick = 1;
    private double _connectionTimeLeft = 0;

    // === MINIGAMES ===
    private enum MinigameType { None, ClickFrenzy, DocumentCatch, MemoryMatrix }
    private MinigameType _currentMinigame = MinigameType.None;
    private bool _minigameActive = false;
    private double _minigameTimer = 0;
    private int _minigameScore = 0;
    private double _minigameSpawnTimer = 0;
    private List<UIElement> _minigameElements = new();
    private int[]? _memoryPattern = null;
    private int _memoryIndex = 0;
    private int _memoryDifficulty = 4; // Number of cells to remember (4-12)

    // Main menu state
    private int _selectedSlot = 0;

    public MainWindow()
    {
        InitializeComponent();

        // Load user settings
        _settings = UserSettings.Load();
        ApplyWindowSettings();

        // Enable dark title bar
        var hwnd = new WindowInteropHelper(this).EnsureHandle();
        int darkMode = 1;
        DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));

        _engine = new GameEngine();
        _engine.OnTick += UpdateUI;
        _engine.OnFlavorMessage += ShowFlavorMessage;
        _engine.OnAchievementUnlocked += ShowAchievementUnlocked;
        _engine.OnClickProcessed += OnClickProcessed;
        _engine.OnComboBurst += OnComboBurst;
        _engine.OnQuestComplete += OnQuestComplete;
        _engine.OnGoldenEyeStart += OnGoldenEyeStart;
        _engine.OnGoldenEyeEnd += OnGoldenEyeEnd;
        _engine.OnPrestigeAvailable += OnPrestigeAvailable;
        _engine.OnPrestigeComplete += OnPrestigeComplete;
        _engine.OnDailyChallengeComplete += OnDailyChallengeComplete;
        _engine.OnOfflineProgress += OnOfflineProgress;
        _engine.SaveManager.OnError += OnSaveError;

        // Hook into rendering for smooth 60fps animations
        CompositionTarget.Rendering += OnRendering;

        // Initialize sound system
        SoundManager.Volume = _settings.SoundVolume; // Set volume before Initialize to apply to generated sounds
        SoundManager.ClickVolume = _settings.ClickVolume; // Set click volume too
        SoundManager.Initialize();
        SoundManager.Enabled = _settings.SoundEnabled;
        UpdateSoundIcon();

        InitializeGeneratorButtons();

        // Set version from assembly
        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        VersionText.Text = $"v{version?.Major}.{version?.Minor}.{version?.Build} - Click responsibly";

        // Show main menu on startup
        RefreshMainMenu();

        // Add keyboard shortcuts
        this.KeyDown += MainWindow_KeyDown;
        this.Focusable = true;

        // Start eye animations
        StartEyeAnimations();

        // Initialize pyramid and eye from sprite sheet
        InitializePyramidAndEye();
    }

    private void InitializePyramidAndEye()
    {
        // Set initial pyramid icon (level 0)
        var pyramid = PyramidSpriteSheetLoader.GetPyramidIcon(0);
        if (pyramid != null) PyramidImage.Source = pyramid;

        // Set initial eye icon (tier based on level 0)
        var eye = PyramidSpriteSheetLoader.GetEyeIcon(pyramidLevel: 0);
        if (eye != null) EyeImage.Source = eye;
    }

    private void StartEyeAnimations()
    {
        var glowAnim = (Storyboard)FindResource("AmbientGlowAnimation");
        glowAnim.Begin(this, true);
    }

    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        // Ignore if menu is showing
        if (MainMenuOverlay.Visibility == Visibility.Visible) return;

        switch (e.Key)
        {
            // Spacebar = Click
            case Key.Space:
                _engine.ProcessClick();
                SoundManager.Play("click");
                e.Handled = true;
                break;

            // 1-9 = Buy generators (based on visible order)
            case Key.D1:
            case Key.NumPad1:
                BuyGeneratorByIndex(0);
                e.Handled = true;
                break;
            case Key.D2:
            case Key.NumPad2:
                BuyGeneratorByIndex(1);
                e.Handled = true;
                break;
            case Key.D3:
            case Key.NumPad3:
                BuyGeneratorByIndex(2);
                e.Handled = true;
                break;
            case Key.D4:
            case Key.NumPad4:
                BuyGeneratorByIndex(3);
                e.Handled = true;
                break;
            case Key.D5:
            case Key.NumPad5:
                BuyGeneratorByIndex(4);
                e.Handled = true;
                break;
            case Key.D6:
            case Key.NumPad6:
                BuyGeneratorByIndex(5);
                e.Handled = true;
                break;
            case Key.D7:
            case Key.NumPad7:
                BuyGeneratorByIndex(6);
                e.Handled = true;
                break;
            case Key.D8:
            case Key.NumPad8:
                BuyGeneratorByIndex(7);
                e.Handled = true;
                break;
            case Key.D9:
            case Key.NumPad9:
                BuyGeneratorByIndex(8);
                e.Handled = true;
                break;

            // Tab = Cycle tabs
            case Key.Tab:
                CycleTab(Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? -1 : 1);
                e.Handled = true;
                break;

            // M = Toggle mute
            case Key.M:
                SoundManager.ToggleMute();
                UpdateSoundIcon();
                e.Handled = true;
                break;

            // F11 = Toggle fullscreen
            case Key.F11:
                ToggleFullscreen();
                e.Handled = true;
                break;

            // Escape = Exit fullscreen (if in fullscreen)
            case Key.Escape:
                if (_isFullscreen)
                {
                    ToggleFullscreen();
                    e.Handled = true;
                }
                break;
        }
    }

    private void BuyGeneratorByIndex(int index)
    {
        if (index < GeneratorData.AllGenerators.Count)
        {
            var gen = GeneratorData.AllGenerators[index];
            if (_engine.PurchaseGenerator(gen.Id))
            {
                SoundManager.Play("purchase");
            }
        }
    }

    private void CycleTab(int direction)
    {
        var tabs = new[] { GeneratorsTab, UpgradesTab, ConspiraciesTab, QuestsTab, TinfoilShopTab, SkillsTab, AchievementsTab, IlluminatiTab };
        var visibleTabs = tabs.Where(t => t.Visibility == Visibility.Visible).ToList();
        if (visibleTabs.Count == 0) return;

        int currentIndex = visibleTabs.FindIndex(t => t.IsSelected);
        int newIndex = (currentIndex + direction + visibleTabs.Count) % visibleTabs.Count;
        visibleTabs[newIndex].IsSelected = true;
    }

    private void UpdateSoundIcon()
    {
        var iconKey = SoundManager.IsMuted ? "Icon_sound_off" : "Icon_sound_on";
        SoundToggleIcon.Source = (System.Windows.Media.ImageSource)Application.Current.FindResource(iconKey);
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        double deltaTime = (now - _lastRenderTime).TotalSeconds;
        _lastRenderTime = now;

        // Cap delta time to avoid huge jumps
        if (deltaTime > 0.1) deltaTime = 0.1;

        // Update menu background if menu is visible
        if (MainMenuOverlay.Visibility == Visibility.Visible)
        {
            UpdateMenuBackground(deltaTime);
        }

        UpdateOrbits(deltaTime);
        UpdateAmbientParticles(deltaTime);
        UpdateStarfield(deltaTime);
        UpdateMysticalSparks(deltaTime);
        UpdateOrbitingRunes(deltaTime);
        UpdateNewsTicker(deltaTime);
        UpdateLuckyDrops(deltaTime);
        UpdateDebunker(deltaTime);
        UpdateEvidenceThief(deltaTime);
        UpdateTinfoilThief(deltaTime);
        UpdateSpecialEvents(deltaTime);
        UpdateMinigame(deltaTime);

        // Random minigame spawn (every 2-4 minutes) - only after first conspiracy, not in zen mode
        if (!_zenMode && !_minigameActive && MinigameOverlay.Visibility != Visibility.Visible &&
            _engine.State.ProvenConspiracies.Count >= 1)
        {
            _minigameSpawnTimer += deltaTime;
            if (_minigameSpawnTimer > 120 + _random.NextDouble() * 120) // 2-4 minutes
            {
                _minigameSpawnTimer = 0;
                SpawnRandomMinigame();
            }
        }
    }

    // === NEWS TICKER ===
    private void UpdateNewsTicker(double deltaTime)
    {
        if (NewsTickerCanvas.ActualWidth <= 0) return;

        _newsTickerX -= deltaTime * 80; // Scroll speed

        string currentNews = NewsHeadlines[_currentNewsIndex];
        NewsTickerText.Text = currentNews;

        // Reset when scrolled off screen
        if (_newsTickerX < -NewsTickerText.ActualWidth - 50)
        {
            _newsTickerX = NewsTickerCanvas.ActualWidth;
            _currentNewsIndex = (_currentNewsIndex + 1) % NewsHeadlines.Length;
        }

        Canvas.SetLeft(NewsTickerText, _newsTickerX);
    }

    // === LUCKY DROPS ===
    private void UpdateLuckyDrops(double deltaTime)
    {
        double width = ClickCanvas.ActualWidth;
        double height = ClickCanvas.ActualHeight;

        if (width <= 0 || height <= 0) return;

        // Spawn new drops randomly (only after first conspiracy is proven, not in zen mode)
        _luckyDropTimer += deltaTime;
        if (!_zenMode && _engine.State.ProvenConspiracies.Count > 0 &&
            _luckyDropTimer > 8 + _random.NextDouble() * 12 && _luckyDrops.Count < 3)
        {
            _luckyDropTimer = 0;
            SpawnLuckyDrop(width, height);
        }

        // Update existing drops using reusable list
        _dropsToRemove.Clear();
        foreach (var drop in _luckyDrops)
        {
            drop.X += drop.VelocityX * deltaTime;
            drop.Y += drop.VelocityY * deltaTime;

            // Gentle floating motion
            drop.VelocityY += Math.Sin(drop.Life * 3) * 0.5;

            // Check if drop should start fading (still clickable during fade!)
            if (!drop.IsFading)
            {
                drop.Life -= deltaTime;
                if (drop.Life <= 0)
                {
                    drop.IsFading = true;
                    drop.FadeTime = 1.0; // Start 1 second fade
                }
            }
            else
            {
                // Fading - reduce fade time
                drop.FadeTime -= deltaTime;
                if (drop.FadeTime <= 0)
                {
                    _dropsToRemove.Add(drop);
                    continue;
                }
            }

            // Remove if off screen
            if (drop.X < -50 || drop.X > width + 50)
            {
                _dropsToRemove.Add(drop);
                continue;
            }

            Canvas.SetLeft(drop.Element, drop.X);
            Canvas.SetTop(drop.Element, drop.Y);

            // Visual effect - pulse when alive, fade when fading
            if (drop.IsFading)
            {
                drop.Element.Opacity = drop.FadeTime; // Fade from 1.0 to 0 over 1 second
            }
            else
            {
                drop.Element.Opacity = 0.8 + 0.2 * Math.Sin(drop.Life * 5);
            }
        }

        foreach (var drop in _dropsToRemove)
        {
            InteractiveCanvas.Children.Remove(drop.Element);
            _luckyDrops.Remove(drop);
        }
    }

    private void SpawnLuckyDrop(double width, double height)
    {
        // Weighted random - rarer drops are less likely
        int roll = _random.Next(100);
        string type;
        if (roll < 35) type = "evidence";        // 35% - Evidence burst
        else if (roll < 55) type = "tinfoil";    // 20% - Tinfoil
        else if (roll < 75) type = "clickboost"; // 20% - Temporary click power
        else if (roll < 90) type = "goldeneye";  // 15% - Temporary golden eye
        else type = "jackpot";                   // 10% - Big jackpot

        string icon = type switch
        {
            "evidence" => "📜",
            "tinfoil" => "🎩",
            "clickboost" => "👆",
            "goldeneye" => "👁️",
            "jackpot" => "💎",
            _ => "❓"
        };

        var button = new Button
        {
            Content = icon,
            FontSize = 28,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
            Tag = type
        };
        button.Click += LuckyDrop_Click;

        bool fromLeft = _random.NextDouble() < 0.5;
        double velocityX = fromLeft ? 40 + _random.NextDouble() * 25 : -40 - _random.NextDouble() * 25; // 40-65 px/sec
        double startX = fromLeft ? -30 : width + 30;
        // Calculate life to ensure drop crosses the full screen
        double travelDistance = width + 100;
        double lifeNeeded = travelDistance / Math.Abs(velocityX) + 3; // Extra buffer

        var drop = new LuckyDrop
        {
            Element = button,
            X = startX,
            Y = 50 + _random.NextDouble() * (height - 100),
            VelocityX = velocityX,
            VelocityY = (_random.NextDouble() - 0.5) * 8,
            Life = lifeNeeded,
            Type = type
        };

        Canvas.SetLeft(button, drop.X);
        Canvas.SetTop(button, drop.Y);
        InteractiveCanvas.Children.Add(button);
        _luckyDrops.Add(drop);
    }

    private void LuckyDrop_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
        {
            var drop = _luckyDrops.FirstOrDefault(d => d.Element == btn);
            if (drop != null)
            {
                SoundManager.Play("drop");
                string notificationText = "";
                SolidColorBrush notificationColor = GoldBrush;

                // Give reward based on type
                switch (drop.Type)
                {
                    case "evidence":
                        double reward = _engine.CalculateEvidencePerSecond() * 30;
                        _engine.State.Evidence += reward;
                        _engine.State.TotalEvidenceEarned += reward;
                        notificationText = $"📜 CLASSIFIED DOCUMENT: +{NumberFormatter.Format(reward)} evidence instantly!";
                        ShowToast("📜", $"+{NumberFormatter.Format(reward)} evidence!");
                        break;

                    case "tinfoil":
                        int tinfoil = 3 + _random.Next(8);
                        _engine.State.Tinfoil += tinfoil;
                        notificationText = $"🎩 TINFOIL STASH: +{tinfoil} tinfoil for the shop!";
                        notificationColor = SilverBrush;
                        ShowToast("🎩", $"+{tinfoil} tinfoil!");
                        break;

                    case "clickboost":
                        _clickBoostMultiplier = 3.0;
                        _clickBoostEnd = DateTime.Now.AddSeconds(20);
                        notificationText = "👆 POWER SURGE: 3x click power for 20 seconds!";
                        notificationColor = GreenBrush;
                        ShowToast("👆", "3x CLICK POWER!");
                        break;

                    case "goldeneye":
                        _goldenDropEnd = DateTime.Now.AddSeconds(15);
                        _engine.State.GoldenEyeActive = true;
                        _engine.State.GoldenEyeEndTime = _goldenDropEnd;
                        notificationText = "👁️ GOLDEN VISION: 5x evidence gain for 15 seconds!";
                        notificationColor = GoldBrush;
                        ShowToast("👁️", "GOLDEN EYE ACTIVATED!");
                        break;

                    case "jackpot":
                        double bigReward = _engine.CalculateEvidencePerSecond() * 120;
                        int bigTinfoil = 10 + _random.Next(15);
                        _engine.State.Evidence += bigReward;
                        _engine.State.TotalEvidenceEarned += bigReward;
                        _engine.State.Tinfoil += bigTinfoil;
                        notificationText = $"💎 JACKPOT! +{NumberFormatter.Format(bigReward)} evidence AND +{bigTinfoil} tinfoil!";
                        notificationColor = PurpleBrush;
                        ShowToast("💎", "JACKPOT!!!");
                        SoundManager.Play("achievement");
                        break;
                }

                // Add to notification log
                AddNotification(notificationText, notificationColor);

                // Remove the drop
                InteractiveCanvas.Children.Remove(btn);
                _luckyDrops.Remove(drop);

                // Spawn celebration particles
                SpawnLuckyDropParticles(drop.X + 15, drop.Y + 15);
            }
        }
    }

    private void SpawnLuckyDropParticles(double x, double y)
    {
        var duration = TimeSpan.FromMilliseconds(350);

        // Reduced from 20 to 10 particles
        for (int i = 0; i < 10; i++)
        {
            var particle = new Ellipse { Width = 5, Height = 5, Fill = GoldBrush };
            Canvas.SetLeft(particle, x);
            Canvas.SetTop(particle, y);
            ClickCanvas.Children.Add(particle);

            double angle = _random.NextDouble() * Math.PI * 2;
            double distance = 25 + _random.NextDouble() * 30;

            var transform = new TranslateTransform();
            particle.RenderTransform = transform;

            var xAnim = new DoubleAnimation { To = Math.Cos(angle) * distance, Duration = duration };
            var yAnim = new DoubleAnimation { To = Math.Sin(angle) * distance, Duration = duration };
            var fadeAnim = new DoubleAnimation { To = 0, Duration = duration };

            fadeAnim.Completed += (s, e) => ClickCanvas.Children.Remove(particle);

            transform.BeginAnimation(TranslateTransform.XProperty, xAnim);
            transform.BeginAnimation(TranslateTransform.YProperty, yAnim);
            particle.BeginAnimation(OpacityProperty, fadeAnim);
        }
    }

    // === DEBUNKERS ===
    private void UpdateDebunker(double deltaTime)
    {
        if (_activeDebunker != null)
        {
            _activeDebunker.TimeLeft -= deltaTime;

            // Update timer display
            var timerText = _activeDebunker.Element.Child as StackPanel;
            if (timerText?.Children.Count > 1 && timerText.Children[1] is TextBlock tb)
            {
                tb.Text = $"⏱️ {_activeDebunker.TimeLeft:F1}s - Click {_activeDebunker.ClicksRequired - _activeDebunker.ClicksReceived} more times!";
            }

            if (_activeDebunker.TimeLeft <= 0)
            {
                // Debunker won - lose some believers
                double loss = _engine.State.Believers * 0.1;
                _engine.State.Believers = Math.Max(0, _engine.State.Believers - loss);
                ShowToast("😱", $"Debunked! Lost {NumberFormatter.FormatInteger(loss)} believers!");
                AddNotification($"🕵️ Debunker escaped! Lost {NumberFormatter.FormatInteger(loss)} believers.", RedBrush);
                SoundManager.Play("error");
                InteractiveCanvas.Children.Remove(_activeDebunker.Element);
                _activeDebunker = null;
            }
        }
        else
        {
            // Maybe spawn a new debunker (not during minigames, only after first conspiracy)
            if (!_zenMode && !_minigameActive && MinigameOverlay.Visibility != Visibility.Visible &&
                _engine.State.ProvenConspiracies.Count > 0)
            {
                _debunkerSpawnTimer += deltaTime;
                if (_debunkerSpawnTimer > 45 + _random.NextDouble() * 30 && _engine.State.Believers > 100)
                {
                    _debunkerSpawnTimer = 0;
                    SpawnDebunker();
                }
            }
        }
    }

    private void SpawnDebunker()
    {
        double centerX = ClickCanvas.ActualWidth / 2;
        double centerY = ClickCanvas.ActualHeight / 2;

        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(220, 60, 20, 20)),
            BorderBrush = RedBrush,
            BorderThickness = new Thickness(3),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(15),
            Cursor = Cursors.Hand
        };

        var stack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
        stack.Children.Add(new TextBlock
        {
            Text = "🕵️ DEBUNKER ALERT!",
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = RedBrush,
            HorizontalAlignment = HorizontalAlignment.Center
        });
        stack.Children.Add(new TextBlock
        {
            Text = "⏱️ 5.0s - Click 10 times!",
            FontSize = 12,
            Foreground = LightBrush,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0)
        });

        border.Child = stack;
        border.MouseDown += Debunker_Click;

        Canvas.SetLeft(border, centerX - 100);
        Canvas.SetTop(border, centerY - 40);
        InteractiveCanvas.Children.Add(border);

        _activeDebunker = new Debunker
        {
            Element = border,
            TimeLeft = 8.0,
            ClicksRequired = 10,
            ClicksReceived = 0
        };

        ShowToast("🚨", "DEBUNKER INCOMING! Click to defeat them!");
        AddNotification("🚨 DEBUNKER ALERT! Click 10 times in 8 seconds to defeat!", OrangeBrush);
        SoundManager.Play("debunker");
    }

    private void Debunker_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_activeDebunker == null) return;

        _activeDebunker.ClicksReceived++;

        // Shake effect
        var transform = _activeDebunker.Element.RenderTransform as TranslateTransform ?? new TranslateTransform();
        _activeDebunker.Element.RenderTransform = transform;
        var shake = new DoubleAnimation { From = -5, To = 5, Duration = TimeSpan.FromMilliseconds(50), AutoReverse = true };
        transform.BeginAnimation(TranslateTransform.XProperty, shake);

        if (_activeDebunker.ClicksReceived >= _activeDebunker.ClicksRequired)
        {
            // Victory!
            SoundManager.Play("achievement");
            double reward = _engine.CalculateEvidencePerSecond() * 45;
            _engine.State.Evidence += reward;
            _engine.State.TotalEvidenceEarned += reward;
            _engine.State.Tinfoil += 3;
            ShowToast("🎉", $"Debunker defeated! +{NumberFormatter.Format(reward)} evidence +3 tinfoil!");
            AddNotification($"🎉 Debunker defeated! +{NumberFormatter.Format(reward)} evidence +3 tinfoil!", GreenBrush);

            // Victory particles
            double posX = Canvas.GetLeft(_activeDebunker.Element) + 50;
            double posY = Canvas.GetTop(_activeDebunker.Element) + 30;
            SpawnLuckyDropParticles(posX, posY);

            InteractiveCanvas.Children.Remove(_activeDebunker.Element);
            _activeDebunker = null;
        }
    }

    // === EVIDENCE THIEF (Moving target - chase and click!) ===
    private void UpdateEvidenceThief(double deltaTime)
    {
        if (_activeEvidenceThief != null)
        {
            _activeEvidenceThief.TimeLeft -= deltaTime;
            _activeEvidenceThief.DirectionChangeTimer -= deltaTime;

            // Change direction randomly
            if (_activeEvidenceThief.DirectionChangeTimer <= 0)
            {
                _activeEvidenceThief.DirectionChangeTimer = 0.5 + _random.NextDouble() * 0.5;
                double angle = _random.NextDouble() * Math.PI * 2;
                _activeEvidenceThief.VelocityX = Math.Cos(angle) * _activeEvidenceThief.Speed;
                _activeEvidenceThief.VelocityY = Math.Sin(angle) * _activeEvidenceThief.Speed;
            }

            // Move the thief
            double width = InteractiveCanvas.ActualWidth;
            double height = InteractiveCanvas.ActualHeight;
            _activeEvidenceThief.X += _activeEvidenceThief.VelocityX * deltaTime;
            _activeEvidenceThief.Y += _activeEvidenceThief.VelocityY * deltaTime;

            // Bounce off edges (element is 130x105)
            if (_activeEvidenceThief.X < 10)
            {
                _activeEvidenceThief.X = 10;
                _activeEvidenceThief.VelocityX = Math.Abs(_activeEvidenceThief.VelocityX);
            }
            if (_activeEvidenceThief.X > width - 140)
            {
                _activeEvidenceThief.X = width - 140;
                _activeEvidenceThief.VelocityX = -Math.Abs(_activeEvidenceThief.VelocityX);
            }
            if (_activeEvidenceThief.Y < 10)
            {
                _activeEvidenceThief.Y = 10;
                _activeEvidenceThief.VelocityY = Math.Abs(_activeEvidenceThief.VelocityY);
            }
            if (_activeEvidenceThief.Y > height - 115)
            {
                _activeEvidenceThief.Y = height - 115;
                _activeEvidenceThief.VelocityY = -Math.Abs(_activeEvidenceThief.VelocityY);
            }

            Canvas.SetLeft(_activeEvidenceThief.Element, _activeEvidenceThief.X);
            Canvas.SetTop(_activeEvidenceThief.Element, _activeEvidenceThief.Y);

            // Update timer display
            var timerText = _activeEvidenceThief.Element.Child as StackPanel;
            if (timerText?.Children.Count > 1 && timerText.Children[1] is TextBlock tb)
            {
                tb.Text = $"⏱️ {_activeEvidenceThief.TimeLeft:F1}s";
            }

            if (_activeEvidenceThief.TimeLeft <= 0)
            {
                // Thief escaped - steal evidence
                double stolen = Math.Min(_engine.State.Evidence, _activeEvidenceThief.StealAmount);
                _engine.State.Evidence -= stolen;
                ShowToast("💸", $"Evidence stolen! Lost {NumberFormatter.Format(stolen)}!");
                AddNotification($"🦹 Evidence Thief escaped with {NumberFormatter.Format(stolen)} evidence!", RedBrush);
                SoundManager.Play("error");
                InteractiveCanvas.Children.Remove(_activeEvidenceThief.Element);
                _activeEvidenceThief = null;
            }
        }
        else
        {
            // Maybe spawn a new evidence thief - only after second conspiracy, not in zen mode
            if (!_zenMode && !_minigameActive && MinigameOverlay.Visibility != Visibility.Visible && _activeDebunker == null &&
                _engine.State.ProvenConspiracies.Count >= 2)
            {
                _evidenceThiefTimer += deltaTime;
                if (_evidenceThiefTimer > 60 + _random.NextDouble() * 45 && _engine.State.Evidence > 1000)
                {
                    _evidenceThiefTimer = 0;
                    SpawnEvidenceThief();
                }
            }
        }
    }

    private void SpawnEvidenceThief()
    {
        double width = InteractiveCanvas.ActualWidth;
        double height = InteractiveCanvas.ActualHeight;

        // Start from a random edge (element is 130x105)
        double startX, startY;
        int edge = _random.Next(4);
        switch (edge)
        {
            case 0: startX = 10; startY = _random.NextDouble() * (height - 115); break;
            case 1: startX = width - 140; startY = _random.NextDouble() * (height - 115); break;
            case 2: startX = _random.NextDouble() * (width - 140); startY = 10; break;
            default: startX = _random.NextDouble() * (width - 140); startY = height - 115; break;
        }

        // Calculate steal amount (15-25% of current evidence)
        double stealPercent = 0.15 + _random.NextDouble() * 0.10;
        double stealAmount = _engine.State.Evidence * stealPercent;

        // Shadowy agent/MIB style design
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(240, 20, 20, 30)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(80, 80, 100)),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(10),
            Cursor = Cursors.Hand,
            Width = 130,
            Height = 105,
            Effect = new DropShadowEffect { Color = Colors.Black, BlurRadius = 15, ShadowDepth = 3, Opacity = 0.8 }
        };

        var stack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

        // Agent silhouette with hat and briefcase
        stack.Children.Add(new TextBlock
        {
            Text = "🕵️",
            FontSize = 32,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontFamily = EmojiFont
        });
        stack.Children.Add(new TextBlock
        {
            Text = $"⏱️ 6.0s",
            FontSize = 11,
            Foreground = LightBrush,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 2, 0, 0)
        });
        stack.Children.Add(new TextBlock
        {
            Text = "[CLASSIFIED]",
            FontSize = 9,
            Foreground = RedBrush,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontWeight = FontWeights.Bold,
            FontFamily = new FontFamily("Consolas")
        });

        border.Child = stack;
        border.MouseDown += EvidenceThief_Click;

        Canvas.SetLeft(border, startX);
        Canvas.SetTop(border, startY);
        InteractiveCanvas.Children.Add(border);

        double speed = 120 + _random.NextDouble() * 60; // 120-180 pixels per second
        double angle = _random.NextDouble() * Math.PI * 2;

        _activeEvidenceThief = new EvidenceThief
        {
            Element = border,
            X = startX,
            Y = startY,
            VelocityX = Math.Cos(angle) * speed,
            VelocityY = Math.Sin(angle) * speed,
            TimeLeft = 6.0,
            StealAmount = stealAmount,
            Speed = speed,
            DirectionChangeTimer = 0.5
        };

        ShowToast("🚨", "EVIDENCE THIEF! Chase and click to catch!");
        AddNotification($"🦹 Evidence Thief appeared! Chase and click before time runs out!", OrangeBrush);
        SoundManager.Play("debunker");
    }

    private void EvidenceThief_Click(object sender, MouseButtonEventArgs e)
    {
        if (_activeEvidenceThief == null) return;

        // Single click catches the thief!
        SoundManager.Play("achievement");
        double reward = _engine.CalculateEvidencePerSecond() * 45;
        _engine.State.Evidence += reward;
        _engine.State.TotalEvidenceEarned += reward;
        _engine.State.Tinfoil += 3;
        ShowToast("🎉", $"Thief caught! +{NumberFormatter.Format(reward)} evidence +3 tinfoil!");
        AddNotification($"🎉 Evidence Thief caught! +{NumberFormatter.Format(reward)} evidence +3 tinfoil!", GreenBrush);

        // Victory particles
        double posX = _activeEvidenceThief.X + 40;
        double posY = _activeEvidenceThief.Y + 30;
        SpawnLuckyDropParticles(posX, posY);

        InteractiveCanvas.Children.Remove(_activeEvidenceThief.Element);
        _activeEvidenceThief = null;
    }

    // === TINFOIL THIEF (Progress bar - click to push back!) ===
    private void UpdateTinfoilThief(double deltaTime)
    {
        if (_activeTinfoilThief != null)
        {
            _activeTinfoilThief.TimeLeft -= deltaTime;

            // Progress fills up over time
            _activeTinfoilThief.StealProgress += _activeTinfoilThief.ProgressRate * deltaTime;
            _activeTinfoilThief.StealProgress = Math.Min(1.0, _activeTinfoilThief.StealProgress);

            // Update progress bar visual
            _activeTinfoilThief.ProgressBar.Width = 140 * _activeTinfoilThief.StealProgress;

            // Update text
            var stack = _activeTinfoilThief.Element.Child as StackPanel;
            if (stack?.Children.Count > 2 && stack.Children[2] is TextBlock tb)
            {
                int percent = (int)(_activeTinfoilThief.StealProgress * 100);
                tb.Text = $"Stealing: {percent}% ({_activeTinfoilThief.StealAmount} tinfoil)";
            }

            if (_activeTinfoilThief.StealProgress >= 1.0)
            {
                // Thief succeeded - steal tinfoil
                long stolen = Math.Min(_engine.State.Tinfoil, _activeTinfoilThief.StealAmount);
                _engine.State.Tinfoil -= stolen;
                ShowToast("💎", $"Tinfoil stolen! Lost {stolen}!");
                AddNotification($"🎭 Tinfoil Thief stole {stolen} tinfoil!", RedBrush);
                SoundManager.Play("error");
                InteractiveCanvas.Children.Remove(_activeTinfoilThief.Element);
                _activeTinfoilThief = null;
            }
            else if (_activeTinfoilThief.TimeLeft <= 0)
            {
                // Time ran out but didn't steal - thief gives up
                ShowToast("😅", "Tinfoil Thief gave up!");
                AddNotification($"🎭 Tinfoil Thief fled empty-handed!", GreenBrush);
                InteractiveCanvas.Children.Remove(_activeTinfoilThief.Element);
                _activeTinfoilThief = null;
            }
        }
        else
        {
            // Maybe spawn a new tinfoil thief (rarer than evidence thief) - only after second conspiracy, not in zen mode
            if (!_zenMode && !_minigameActive && MinigameOverlay.Visibility != Visibility.Visible &&
                _activeDebunker == null && _activeEvidenceThief == null &&
                _engine.State.ProvenConspiracies.Count >= 2)
            {
                _tinfoilThiefTimer += deltaTime;
                if (_tinfoilThiefTimer > 90 + _random.NextDouble() * 60 && _engine.State.Tinfoil >= 5)
                {
                    _tinfoilThiefTimer = 0;
                    SpawnTinfoilThief();
                }
            }
        }
    }

    private void SpawnTinfoilThief()
    {
        double centerX = InteractiveCanvas.ActualWidth / 2;
        double centerY = InteractiveCanvas.ActualHeight / 2;

        // Scale with tinfoil amount
        long tinfoil = _engine.State.Tinfoil;

        // Steal amount scales: 10-30% of current tinfoil, minimum 1, max 100
        double stealPercent = 0.10 + Math.Min(0.20, tinfoil / 500.0 * 0.20);
        int stealAmount = Math.Max(1, Math.Min(100, (int)(tinfoil * stealPercent)));

        // Progress rate scales: more tinfoil = faster steal (but also more time)
        double baseRate = 0.15; // Base: fills in ~6.7 seconds
        double scaledRate = baseRate + Math.Min(0.10, tinfoil / 1000.0 * 0.10); // Up to 0.25 (4 seconds)

        // Pushback scales: harder to push back with more tinfoil
        double pushback = 0.20 - Math.Min(0.12, tinfoil / 500.0 * 0.12); // 0.20 down to 0.08
        pushback = Math.Max(0.05, pushback);

        // Time limit scales up slightly with tinfoil
        double timeLimit = 8.0 + Math.Min(4.0, tinfoil / 200.0);

        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(230, 50, 30, 80)),
            BorderBrush = PurpleBrush,
            BorderThickness = new Thickness(3),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(12),
            Cursor = Cursors.Hand,
            Width = 180
        };

        var stack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
        stack.Children.Add(new TextBlock
        {
            Text = "🎭 TINFOIL THIEF!",
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = PurpleBrush,
            HorizontalAlignment = HorizontalAlignment.Center
        });

        // Progress bar container
        var progressContainer = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)),
            BorderBrush = SilverBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            Height = 16,
            Width = 140,
            Margin = new Thickness(0, 8, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var progressBar = new Border
        {
            Background = PurpleBrush,
            CornerRadius = new CornerRadius(2),
            Height = 12,
            Width = 0,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(2)
        };
        progressContainer.Child = progressBar;
        stack.Children.Add(progressContainer);

        stack.Children.Add(new TextBlock
        {
            Text = $"Stealing: 0% ({stealAmount} tinfoil)",
            FontSize = 10,
            Foreground = LightBrush,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0)
        });

        stack.Children.Add(new TextBlock
        {
            Text = "CLICK TO PUSH BACK!",
            FontSize = 9,
            FontWeight = FontWeights.Bold,
            Foreground = GoldBrush,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 3, 0, 0)
        });

        border.Child = stack;
        border.MouseDown += TinfoilThief_Click;

        double offsetX = (_random.NextDouble() - 0.5) * 100;
        double offsetY = (_random.NextDouble() - 0.5) * 80;
        Canvas.SetLeft(border, centerX - 90 + offsetX);
        Canvas.SetTop(border, centerY - 50 + offsetY);
        InteractiveCanvas.Children.Add(border);

        _activeTinfoilThief = new TinfoilThief
        {
            Element = border,
            ProgressBar = progressBar,
            StealProgress = 0,
            ProgressRate = scaledRate,
            PushbackAmount = pushback,
            StealAmount = stealAmount,
            TimeLeft = timeLimit
        };

        ShowToast("🚨", "TINFOIL THIEF! Click to push back the steal meter!");
        AddNotification($"🎭 Tinfoil Thief appeared! Click rapidly to prevent the steal!", PurpleBrush);
        SoundManager.Play("debunker");
    }

    private void TinfoilThief_Click(object sender, MouseButtonEventArgs e)
    {
        if (_activeTinfoilThief == null) return;

        // Push back the progress
        _activeTinfoilThief.StealProgress -= _activeTinfoilThief.PushbackAmount;
        _activeTinfoilThief.StealProgress = Math.Max(0, _activeTinfoilThief.StealProgress);

        // Shake effect
        var transform = _activeTinfoilThief.Element.RenderTransform as TranslateTransform ?? new TranslateTransform();
        _activeTinfoilThief.Element.RenderTransform = transform;
        var shake = new DoubleAnimation { From = -3, To = 3, Duration = TimeSpan.FromMilliseconds(30), AutoReverse = true };
        transform.BeginAnimation(TranslateTransform.XProperty, shake);

        // If pushed all the way back, thief flees
        if (_activeTinfoilThief.StealProgress <= 0)
        {
            SoundManager.Play("achievement");
            int reward = 2 + _random.Next(4); // 2-5 tinfoil
            _engine.State.Tinfoil += reward;
            ShowToast("🎉", $"Thief defeated! +{reward} tinfoil!");
            AddNotification($"🎉 Tinfoil Thief defeated! +{reward} tinfoil!", GreenBrush);

            double posX = Canvas.GetLeft(_activeTinfoilThief.Element) + 90;
            double posY = Canvas.GetTop(_activeTinfoilThief.Element) + 40;
            SpawnLuckyDropParticles(posX, posY);

            InteractiveCanvas.Children.Remove(_activeTinfoilThief.Element);
            _activeTinfoilThief = null;
        }
    }

    // === SPECIAL EVENTS ===
    private void UpdateSpecialEvents(double deltaTime)
    {
        double width = ClickCanvas.ActualWidth;
        double height = ClickCanvas.ActualHeight;
        if (width <= 0 || height <= 0) return;

        // Update active events
        UpdateEscapedDocument(deltaTime);
        UpdateEvidenceTrail(deltaTime);
        UpdateConnectionPins(deltaTime);

        // Spawn new events randomly (only one at a time, only after first conspiracy, not in zen mode)
        if (!_zenMode && _engine.State.ProvenConspiracies.Count > 0 &&
            _escapedDocument == null && _evidenceTrail.Count == 0 && _connectionPins.Count == 0)
        {
            _specialEventTimer += deltaTime;
            if (_specialEventTimer > 15 + _random.NextDouble() * 20) // 15-35 seconds
            {
                _specialEventTimer = 0;
                int eventType = _random.Next(3);
                switch (eventType)
                {
                    case 0: SpawnEscapedDocument(width, height); break;
                    case 1: SpawnEvidenceTrail(width, height); break;
                    case 2: SpawnConnectionPins(width, height); break;
                }
            }
        }
    }

    // --- Event 1: Escaped Document ---
    private void SpawnEscapedDocument(double width, double height)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(230, 50, 50, 70)),
            BorderBrush = GoldBrush,
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(5),
            Padding = new Thickness(10, 8, 10, 8),
            Cursor = Cursors.Hand,
            Effect = new System.Windows.Media.Effects.DropShadowEffect { Color = Colors.Gold, BlurRadius = 10, ShadowDepth = 0, Opacity = 0.7 }
        };

        var stack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
        stack.Children.Add(new TextBlock { Text = "📄 ESCAPED DOCUMENT!", FontSize = 12, FontWeight = FontWeights.Bold, Foreground = GoldBrush, HorizontalAlignment = HorizontalAlignment.Center });
        stack.Children.Add(new TextBlock { Text = "Click 5 times to catch!", FontSize = 10, Foreground = LightBrush, HorizontalAlignment = HorizontalAlignment.Center });
        border.Child = stack;
        border.MouseDown += EscapedDocument_Click;

        double startX = -100;
        double startY = 50 + _random.NextDouble() * (height - 150);

        Canvas.SetLeft(border, startX);
        Canvas.SetTop(border, startY);
        InteractiveCanvas.Children.Add(border);

        double velocityX = 60 + _random.NextDouble() * 30; // 60-90 px/sec
        // Calculate time needed to cross screen (start at -100, exit at width+100)
        double travelDistance = width + 200;
        double timeNeeded = travelDistance / velocityX + 2; // Extra buffer time

        _escapedDocument = new EscapedDocument
        {
            Element = border,
            X = startX,
            Y = startY,
            VelocityX = velocityX,
            VelocityY = (_random.NextDouble() - 0.5) * 20,
            ClicksNeeded = 5,
            ClicksReceived = 0,
            TimeLeft = timeNeeded
        };

        ShowToast("📄", "Escaped document! Click it 5 times!");
        AddNotification("📄 Escaped document spotted! Click it before it escapes!", GoldBrush);
    }

    private void UpdateEscapedDocument(double deltaTime)
    {
        if (_escapedDocument == null) return;

        _escapedDocument.TimeLeft -= deltaTime;
        _escapedDocument.X += _escapedDocument.VelocityX * deltaTime;
        _escapedDocument.Y += _escapedDocument.VelocityY * deltaTime;
        _escapedDocument.VelocityY += Math.Sin(_escapedDocument.TimeLeft * 2) * 2;

        Canvas.SetLeft(_escapedDocument.Element, _escapedDocument.X);
        Canvas.SetTop(_escapedDocument.Element, _escapedDocument.Y);

        double width = ClickCanvas.ActualWidth;
        if (_escapedDocument.X > width + 100 || _escapedDocument.TimeLeft <= 0)
        {
            InteractiveCanvas.Children.Remove(_escapedDocument.Element);
            _escapedDocument = null;
            AddNotification("📄 Document escaped...", DimBrush);
        }
    }

    private void EscapedDocument_Click(object sender, MouseButtonEventArgs e)
    {
        if (_escapedDocument == null) return;

        _escapedDocument.ClicksReceived++;
        SoundManager.Play("click");

        // Shake effect
        var transform = _escapedDocument.Element.RenderTransform as TranslateTransform ?? new TranslateTransform();
        _escapedDocument.Element.RenderTransform = transform;
        var shake = new DoubleAnimation { From = -8, To = 8, Duration = TimeSpan.FromMilliseconds(50), AutoReverse = true };
        transform.BeginAnimation(TranslateTransform.XProperty, shake);

        // Slow it down
        _escapedDocument.VelocityX *= 0.85;

        if (_escapedDocument.ClicksReceived >= _escapedDocument.ClicksNeeded)
        {
            SoundManager.Play("achievement");
            double reward = _engine.CalculateEvidencePerSecond() * 20;
            _engine.State.Evidence += reward;
            _engine.State.TotalEvidenceEarned += reward;
            _engine.State.Tinfoil += 2;

            SpawnLuckyDropParticles(_escapedDocument.X + 60, _escapedDocument.Y + 25);
            ShowToast("📄", $"Document captured! +{NumberFormatter.Format(reward)} evidence!");
            AddNotification($"📄 Document captured! +{NumberFormatter.Format(reward)} evidence +2 tinfoil!", GreenBrush);

            InteractiveCanvas.Children.Remove(_escapedDocument.Element);
            _escapedDocument = null;
        }
    }

    // --- Event 2: Evidence Trail ---
    private void SpawnEvidenceTrail(double width, double height)
    {
        _evidenceTrail.Clear();
        _trailCollected = 0;
        _trailTimeLeft = 6;

        double centerX = width / 2;
        double centerY = height / 2;
        double radius = Math.Min(width, height) * 0.35;

        for (int i = 0; i < 6; i++)
        {
            double angle = (i / 6.0) * Math.PI * 2 + _random.NextDouble() * 0.3;
            double x = centerX + Math.Cos(angle) * radius + (_random.NextDouble() - 0.5) * 40;
            double y = centerY + Math.Sin(angle) * radius + (_random.NextDouble() - 0.5) * 40;

            var orb = new Ellipse
            {
                Width = 35,
                Height = 35,
                Fill = new RadialGradientBrush(Color.FromRgb(0, 255, 100), Color.FromArgb(100, 0, 200, 50)),
                Stroke = GreenBrush,
                StrokeThickness = 2,
                Cursor = Cursors.Hand,
                Effect = new System.Windows.Media.Effects.DropShadowEffect { Color = Colors.Lime, BlurRadius = 15, ShadowDepth = 0, Opacity = 0.8 }
            };

            Canvas.SetLeft(orb, x - 17);
            Canvas.SetTop(orb, y - 17);
            InteractiveCanvas.Children.Add(orb);

            var evidenceOrb = new EvidenceOrb { Element = orb, Collected = false };
            orb.MouseEnter += (s, e) => EvidenceOrb_MouseEnter(evidenceOrb);
            _evidenceTrail.Add(evidenceOrb);
        }

        ShowToast("✨", "Evidence trail! Hover over all orbs!");
        AddNotification("✨ Evidence trail appeared! Mouse over all orbs to collect!", PurpleBrush);
    }

    private void EvidenceOrb_MouseEnter(EvidenceOrb orb)
    {
        if (orb.Collected || _trailTimeLeft <= 0) return;

        orb.Collected = true;
        _trailCollected++;
        SoundManager.Play("drop");

        // Shrink and fade
        var scaleAnim = new DoubleAnimation { To = 0, Duration = TimeSpan.FromMilliseconds(200) };
        var fadeAnim = new DoubleAnimation { To = 0, Duration = TimeSpan.FromMilliseconds(200) };
        fadeAnim.Completed += (s, e) => InteractiveCanvas.Children.Remove(orb.Element);

        orb.Element.RenderTransform = new ScaleTransform(1, 1, 17, 17);
        ((ScaleTransform)orb.Element.RenderTransform).BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
        ((ScaleTransform)orb.Element.RenderTransform).BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        orb.Element.BeginAnimation(OpacityProperty, fadeAnim);

        if (_trailCollected >= _evidenceTrail.Count)
        {
            SoundManager.Play("achievement");
            double reward = _engine.CalculateEvidencePerSecond() * 15;
            _engine.State.Evidence += reward;
            _engine.State.TotalEvidenceEarned += reward;
            _engine.State.Tinfoil += 3;

            ShowToast("✨", $"Trail complete! +{NumberFormatter.Format(reward)} evidence!");
            AddNotification($"✨ Evidence trail complete! +{NumberFormatter.Format(reward)} evidence +3 tinfoil!", GreenBrush);
            _evidenceTrail.Clear();
        }
    }

    private void UpdateEvidenceTrail(double deltaTime)
    {
        if (_evidenceTrail.Count == 0) return;

        _trailTimeLeft -= deltaTime;
        if (_trailTimeLeft <= 0)
        {
            foreach (var orb in _evidenceTrail)
                InteractiveCanvas.Children.Remove(orb.Element);
            _evidenceTrail.Clear();
            if (_trailCollected < 6)
                AddNotification("✨ Evidence trail faded away...", DimBrush);
        }
    }

    // --- Event 3: Red String Connection ---
    private void SpawnConnectionPins(double width, double height)
    {
        _connectionPins.Clear();
        _nextPinToClick = 1;
        _connectionTimeLeft = 8;

        double centerX = width / 2;
        double centerY = height / 2;

        // Create 3 pins at evenly spaced angles to prevent overlap
        double baseAngle = _random.NextDouble() * Math.PI * 2;
        for (int i = 1; i <= 3; i++)
        {
            // Space pins 120 degrees apart with small random offset
            double angle = baseAngle + (i - 1) * (Math.PI * 2 / 3) + (_random.NextDouble() - 0.5) * 0.5;
            double radius = 80 + _random.NextDouble() * 100;
            double x = centerX + Math.Cos(angle) * radius;
            double y = centerY + Math.Sin(angle) * radius;

            var border = new Border
            {
                Width = 50,
                Height = 50,
                Background = new SolidColorBrush(Color.FromArgb(200, 80, 20, 20)),
                BorderBrush = RedBrush,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(25),
                Cursor = Cursors.Hand
            };

            var text = new TextBlock
            {
                Text = $"📌{i}",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = LightBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            border.Child = text;

            Canvas.SetLeft(border, x - 25);
            Canvas.SetTop(border, y - 25);
            InteractiveCanvas.Children.Add(border);

            var pin = new ConnectionPin { Element = border, Order = i, Clicked = false };
            border.MouseDown += (s, e) => ConnectionPin_Click(pin);
            _connectionPins.Add(pin);
        }

        ShowToast("📌", "Connect the pins! Click 1, 2, 3 in order!");
        AddNotification("📌 Red string connection! Click pins 1, 2, 3 in order!", OrangeBrush);
    }

    private void ConnectionPin_Click(ConnectionPin pin)
    {
        if (pin.Clicked || _connectionTimeLeft <= 0) return;

        if (pin.Order == _nextPinToClick)
        {
            pin.Clicked = true;
            _nextPinToClick++;
            SoundManager.Play("click");

            // Turn green
            pin.Element.BorderBrush = GreenBrush;
            pin.Element.Background = new SolidColorBrush(Color.FromArgb(200, 20, 80, 20));

            if (_nextPinToClick > 3)
            {
                SoundManager.Play("achievement");
                double reward = _engine.CalculateEvidencePerSecond() * 25;
                _engine.State.Evidence += reward;
                _engine.State.TotalEvidenceEarned += reward;
                _engine.State.Tinfoil += 2;

                ShowToast("📌", $"Connected! +{NumberFormatter.Format(reward)} evidence!");
                AddNotification($"📌 Red string connected! +{NumberFormatter.Format(reward)} evidence +2 tinfoil!", GreenBrush);

                foreach (var p in _connectionPins)
                    InteractiveCanvas.Children.Remove(p.Element);
                _connectionPins.Clear();
            }
        }
        else
        {
            // Wrong order - flash red
            SoundManager.Play("error");
            var originalBg = pin.Element.Background;
            pin.Element.Background = RedBrush;
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            timer.Tick += (s, e) => { pin.Element.Background = originalBg; timer.Stop(); };
            timer.Start();
        }
    }

    private void UpdateConnectionPins(double deltaTime)
    {
        if (_connectionPins.Count == 0) return;

        _connectionTimeLeft -= deltaTime;
        if (_connectionTimeLeft <= 0)
        {
            foreach (var pin in _connectionPins)
                InteractiveCanvas.Children.Remove(pin.Element);
            _connectionPins.Clear();
            AddNotification("📌 Connection timed out...", DimBrush);
        }
    }

    private double _orbitSyncTimer = 0;

    private void UpdateOrbits(double deltaTime)
    {
        // Get canvas center (eye is centered in the canvas)
        double centerX = ClickCanvas.ActualWidth / 2;
        double centerY = ClickCanvas.ActualHeight / 2;

        if (centerX <= 0 || centerY <= 0) return;

        // Update existing orbit positions
        foreach (var orbit in _orbitIcons)
        {
            orbit.Angle += orbit.Speed * deltaTime;
            orbit.BobPhase += deltaTime * 3;

            double bob = Math.Sin(orbit.BobPhase) * 5;
            double x = centerX + Math.Cos(orbit.Angle) * orbit.Radius - 12;
            double y = centerY + Math.Sin(orbit.Angle) * orbit.Radius + bob - 12;

            Canvas.SetLeft(orbit.Element, x);
            Canvas.SetTop(orbit.Element, y);

            // Keep high opacity with subtle pulse
            orbit.Element.Opacity = 0.9 + 0.1 * Math.Sin(orbit.BobPhase * 0.5);
        }

        // Sync orbit icons with generator counts (only every 0.5 seconds to reduce CPU)
        _orbitSyncTimer += deltaTime;
        if (_orbitSyncTimer >= 0.5)
        {
            _orbitSyncTimer = 0;
            SyncOrbitIcons();
        }
    }

    private readonly Dictionary<string, int> _orbitIconCounts = new();

    private void SyncOrbitIcons()
    {
        var state = _engine.State;

        foreach (var (genId, config) in OrbitConfigs)
        {
            int owned = state.GetGeneratorCount(genId);
            int targetIcons = Math.Min(owned / 5, 2); // 1 icon per 5, max 2

            // Use cached count instead of LINQ Count()
            _orbitIconCounts.TryGetValue(genId, out int currentIcons);

            // Add icons if needed
            while (currentIcons < targetIcons)
            {
                SpawnOrbitIcon(genId, config.Icon, config.Radius, config.Speed);
                currentIcons++;
            }

            // Remove icons if needed
            while (currentIcons > targetIcons)
            {
                var toRemove = _orbitIcons.LastOrDefault(o => o.GeneratorId == genId);
                if (toRemove != null)
                {
                    ClickCanvas.Children.Remove(toRemove.Element);
                    _orbitIcons.Remove(toRemove);
                    currentIcons--;
                }
            }

            _orbitIconCounts[genId] = currentIcons;
        }
    }

    private void SpawnOrbitIcon(string genId, string icon, double radius, double speed)
    {
        var element = new TextBlock
        {
            Text = icon,
            FontSize = 28,
            FontFamily = new FontFamily("Segoe UI Emoji"),
            Foreground = Brushes.White,
            Opacity = 1.0,
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Lime,
                BlurRadius = 15,
                ShadowDepth = 0,
                Opacity = 1.0
            }
        };

        // Random starting angle for variety
        double startAngle = _random.NextDouble() * Math.PI * 2;

        var orbitIcon = new OrbitIcon
        {
            GeneratorId = genId,
            Element = element,
            Angle = startAngle,
            Radius = radius + (_random.NextDouble() - 0.5) * 10, // Slight radius variation
            Speed = speed + (_random.NextDouble() - 0.5) * 0.2,  // Slight speed variation
            BobPhase = _random.NextDouble() * Math.PI * 2
        };

        ClickCanvas.Children.Add(element);
        _orbitIcons.Add(orbitIcon);

        // Spawn burst effect for new icon
        SpawnOrbitBurst(orbitIcon);
    }

    private void SpawnOrbitBurst(OrbitIcon orbit)
    {
        double centerX = ClickCanvas.ActualWidth / 2;
        double centerY = ClickCanvas.ActualHeight / 2;

        if (centerX <= 0 || centerY <= 0) return;

        double x = centerX + Math.Cos(orbit.Angle) * orbit.Radius;
        double y = centerY + Math.Sin(orbit.Angle) * orbit.Radius;
        var duration = TimeSpan.FromMilliseconds(250);

        // Reduced from 8 to 4 particles
        for (int i = 0; i < 4; i++)
        {
            var particle = new Ellipse { Width = 5, Height = 5, Fill = GreenBrush };
            Canvas.SetLeft(particle, x);
            Canvas.SetTop(particle, y);
            ClickCanvas.Children.Add(particle);

            double angle = _random.NextDouble() * Math.PI * 2;
            double distance = 15 + _random.NextDouble() * 15;

            var transform = new TranslateTransform();
            particle.RenderTransform = transform;

            var xAnim = new DoubleAnimation { To = Math.Cos(angle) * distance, Duration = duration };
            var yAnim = new DoubleAnimation { To = Math.Sin(angle) * distance, Duration = duration };
            var fadeAnim = new DoubleAnimation { To = 0, Duration = duration };

            fadeAnim.Completed += (s, e) => ClickCanvas.Children.Remove(particle);

            transform.BeginAnimation(TranslateTransform.XProperty, xAnim);
            transform.BeginAnimation(TranslateTransform.YProperty, yAnim);
            particle.BeginAnimation(OpacityProperty, fadeAnim);
        }
    }

    private void UpdateAmbientParticles(double deltaTime)
    {
        double width = AmbientCanvas.ActualWidth;
        double height = AmbientCanvas.ActualHeight;

        if (width <= 0 || height <= 0) return;

        // Spawn new particles (reduced rate)
        _ambientSpawnTimer += deltaTime;
        if (_ambientSpawnTimer > 2.5 && _ambientParticles.Count < 20)
        {
            _ambientSpawnTimer = 0;
            SpawnAmbientParticle(width, height);
        }

        // Update existing particles using reusable list
        _ambientToRemove.Clear();
        foreach (var p in _ambientParticles)
        {
            p.Life -= deltaTime;
            p.X += p.VelocityX * deltaTime;
            p.Y += p.VelocityY * deltaTime;
            p.Rotation += p.RotationSpeed * deltaTime;

            if (p.Life <= 0)
            {
                _ambientToRemove.Add(p);
                continue;
            }

            Canvas.SetLeft(p.Element, p.X);
            Canvas.SetTop(p.Element, p.Y);

            // Fade based on life
            double lifeFraction = p.Life / p.MaxLife;
            double opacity = lifeFraction < 0.2 ? lifeFraction * 5 * 0.3 :
                             lifeFraction > 0.8 ? (1 - lifeFraction) * 5 * 0.3 : 0.3;
            p.Element.Opacity = opacity;

            // Use cached transform instead of creating new one
            p.CachedTransform.Angle = p.Rotation * 30;
        }

        foreach (var p in _ambientToRemove)
        {
            AmbientCanvas.Children.Remove(p.Element);
            _ambientParticles.Remove(p);
        }
    }

    private void SpawnAmbientParticle(double width, double height)
    {
        string iconKey = AmbientIconKeys[_random.Next(AmbientIconKeys.Length)];
        var rotateTransform = new RotateTransform();
        double size = 14 + _random.Next(8);

        var element = new System.Windows.Controls.Image
        {
            Source = (System.Windows.Media.ImageSource)Application.Current.FindResource(iconKey),
            Width = size,
            Height = size,
            Opacity = 0.3,
            RenderTransformOrigin = new Point(0.5, 0.5),
            RenderTransform = rotateTransform
        };

        double maxLife = 12 + _random.NextDouble() * 8;

        var particle = new AmbientParticle
        {
            Element = element,
            CachedTransform = rotateTransform,
            X = _random.NextDouble() * width,
            Y = _random.NextDouble() * height,
            VelocityX = (_random.NextDouble() - 0.5) * 8,
            VelocityY = -5 - _random.NextDouble() * 10, // Drift upward
            Life = maxLife,
            MaxLife = maxLife,
            Rotation = _random.NextDouble() * 360,
            RotationSpeed = (_random.NextDouble() - 0.5) * 2
        };

        Canvas.SetLeft(element, particle.X);
        Canvas.SetTop(element, particle.Y);
        AmbientCanvas.Children.Add(element);
        _ambientParticles.Add(particle);
    }

    // === STARFIELD ===
    private void UpdateStarfield(double deltaTime)
    {
        double width = AmbientCanvas.ActualWidth;
        double height = AmbientCanvas.ActualHeight;

        if (width <= 0 || height <= 0) return;

        // Initialize starfield once
        if (!_starfieldInitialized)
        {
            InitializeStarfield(width, height);
            _starfieldInitialized = true;
        }

        // Update star twinkling
        foreach (var star in _stars)
        {
            star.TwinklePhase += star.TwinkleSpeed * deltaTime;
            double twinkle = 0.5 + 0.5 * Math.Sin(star.TwinklePhase);
            star.Element.Opacity = star.BaseOpacity * (0.3 + 0.7 * twinkle);
        }
    }

    private void InitializeStarfield(double width, double height)
    {
        // Create 40-60 stars scattered across the background
        int starCount = 40 + _random.Next(20);

        for (int i = 0; i < starCount; i++)
        {
            double size = 1 + _random.NextDouble() * 2.5;
            double baseOpacity = 0.3 + _random.NextDouble() * 0.5;

            var starElement = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = _random.NextDouble() < 0.7
                    ? new SolidColorBrush(Color.FromRgb(255, 255, 255))
                    : new SolidColorBrush(Color.FromRgb(200, 220, 255)),
                Opacity = baseOpacity,
                IsHitTestVisible = false
            };

            double x = _random.NextDouble() * width;
            double y = _random.NextDouble() * height;

            Canvas.SetLeft(starElement, x);
            Canvas.SetTop(starElement, y);
            AmbientCanvas.Children.Insert(0, starElement); // Behind other elements

            _stars.Add(new Star
            {
                Element = starElement,
                TwinklePhase = _random.NextDouble() * Math.PI * 2,
                TwinkleSpeed = 1 + _random.NextDouble() * 3,
                BaseOpacity = baseOpacity
            });
        }
    }

    // === MYSTICAL SPARKS ===
    private void UpdateMysticalSparks(double deltaTime)
    {
        double width = AmbientCanvas.ActualWidth;
        double height = AmbientCanvas.ActualHeight;

        if (width <= 0 || height <= 0) return;

        double centerX = width / 2;
        double centerY = height / 2;

        // Spawn new sparks
        _sparkSpawnTimer += deltaTime;
        if (_sparkSpawnTimer > 0.3 && _mysticalSparks.Count < 25)
        {
            _sparkSpawnTimer = 0;
            SpawnMysticalSpark(centerX, centerY);
        }

        // Update existing sparks
        _sparksToRemove.Clear();
        foreach (var spark in _mysticalSparks)
        {
            spark.Life -= deltaTime;
            spark.PulsePhase += deltaTime * 5;
            spark.Angle += spark.Speed * deltaTime;
            spark.Distance += deltaTime * 15; // Slowly drift outward

            if (spark.Life <= 0 || spark.Distance > 200)
            {
                _sparksToRemove.Add(spark);
                continue;
            }

            // Calculate position in orbit
            double x = centerX + Math.Cos(spark.Angle) * spark.Distance - 2;
            double y = centerY + Math.Sin(spark.Angle) * spark.Distance - 2;

            Canvas.SetLeft(spark.Element, x);
            Canvas.SetTop(spark.Element, y);

            // Pulsing glow effect
            double pulse = 0.5 + 0.5 * Math.Sin(spark.PulsePhase);
            double lifeFade = Math.Min(1, spark.Life * 2);
            spark.Element.Opacity = (0.4 + 0.4 * pulse) * lifeFade;
        }

        foreach (var spark in _sparksToRemove)
        {
            AmbientCanvas.Children.Remove(spark.Element);
            _mysticalSparks.Remove(spark);
        }
    }

    private void SpawnMysticalSpark(double centerX, double centerY)
    {
        // Random color - green, gold, or purple energy
        Color sparkColor = _random.Next(3) switch
        {
            0 => Color.FromRgb(0, 255, 100),   // Green
            1 => Color.FromRgb(255, 215, 0),   // Gold
            _ => Color.FromRgb(180, 100, 255)  // Purple
        };

        var spark = new Ellipse
        {
            Width = 4,
            Height = 4,
            Fill = new RadialGradientBrush(sparkColor, Colors.Transparent),
            IsHitTestVisible = false
        };

        double angle = _random.NextDouble() * Math.PI * 2;
        double startDistance = 60 + _random.NextDouble() * 40;

        Canvas.SetLeft(spark, centerX + Math.Cos(angle) * startDistance);
        Canvas.SetTop(spark, centerY + Math.Sin(angle) * startDistance);
        AmbientCanvas.Children.Add(spark);

        _mysticalSparks.Add(new MysticalSpark
        {
            Element = spark,
            Angle = angle,
            Distance = startDistance,
            Speed = (_random.NextDouble() - 0.5) * 1.5,
            Life = 3 + _random.NextDouble() * 4,
            PulsePhase = _random.NextDouble() * Math.PI * 2
        });
    }

    // === ORBITING RUNES ===
    private void UpdateOrbitingRunes(double deltaTime)
    {
        double width = AmbientCanvas.ActualWidth;
        double height = AmbientCanvas.ActualHeight;

        if (width <= 0 || height <= 0) return;

        double centerX = width / 2;
        double centerY = height / 2;

        // Initialize runes based on conspiracy count
        int targetRuneCount = Math.Min(_engine.State.ProvenConspiracies.Count, 8);

        if (!_runesInitialized || _orbitingRunes.Count < targetRuneCount)
        {
            while (_orbitingRunes.Count < targetRuneCount)
            {
                SpawnOrbitingRune(_orbitingRunes.Count);
            }
            _runesInitialized = true;
        }

        // Update rune positions
        foreach (var rune in _orbitingRunes)
        {
            rune.Angle += rune.Speed * deltaTime;
            rune.PulsePhase += deltaTime * 2;

            double x = centerX + Math.Cos(rune.Angle) * rune.Radius - 8;
            double y = centerY + Math.Sin(rune.Angle) * rune.Radius - 8;

            Canvas.SetLeft(rune.Element, x);
            Canvas.SetTop(rune.Element, y);

            // Gentle pulsing
            double pulse = 0.6 + 0.4 * Math.Sin(rune.PulsePhase);
            rune.Element.Opacity = 0.4 * pulse;
        }
    }

    private void SpawnOrbitingRune(int index)
    {
        string symbol = RuneSymbols[index % RuneSymbols.Length];

        var rune = new TextBlock
        {
            Text = symbol,
            FontSize = 16,
            Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 100)),
            Opacity = 0.4,
            IsHitTestVisible = false
        };

        // Distribute evenly around the circle, with varying radii
        double baseAngle = (2 * Math.PI / 8) * index;
        double radius = 140 + (index % 3) * 25; // Varying orbits

        AmbientCanvas.Children.Add(rune);

        _orbitingRunes.Add(new OrbitingRune
        {
            Element = rune,
            Angle = baseAngle,
            Radius = radius,
            Speed = 0.15 + (index % 2) * 0.1, // Alternate speeds
            PulsePhase = _random.NextDouble() * Math.PI * 2
        });
    }

    // === MENU BACKGROUND ===
    private void UpdateMenuBackground(double deltaTime)
    {
        double width = MenuBackgroundCanvas.ActualWidth;
        double height = MenuBackgroundCanvas.ActualHeight;

        if (width <= 0 || height <= 0) return;

        // Initialize menu background once
        if (!_menuBackgroundInitialized)
        {
            InitializeMenuBackground(width, height);
            _menuBackgroundInitialized = true;
        }

        // Slowly rotate background pyramid
        _menuPyramidAngle += deltaTime * 3; // 3 degrees per second
        MenuPyramidRotation.Angle = _menuPyramidAngle;

        // Update star twinkling
        foreach (var star in _menuStars)
        {
            star.TwinklePhase += star.TwinkleSpeed * deltaTime;
            double twinkle = 0.5 + 0.5 * Math.Sin(star.TwinklePhase);
            star.Element.Opacity = star.BaseOpacity * (0.3 + 0.7 * twinkle);
        }

        // Spawn floating particles
        _menuParticleSpawnTimer += deltaTime;
        if (_menuParticleSpawnTimer > 0.4 && _menuParticles.Count < 30)
        {
            _menuParticleSpawnTimer = 0;
            SpawnMenuParticle(width, height);
        }

        // Update particles
        _menuParticlesToRemove.Clear();
        foreach (var p in _menuParticles)
        {
            p.Life -= deltaTime;
            p.X += p.VelocityX * deltaTime;
            p.Y += p.VelocityY * deltaTime;
            p.PulsePhase += deltaTime * 4;

            if (p.Life <= 0)
            {
                _menuParticlesToRemove.Add(p);
                continue;
            }

            Canvas.SetLeft(p.Element, p.X);
            Canvas.SetTop(p.Element, p.Y);

            // Fade in/out with pulsing
            double lifeFraction = p.Life / p.MaxLife;
            double fadeMultiplier = lifeFraction < 0.2 ? lifeFraction * 5 :
                                    lifeFraction > 0.8 ? (1 - lifeFraction) * 5 : 1.0;
            double pulse = 0.6 + 0.4 * Math.Sin(p.PulsePhase);
            p.Element.Opacity = 0.6 * fadeMultiplier * pulse;
        }

        foreach (var p in _menuParticlesToRemove)
        {
            MenuBackgroundCanvas.Children.Remove(p.Element);
            _menuParticles.Remove(p);
        }
    }

    private void InitializeMenuBackground(double width, double height)
    {
        // Create starfield for menu - more stars, denser
        int starCount = 80 + _random.Next(40);

        for (int i = 0; i < starCount; i++)
        {
            double size = 1 + _random.NextDouble() * 3;
            double baseOpacity = 0.2 + _random.NextDouble() * 0.6;

            // Varied colors - white, blue, gold, green tints
            Color starColor = _random.Next(10) switch
            {
                0 or 1 => Color.FromRgb(200, 220, 255),  // Blue-white
                2 => Color.FromRgb(255, 230, 180),       // Warm white
                3 => Color.FromRgb(180, 255, 200),       // Green tint
                4 => Color.FromRgb(255, 215, 100),       // Gold tint
                _ => Color.FromRgb(255, 255, 255)        // Pure white
            };

            var starElement = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = new SolidColorBrush(starColor),
                Opacity = baseOpacity,
                IsHitTestVisible = false
            };

            double x = _random.NextDouble() * width;
            double y = _random.NextDouble() * height;

            Canvas.SetLeft(starElement, x);
            Canvas.SetTop(starElement, y);
            MenuBackgroundCanvas.Children.Add(starElement);

            _menuStars.Add(new MenuStar
            {
                Element = starElement,
                TwinklePhase = _random.NextDouble() * Math.PI * 2,
                TwinkleSpeed = 0.5 + _random.NextDouble() * 4,
                BaseOpacity = baseOpacity
            });
        }
    }

    private void SpawnMenuParticle(double width, double height)
    {
        // Create glowing orb particles that float upward
        Color particleColor = _random.Next(4) switch
        {
            0 => Color.FromRgb(0, 255, 100),    // Green
            1 => Color.FromRgb(255, 215, 0),    // Gold
            2 => Color.FromRgb(150, 100, 255),  // Purple
            _ => Color.FromRgb(100, 200, 255)   // Cyan
        };

        double size = 3 + _random.NextDouble() * 5;

        var particle = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new RadialGradientBrush(particleColor, Colors.Transparent),
            IsHitTestVisible = false
        };

        double maxLife = 6 + _random.NextDouble() * 6;

        var p = new MenuParticle
        {
            Element = particle,
            X = _random.NextDouble() * width,
            Y = height + 10, // Start from bottom
            VelocityX = (_random.NextDouble() - 0.5) * 30,
            VelocityY = -20 - _random.NextDouble() * 40, // Float upward
            Life = maxLife,
            MaxLife = maxLife,
            PulsePhase = _random.NextDouble() * Math.PI * 2
        };

        Canvas.SetLeft(particle, p.X);
        Canvas.SetTop(particle, p.Y);
        MenuBackgroundCanvas.Children.Add(particle);
        _menuParticles.Add(p);
    }

    private void InitializeGeneratorButtons()
    {
        var buttonStyle = (Style)FindResource("GeneratorButton");

        foreach (var gen in GeneratorData.AllGenerators)
        {
            var (container, button, upgradePanel) = CreateGeneratorButton(gen, buttonStyle);
            GeneratorPanel.Children.Add(container);
            _generatorButtons[gen.Id] = button;
            _generatorContainers[gen.Id] = container;
            _generatorUpgradePanels[gen.Id] = upgradePanel;
        }
    }

    private (StackPanel container, Button button, WrapPanel upgradePanel) CreateGeneratorButton(Generator gen, Style buttonStyle)
    {
        // Container holds button and upgrade panel as siblings (so upgrade hover doesn't highlight button)
        var container = new StackPanel();

        var button = new Button
        {
            Style = buttonStyle,
            Tag = gen.Id,
            HorizontalContentAlignment = HorizontalAlignment.Stretch
        };
        button.Click += GeneratorButton_Click;

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var fallbackText = IconData.GeneratorIcons.TryGetValue(gen.Id, out var icon) ? icon : "?";
        var iconImage = IconHelper.CreateIconWithFallback(gen.Id, fallbackText, 76, GreenBrush);

        // Wrap icon in a border with rounded corners and drop shadow
        var iconBorder = new Border
        {
            Width = 80,
            Height = 80,
            CornerRadius = new CornerRadius(8),
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(60, 0, 255, 65)),
            BorderThickness = new Thickness(1),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 15, 0),
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 10,
                ShadowDepth = 3,
                Opacity = 0.5,
                Direction = 315
            }
        };

        // If it's an Image, use ImageBrush as background to clip to rounded corners
        if (iconImage is Image img && img.Source != null)
        {
            iconBorder.Background = new ImageBrush
            {
                ImageSource = img.Source,
                Stretch = Stretch.Uniform,
                AlignmentX = AlignmentX.Center,
                AlignmentY = AlignmentY.Center
            };
        }
        else
        {
            // For TextBlock fallback, keep as child
            iconBorder.Child = iconImage;
            iconImage.SetValue(MarginProperty, new Thickness(2));
        }

        var iconElement = iconBorder;

        var leftStack = new StackPanel();
        leftStack.Children.Add(new TextBlock { Text = gen.Name, FontWeight = FontWeights.Bold, Foreground = GreenBrush, FontSize = 21 });
        leftStack.Children.Add(new TextBlock { Text = gen.FlavorText, FontSize = 15, Foreground = DimBrush, TextWrapping = TextWrapping.Wrap, MaxWidth = 375 });
        leftStack.Children.Add(new TextBlock { Tag = "prod", FontSize = 17, Foreground = LightBrush, Margin = new Thickness(0, 5, 0, 0) });
        if (gen.BelieverBonus > 0)
            leftStack.Children.Add(new TextBlock { Text = $"+{NumberFormatter.FormatInteger(gen.BelieverBonus)} believers each", FontSize = 15, Foreground = GoldBrush });

        var rightStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(15, 0, 0, 0) };
        rightStack.Children.Add(new TextBlock { Tag = "cost", FontWeight = FontWeights.Bold, Foreground = GoldBrush, HorizontalAlignment = HorizontalAlignment.Right, FontSize = 21 });
        rightStack.Children.Add(new TextBlock { Tag = "owned", FontSize = 17, Foreground = DimBrush, HorizontalAlignment = HorizontalAlignment.Right });

        Grid.SetColumn(iconElement, 0);
        Grid.SetColumn(leftStack, 1);
        Grid.SetColumn(rightStack, 2);
        grid.Children.Add(iconElement);
        grid.Children.Add(leftStack);
        grid.Children.Add(rightStack);

        button.Content = grid;
        container.Children.Add(button);

        // Upgrade panel for generator-specific upgrades - OUTSIDE the button so hover doesn't highlight button
        var upgradePanel = new WrapPanel { Tag = "upgrades", Margin = new Thickness(17, 0, 17, 8), Visibility = Visibility.Collapsed };
        container.Children.Add(upgradePanel);

        return (container, button, upgradePanel);
    }

    private void ClickButton_Click(object sender, RoutedEventArgs e)
    {
        // Check for temporary click boost from lucky drops
        double boostMultiplier = 1.0;
        if (DateTime.Now < _clickBoostEnd)
            boostMultiplier = _clickBoostMultiplier;

        _engine.ProcessClick(externalMultiplier: boostMultiplier);
        if (_random.Next(5) == 0)
            FlavorTextDisplay.Text = FlavorText.GetRandomClickMessage();
    }

    private void OnClickProcessed(double clickPower, bool isCritical)
    {
        SpawnFloatingNumber(clickPower, isCritical);
        PulseEye();
        SpawnClickParticles();

        if (isCritical)
        {
            TriggerScreenShake();
            SoundManager.Play("crit");
        }
        else
        {
            SoundManager.Play("click");
        }
    }

    private void TriggerScreenShake()
    {
        var transform = EyeContainer.RenderTransform as TranslateTransform ?? new TranslateTransform();
        EyeContainer.RenderTransform = transform;

        double shakeAmount = 4;
        var shakeX = new DoubleAnimation
        {
            From = -shakeAmount,
            To = shakeAmount,
            Duration = TimeSpan.FromMilliseconds(50),
            AutoReverse = true,
            RepeatBehavior = new RepeatBehavior(2)
        };
        var shakeY = new DoubleAnimation
        {
            From = -shakeAmount / 2,
            To = shakeAmount / 2,
            Duration = TimeSpan.FromMilliseconds(50),
            AutoReverse = true,
            RepeatBehavior = new RepeatBehavior(2)
        };

        shakeX.Completed += (s, e) => transform.X = 0;
        shakeY.Completed += (s, e) => transform.Y = 0;

        transform.BeginAnimation(TranslateTransform.XProperty, shakeX);
        transform.BeginAnimation(TranslateTransform.YProperty, shakeY);
    }

    private void SpawnFloatingNumber(double amount, bool isCritical = false)
    {
        var text = new TextBlock
        {
            Text = isCritical ? $"CRIT! +{NumberFormatter.Format(amount)}" : $"+{NumberFormatter.Format(amount)}",
            FontSize = isCritical ? 20 : 16,
            FontWeight = FontWeights.Bold,
            Foreground = isCritical ? CritBrush : (_engine.State.GoldenEyeActive ? GoldBrush : GreenBrush)
        };

        double centerX = ClickCanvas.ActualWidth / 2;
        double centerY = ClickCanvas.ActualHeight / 2;
        double offsetX = (_random.NextDouble() - 0.5) * 60;

        Canvas.SetLeft(text, centerX + offsetX - 30);
        Canvas.SetTop(text, centerY - 40);
        ClickCanvas.Children.Add(text);

        var translateAnim = new DoubleAnimation
        {
            From = 0, To = -80,
            Duration = TimeSpan.FromMilliseconds(800),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var fadeAnim = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(800) };
        var transform = new TranslateTransform();
        text.RenderTransform = transform;

        fadeAnim.Completed += (s, e) => ClickCanvas.Children.Remove(text);

        transform.BeginAnimation(TranslateTransform.YProperty, translateAnim);
        text.BeginAnimation(OpacityProperty, fadeAnim);
    }

    private void PulseEye()
    {
        // Get current combo-based scale and add a click bump on top
        double comboProgress = Math.Min(_engine.State.ComboMeter, 1.0);
        double baseScale = 1.0 + 0.25 * comboProgress;
        double clickScale = baseScale + 0.1; // Add 0.1 bump on click

        // Quick scale up then back to combo-based scale
        var scaleAnim = new DoubleAnimation
        {
            From = clickScale,
            To = baseScale,
            Duration = TimeSpan.FromMilliseconds(150),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        EyeScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
        EyeScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);

        // Pulse the glow effect brighter momentarily
        double currentBlur = 20 + 20 * comboProgress;
        var glowAnim = new DoubleAnimation { From = currentBlur + 15, To = currentBlur, Duration = TimeSpan.FromMilliseconds(200) };
        EyeGlow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, glowAnim);
    }

    // Cached easing function for particles
    private static readonly QuadraticEase ParticleEase = new() { EasingMode = EasingMode.EaseOut };

    private void SpawnClickParticles()
    {
        double centerX = ClickCanvas.ActualWidth / 2;
        double centerY = ClickCanvas.ActualHeight / 2;
        bool isGolden = _engine.State.GoldenEyeActive;
        var brush = isGolden ? GoldBrush : GreenBrush;

        // Reduced particle count for performance (6-8 instead of 10-15)
        int particleCount = 6 + _random.Next(3);

        for (int i = 0; i < particleCount; i++)
        {
            double size = 4 + _random.NextDouble() * 5;
            var particle = new Ellipse { Width = size, Height = size, Fill = brush };
            Canvas.SetLeft(particle, centerX - size / 2);
            Canvas.SetTop(particle, centerY - size / 2);
            ClickCanvas.Children.Add(particle);

            double angle = _random.NextDouble() * Math.PI * 2;
            double distance = 40 + _random.NextDouble() * 60;
            var duration = TimeSpan.FromMilliseconds(350);

            var transform = new TranslateTransform();
            particle.RenderTransform = transform;

            var xAnim = new DoubleAnimation { To = Math.Cos(angle) * distance, Duration = duration, EasingFunction = ParticleEase };
            var yAnim = new DoubleAnimation { To = Math.Sin(angle) * distance, Duration = duration, EasingFunction = ParticleEase };
            var fadeAnim = new DoubleAnimation { To = 0, Duration = duration };

            fadeAnim.Completed += (s, e) => ClickCanvas.Children.Remove(particle);

            transform.BeginAnimation(TranslateTransform.XProperty, xAnim);
            transform.BeginAnimation(TranslateTransform.YProperty, yAnim);
            particle.BeginAnimation(OpacityProperty, fadeAnim);
        }
    }

    private void OnComboBurst(double amount)
    {
        SoundManager.Play("combo");

        // BIG POP effect - eye scales up dramatically then back to normal
        // First cancel any running animations
        EyeScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        EyeScale.BeginAnimation(ScaleTransform.ScaleYProperty, null);

        // Pop animation: current size -> 1.5x -> 1.0x
        var popAnim = new DoubleAnimation
        {
            From = 1.4,  // Big pop
            To = 1.0,    // Back to normal
            Duration = TimeSpan.FromMilliseconds(400),
            EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 2, Springiness = 3 }
        };
        EyeScale.BeginAnimation(ScaleTransform.ScaleXProperty, popAnim);
        EyeScale.BeginAnimation(ScaleTransform.ScaleYProperty, popAnim);

        // Flash the glow bright white/gold
        EyeGlow.Color = Colors.White;
        var glowColorAnim = new ColorAnimation { From = Colors.White, To = Color.FromRgb(0, 255, 65), Duration = TimeSpan.FromMilliseconds(500) };
        EyeGlow.BeginAnimation(DropShadowEffect.ColorProperty, glowColorAnim);

        var glowSizeAnim = new DoubleAnimation { From = 50, To = 20, Duration = TimeSpan.FromMilliseconds(400) };
        EyeGlow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, glowSizeAnim);

        // Flash the ambient glow too
        EyeAmbientGlow.Fill = new RadialGradientBrush(Colors.Gold, Colors.Transparent);
        var ambientOpacityAnim = new DoubleAnimation { From = 0.8, To = 0.2, Duration = TimeSpan.FromMilliseconds(500) };
        EyeAmbientGlow.BeginAnimation(OpacityProperty, ambientOpacityAnim);
        var ambientSizeAnim = new DoubleAnimation { From = 220, To = 160, Duration = TimeSpan.FromMilliseconds(500) };
        EyeAmbientGlow.BeginAnimation(WidthProperty, ambientSizeAnim);
        EyeAmbientGlow.BeginAnimation(HeightProperty, ambientSizeAnim);

        // Combo text
        var text = new TextBlock { Text = $"COMBO! +{NumberFormatter.Format(amount)}", FontSize = 24, FontWeight = FontWeights.Bold, Foreground = GoldBrush };
        double centerX = ClickCanvas.ActualWidth / 2;
        double centerY = ClickCanvas.ActualHeight / 2;

        Canvas.SetLeft(text, centerX - 80);
        Canvas.SetTop(text, centerY - 60);
        ClickCanvas.Children.Add(text);

        var scaleTransform = new ScaleTransform(0.5, 0.5, 80, 12);
        text.RenderTransform = scaleTransform;

        var textScaleAnim = new DoubleAnimation { From = 0.5, To = 1.5, Duration = TimeSpan.FromMilliseconds(600), EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
        var fadeAnim = new DoubleAnimation { From = 1, To = 0, BeginTime = TimeSpan.FromMilliseconds(400), Duration = TimeSpan.FromMilliseconds(400) };

        fadeAnim.Completed += (s, e) => ClickCanvas.Children.Remove(text);

        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, textScaleAnim);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, textScaleAnim);
        text.BeginAnimation(OpacityProperty, fadeAnim);

        // Explosion of gold particles from the eye
        for (int i = 0; i < 30; i++)
        {
            var particle = new Ellipse { Width = 8, Height = 8, Fill = new SolidColorBrush(Color.FromRgb((byte)_random.Next(200, 256), (byte)_random.Next(180, 220), (byte)_random.Next(0, 50))) };
            Canvas.SetLeft(particle, centerX - 4);
            Canvas.SetTop(particle, centerY - 4);
            ClickCanvas.Children.Add(particle);

            double angle = _random.NextDouble() * Math.PI * 2;
            double distance = 80 + _random.NextDouble() * 100;

            var transform = new TranslateTransform();
            particle.RenderTransform = transform;

            var xAnim = new DoubleAnimation { From = 0, To = Math.Cos(angle) * distance, Duration = TimeSpan.FromMilliseconds(500), EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
            var yAnim = new DoubleAnimation { From = 0, To = Math.Sin(angle) * distance, Duration = TimeSpan.FromMilliseconds(500), EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
            var particleFade = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(500) };

            particleFade.Completed += (s, e) => ClickCanvas.Children.Remove(particle);

            transform.BeginAnimation(TranslateTransform.XProperty, xAnim);
            transform.BeginAnimation(TranslateTransform.YProperty, yAnim);
            particle.BeginAnimation(OpacityProperty, particleFade);
        }

        FlavorTextDisplay.Text = "TRUTH OVERLOAD!";
        AddNotification("Combo burst! +" + NumberFormatter.Format(amount) + " evidence", GoldBrush);
    }

    private void OnGoldenEyeStart()
    {
        // Ignore golden eye in zen mode
        if (_zenMode)
        {
            _engine.State.GoldenEyeActive = false;
            return;
        }

        // Switch to golden eye (keep pyramid at current level, just make it golden)
        var goldenEye = PyramidSpriteSheetLoader.GetEyeIcon(pyramidLevel: _currentPyramidLevel, forceGolden: true);
        if (goldenEye != null) EyeImage.Source = goldenEye;
        EyeGlow.Color = Colors.Gold;
        PyramidGlow.Color = Colors.Gold;
        PyramidGlow.Opacity = 0.8;
        FlavorTextDisplay.Text = "GOLDEN EYE! 10x clicks for 10 seconds!";
        ShowToast("GOLDEN EYE!", "10x click power for 10 seconds!");

        // Pulsing glow animation for both eye and pyramid
        var glowAnim = new DoubleAnimation { From = 20, To = 35, Duration = TimeSpan.FromMilliseconds(500), AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever };
        EyeGlow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, glowAnim);
        var pyramidGlowAnim = new DoubleAnimation { From = 15, To = 30, Duration = TimeSpan.FromMilliseconds(500), AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever };
        PyramidGlow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, pyramidGlowAnim);
    }

    private void OnGoldenEyeEnd()
    {
        // Switch back to tier-appropriate eye based on current pyramid level
        var normalEye = PyramidSpriteSheetLoader.GetEyeIcon(pyramidLevel: _currentPyramidLevel);
        if (normalEye != null) EyeImage.Source = normalEye;

        // Restore glow colors based on current pyramid level (matches eye tier thresholds)
        Color glowColor = _currentPyramidLevel switch
        {
            <= 11 => Color.FromRgb(0, 255, 65),      // Green (eye_basic)
            <= 17 => Color.FromRgb(200, 220, 100),   // Yellow-green (eye_golden)
            <= 23 => Color.FromRgb(150, 100, 255),   // Purple (eye_cosmic)
            _ => Color.FromRgb(220, 150, 255),       // Light purple/pink (eye_omega)
        };

        EyeGlow.Color = glowColor;
        EyeGlow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, null);
        EyeGlow.BlurRadius = 20;

        PyramidGlow.Color = glowColor;
        PyramidGlow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, null);
        PyramidGlow.BlurRadius = 15;
        PyramidGlow.Opacity = 0.4;

        EyeAmbientGlow.Fill = new RadialGradientBrush(glowColor, Colors.Transparent);

        FlavorTextDisplay.Text = FlavorText.GetRandomClickMessage();
    }

    private void OnQuestComplete(string questId, bool success, double evidence, long tinfoil)
    {
        var quest = QuestData.GetById(questId);
        if (quest == null) return;

        string message;
        SolidColorBrush color;

        if (success)
        {
            message = $"Quest '{quest.Name}' SUCCESS! +{NumberFormatter.Format(evidence)} evidence";
            if (tinfoil > 0) message += $" +{tinfoil} tinfoil";
            color = GreenBrush;
            ShowToast("SUCCESS!", message);
        }
        else
        {
            message = quest.Risk switch
            {
                QuestRisk.Low => $"Quest '{quest.Name}' failed. Partial reward: +{NumberFormatter.Format(evidence)} evidence",
                QuestRisk.Medium => $"Quest '{quest.Name}' failed. No rewards.",
                QuestRisk.High => $"Quest '{quest.Name}' FAILED! Believers detained!",
                _ => $"Quest '{quest.Name}' ended."
            };
            color = quest.Risk == QuestRisk.High ? RedBrush : OrangeBrush;
            ShowToast("FAILED", message);
        }

        AddNotification(message, color);
        FlavorTextDisplay.Text = message;
    }

    private void OnPrestigeAvailable()
    {
        PrestigeTeaser.Visibility = Visibility.Visible;
        ShowToast("PRESTIGE", "The Illuminati awaits...");
        AddNotification("Prestige is now available! The Illuminati awaits...", PurpleBrush);
    }

    private void ShowToast(string icon, string message, double displaySeconds = 3)
    {
        ToastIcon.Text = icon;
        ToastMessage.Text = message;

        // Stop any existing animation and reset
        ToastNotification.BeginAnimation(OpacityProperty, null);
        ToastNotification.Opacity = 0;
        ToastNotification.Visibility = Visibility.Visible;

        // Create storyboard with fade in, hold, fade out
        var storyboard = new Storyboard();

        // Fade in over 0.5 seconds
        var fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(500)
        };
        Storyboard.SetTarget(fadeIn, ToastNotification);
        Storyboard.SetTargetProperty(fadeIn, new PropertyPath(OpacityProperty));
        storyboard.Children.Add(fadeIn);

        // Fade out after display time
        var fadeOut = new DoubleAnimation
        {
            From = 1,
            To = 0,
            BeginTime = TimeSpan.FromSeconds(0.5 + displaySeconds), // After fade in + display
            Duration = TimeSpan.FromMilliseconds(500)
        };
        Storyboard.SetTarget(fadeOut, ToastNotification);
        Storyboard.SetTargetProperty(fadeOut, new PropertyPath(OpacityProperty));
        storyboard.Children.Add(fadeOut);

        storyboard.Completed += (s, e) =>
        {
            ToastNotification.Visibility = Visibility.Collapsed;
        };

        storyboard.Begin();
    }

    private void AddNotification(string message, SolidColorBrush color)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        _notificationLog.Insert(0, $"[{timestamp}] {message}");
        if (_notificationLog.Count > MAX_NOTIFICATIONS)
            _notificationLog.RemoveAt(_notificationLog.Count - 1);

        NotificationLogPanel.Children.Clear();
        for (int i = 0; i < _notificationLog.Count; i++)
        {
            NotificationLogPanel.Children.Add(new TextBlock
            {
                Text = _notificationLog[i],
                Foreground = i == 0 ? color : DimBrush,
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 0, 2)
            });
        }
    }

    private void TinfoilShopButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string upgradeId)
            _engine.PurchaseTinfoilUpgrade(upgradeId);
    }

    private void QuestButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string questId)
            _engine.StartQuest(questId);
    }

    private void AutoQuestToggle_Click(object sender, RoutedEventArgs e)
    {
        _engine.ToggleAutoQuest();
        UpdateAutoQuestToggle();
    }

    private void UpdateAutoQuestToggle()
    {
        bool hasAutoQuest = _engine.HasAutoQuest();
        AutoQuestToggle.Visibility = hasAutoQuest ? Visibility.Visible : Visibility.Collapsed;

        if (hasAutoQuest)
        {
            bool enabled = _engine.IsAutoQuestEnabled;
            AutoQuestToggle.Content = enabled ? "Auto: ON" : "Auto: OFF";
            AutoQuestToggle.Background = enabled ? new SolidColorBrush(Color.FromRgb(34, 85, 34)) : new SolidColorBrush(Color.FromRgb(85, 34, 34));
            AutoQuestToggle.Foreground = enabled ? GreenBrush : RedBrush;
            AutoQuestToggle.BorderBrush = enabled ? GreenBrush : RedBrush;
        }
    }

    private void GeneratorButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string genId)
        {
            int beforeCount = _engine.State.GetGeneratorCount(genId);

            if (_buyMode == -1)
            {
                _engine.PurchaseMaxGenerators(genId);
            }
            else
            {
                for (int i = 0; i < _buyMode; i++)
                    if (!_engine.PurchaseGenerator(genId)) break;
            }

            int afterCount = _engine.State.GetGeneratorCount(genId);
            if (afterCount > beforeCount)
            {
                SpawnPurchaseEffect(btn, afterCount - beforeCount);
                SoundManager.Play("purchase");
            }
        }
    }

    private void SpawnPurchaseEffect(Button button, int count)
    {
        // Spawn sparkles at eye center
        double centerX = ClickCanvas.ActualWidth / 2;
        double centerY = ClickCanvas.ActualHeight / 2;

        // Green pulse on eye - scale up then back to current combo-based size
        double comboProgress = Math.Min(_engine.State.ComboMeter, 1.0);
        double baseScale = 1.0 + 0.25 * comboProgress;
        var pulseAnim = new DoubleAnimation { From = baseScale + 0.15, To = baseScale, Duration = TimeSpan.FromMilliseconds(200) };
        EyeScale.BeginAnimation(ScaleTransform.ScaleXProperty, pulseAnim);
        EyeScale.BeginAnimation(ScaleTransform.ScaleYProperty, pulseAnim);

        // Gold sparkle shower
        for (int i = 0; i < 15; i++)
        {
            var sparkle = new Ellipse
            {
                Width = 3 + _random.NextDouble() * 4,
                Height = 3 + _random.NextDouble() * 4,
                Fill = new SolidColorBrush(Color.FromRgb(
                    (byte)(200 + _random.Next(55)),
                    (byte)(180 + _random.Next(75)),
                    (byte)_random.Next(100)))
            };

            double startX = centerX + (_random.NextDouble() - 0.5) * 100;
            double startY = centerY - 50 - _random.NextDouble() * 30;

            Canvas.SetLeft(sparkle, startX);
            Canvas.SetTop(sparkle, startY);
            ClickCanvas.Children.Add(sparkle);

            var transform = new TranslateTransform();
            sparkle.RenderTransform = transform;

            double fallDistance = 80 + _random.NextDouble() * 60;
            double drift = (_random.NextDouble() - 0.5) * 40;
            double duration = 400 + _random.NextDouble() * 300;

            var yAnim = new DoubleAnimation { From = 0, To = fallDistance, Duration = TimeSpan.FromMilliseconds(duration), EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn } };
            var xAnim = new DoubleAnimation { From = 0, To = drift, Duration = TimeSpan.FromMilliseconds(duration) };
            var fadeAnim = new DoubleAnimation { From = 1, To = 0, BeginTime = TimeSpan.FromMilliseconds(duration * 0.5), Duration = TimeSpan.FromMilliseconds(duration * 0.5) };

            fadeAnim.Completed += (s, e) => ClickCanvas.Children.Remove(sparkle);

            transform.BeginAnimation(TranslateTransform.YProperty, yAnim);
            transform.BeginAnimation(TranslateTransform.XProperty, xAnim);
            sparkle.BeginAnimation(OpacityProperty, fadeAnim);
        }
    }

    private void BuyMode_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string modeStr)
        {
            _buyMode = modeStr == "max" ? -1 : int.Parse(modeStr);

            // Update button visuals
            BuyMode1.Foreground = _buyMode == 1 ? GreenBrush : DimBrush;
            BuyMode1.BorderBrush = _buyMode == 1 ? GreenBrush : DimBrush;
            BuyMode10.Foreground = _buyMode == 10 ? GreenBrush : DimBrush;
            BuyMode10.BorderBrush = _buyMode == 10 ? GreenBrush : DimBrush;
            BuyMode100.Foreground = _buyMode == 100 ? GreenBrush : DimBrush;
            BuyMode100.BorderBrush = _buyMode == 100 ? GreenBrush : DimBrush;
            BuyModeMax.Foreground = _buyMode == -1 ? GreenBrush : DimBrush;
            BuyModeMax.BorderBrush = _buyMode == -1 ? GreenBrush : DimBrush;
        }
    }

    private void UpgradeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string upgradeId)
        {
            if (_engine.PurchaseUpgrade(upgradeId))
                SoundManager.Play("upgrade");
        }
    }

    private void ConspiracyButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string conspiracyId)
        {
            if (_engine.ProveConspiracy(conspiracyId))
            {
                SoundManager.Play("upgrade");
                UpdatePyramidLevel(); // Immediately update pyramid appearance
            }
        }
    }

    private void UpdateUI()
    {
        var state = _engine.State;

        EvidenceDisplay.Text = $"{NumberFormatter.Format(state.Evidence)} Evidence";
        EpsDisplay.Text = NumberFormatter.FormatPerSecond(_engine.CalculateEvidencePerSecond());
        TinfoilDisplay.Text = NumberFormatter.FormatInteger(state.Tinfoil);
        IlluminatiTokenDisplay.Text = NumberFormatter.FormatInteger(state.IlluminatiTokens);
        TokensPanel.Visibility = state.IlluminatiTokens > 0 ? Visibility.Visible : Visibility.Collapsed;
        BelieversDisplay.Text = NumberFormatter.FormatInteger(state.Believers);
        AvailableBelieversDisplay.Text = NumberFormatter.FormatInteger(state.AvailableBelievers);

        double autoRate = _engine.GetAutoClickRate();
        if (autoRate > 0)
            EpsDisplay.Text += $" (+{autoRate}/s auto)";

        // Combo visual - eye grows bigger and changes color with combo
        double comboProgress = Math.Min(state.ComboMeter, 1.0);

        // Color gradient: green (0%) -> yellow (50%) -> gold (100%)
        Color comboColor;
        if (state.GoldenEyeActive)
        {
            comboColor = Colors.Gold;
        }
        else if (comboProgress < 0.5)
        {
            // Green to Yellow
            double t = comboProgress * 2;
            comboColor = Color.FromRgb(
                (byte)(0 + 255 * t),      // R: 0 -> 255
                (byte)(255),               // G: 255
                (byte)(65 - 65 * t)        // B: 65 -> 0
            );
        }
        else
        {
            // Yellow to Gold
            double t = (comboProgress - 0.5) * 2;
            comboColor = Color.FromRgb(
                (byte)255,                 // R: 255
                (byte)(255 - 40 * t),      // G: 255 -> 215
                (byte)0                    // B: 0
            );
        }

        // Update eye size based on combo (grows from 1.0 to 1.25 as combo builds) + idle pulse
        if (!state.GoldenEyeActive)
        {
            // Add subtle idle pulse (oscillates ±4% over 1.5 seconds)
            double pulsePhase = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) % 3000 / 3000.0;
            double pulseFactor = 1.0 + 0.04 * Math.Sin(pulsePhase * 2 * Math.PI);

            double targetScale = (1.0 + 0.25 * comboProgress) * pulseFactor;
            EyeScale.ScaleX = targetScale;
            EyeScale.ScaleY = targetScale;

            // Update glow effect - color and intensity
            EyeGlow.Color = comboColor;
            EyeGlow.BlurRadius = 20 + 20 * comboProgress;
            EyeGlow.Opacity = 0.6 + 0.4 * comboProgress;

            // Also update ambient glow color and size
            EyeAmbientGlow.Fill = new RadialGradientBrush(comboColor, Colors.Transparent);
            EyeAmbientGlow.Width = 200 + 40 * comboProgress;
            EyeAmbientGlow.Height = 200 + 40 * comboProgress;
            EyeAmbientGlow.Opacity = 0.2 + 0.3 * comboProgress;
        }

        // Update pyramid based on proven conspiracies
        UpdatePyramidLevel();

        // Combo label
        ComboLabel.Text = state.ComboClicks > 0 ? $"x{state.ComboClicks} COMBO" : "";

        TotalEvidenceDisplay.Text = NumberFormatter.Format(state.TotalEvidenceEarned);
        TotalClicksDisplay.Text = NumberFormatter.FormatInteger(state.TotalClicks);

        // Show click power breakdown
        var (clickBase, clickMult, clickEps) = _engine.GetClickPowerBreakdown();
        double clickTotal = (clickBase * clickMult) + clickEps;
        string clickBreakdown = $"{NumberFormatter.Format(clickBase)} × {clickMult:F2}";
        if (clickEps > 0) clickBreakdown += $" + {NumberFormatter.Format(clickEps)}";
        clickBreakdown += $" = {NumberFormatter.Format(clickTotal)}/click";
        ClickPowerDisplay.Text = clickBreakdown;

        // Show EPS breakdown
        var (epsBase, epsMult) = _engine.GetEpsBreakdown();
        double epsTotal = epsBase * epsMult;
        EpsBreakdownDisplay.Text = epsTotal > 0 ? $"{NumberFormatter.Format(epsBase)} × {epsMult:F2} = {NumberFormatter.FormatPerSecond(epsTotal)}" : "0/sec";

        var playTime = TimeSpan.FromSeconds(state.TotalPlayTimeSeconds);
        PlayTimeDisplay.Text = $"{(int)playTime.TotalHours}h {playTime.Minutes}m";

        AchievementCountDisplay.Text = $"{state.UnlockedAchievements.Count}/{AchievementData.AllAchievements.Count}";
        ConspiracyCountDisplay.Text = $"{state.ProvenConspiracies.Count}/{ConspiracyData.AllConspiracies.Count}";

        UpdateGeneratorButtons();
        UpdateUpgradePanel();
        UpdateTinfoilShopPanel();
        UpdateConspiracyPanel();
        UpdateQuestPanel();
        UpdateAchievementPanel();
        UpdateOwnedGeneratorsPanel();
        UpdateSkillTreePanel();
        UpdateDailyChallengesPanel();
        UpdatePrestigePanel();
        UpdateMatrixPanel();
        UpdateStatisticsPanel();
        UpdateTabVisibility();
        UpdateTabHighlights();
    }

    private void UpdatePyramidLevel()
    {
        int conspiracyCount = _engine.State.ProvenConspiracies.Count;

        // Update rank display
        UpdateRankDisplay(conspiracyCount);

        // Each conspiracy increases level, max at 25 for pyramid icons
        int level = Math.Min(conspiracyCount, 25);

        // Only update pyramid if level changed
        if (level == _currentPyramidLevel) return;

        bool isLevelUp = level > _currentPyramidLevel && _currentPyramidLevel >= 0;
        _currentPyramidLevel = level;

        // Update pyramid icon from sprite sheet
        var pyramidIcon = PyramidSpriteSheetLoader.GetPyramidIcon(level);
        if (pyramidIcon != null)
        {
            PyramidImage.Source = pyramidIcon;
        }

        // Glow color tiers - eye changes when glow color tier changes
        Color glowColor = level switch
        {
            <= 11 => Color.FromRgb(0, 255, 65),      // Green (eye_basic)
            <= 17 => Color.FromRgb(200, 220, 100),   // Yellow-green (eye_golden)
            <= 23 => Color.FromRgb(150, 100, 255),   // Purple (eye_cosmic)
            _ => Color.FromRgb(220, 150, 255),       // Light purple/pink (eye_omega)
        };

        // Update glow color and eye tier (only when not in golden eye mode)
        if (!_engine.State.GoldenEyeActive)
        {
            // Update eye icon to match glow color tier
            var eyeIcon = PyramidSpriteSheetLoader.GetEyeIcon(pyramidLevel: level);
            if (eyeIcon != null) EyeImage.Source = eyeIcon;

            PyramidGlow.Color = glowColor;
            EyeAmbientGlow.Fill = new RadialGradientBrush(glowColor, Colors.Transparent);
        }

        // Scale pyramid gradually with each level (1.0 at level 0, up to 2.0 at level 25)
        double baseScale = 1.0 + (level * 0.04); // Each level adds 4% size
        PyramidScale.ScaleX = baseScale;
        PyramidScale.ScaleY = baseScale;

        // Also scale the click button and ambient glow
        ClickButton.Width = 180 * baseScale;
        ClickButton.Height = 180 * baseScale;

        // Increase eye size with progression
        double eyeSize = 50 + (level * 2.4); // 50 to 110 over 25 levels
        EyeImage.Width = eyeSize;
        EyeImage.Height = eyeSize;

        // Flash effect on level up
        if (isLevelUp)
        {
            PlayPyramidLevelUpEffect();
        }
    }

    private void UpdateRankDisplay(int conspiracyCount)
    {
        var rank = RankData.GetRankForConspiracies(conspiracyCount);

        // Only update if rank changed
        if (rank.Id == _currentRankId) return;
        _currentRankId = rank.Id;

        // Rank panel is permanently hidden - only show notifications
        if (conspiracyCount == 0) return;

        // Show rank up notification if not first rank
        if (conspiracyCount > 1)
        {
            try
            {
                AddNotification($"Rank Up! You are now {rank.Title} of the {rank.Society}!", new SolidColorBrush((Color)ColorConverter.ConvertFromString(rank.Color)));
                if (!string.IsNullOrEmpty(rank.FlavorText))
                {
                    ShowToast(rank.Icon, rank.FlavorText);
                }
            }
            catch
            {
                AddNotification($"Rank Up! You are now {rank.Title} of the {rank.Society}!", GreenBrush);
            }
        }
    }

    private void PlayPyramidLevelUpEffect()
    {
        // Create a white flash overlay
        var flash = new Ellipse
        {
            Width = 300,
            Height = 300,
            Fill = new RadialGradientBrush(
                Color.FromArgb(200, 255, 255, 255),
                Colors.Transparent),
            IsHitTestVisible = false,
            Opacity = 0
        };

        // Center it on the pyramid
        flash.HorizontalAlignment = HorizontalAlignment.Center;
        flash.VerticalAlignment = VerticalAlignment.Center;
        EyeContainer.Children.Insert(0, flash);

        // Animate flash in and out
        var fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(150)
        };

        var fadeOut = new DoubleAnimation
        {
            From = 1,
            To = 0,
            BeginTime = TimeSpan.FromMilliseconds(150),
            Duration = TimeSpan.FromMilliseconds(400)
        };

        fadeOut.Completed += (s, e) => EyeContainer.Children.Remove(flash);

        flash.BeginAnimation(OpacityProperty, fadeIn);
        flash.BeginAnimation(OpacityProperty, fadeOut);

        // Also pulse the pyramid scale
        var scaleUp = new DoubleAnimation
        {
            To = PyramidScale.ScaleX * 1.15,
            Duration = TimeSpan.FromMilliseconds(150),
            AutoReverse = true
        };

        PyramidScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleUp);
        PyramidScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleUp);

        // Play a sound
        SoundManager.Play("achievement");
    }

    private static readonly SolidColorBrush TabGlowBrush = new(Color.FromRgb(0, 255, 65));
    private static readonly SolidColorBrush TabNormalBrush = new(Color.FromRgb(136, 136, 136));

    private void UpdateTabHighlights()
    {
        var state = _engine.State;

        // Generators - highlight if any generator is affordable
        bool canBuyGenerator = GeneratorData.AllGenerators.Any(g => state.Evidence >= _engine.GetGeneratorCost(g.Id));
        SetTabHighlight(GeneratorsTab, canBuyGenerator);

        // Upgrades - highlight if any upgrade is affordable
        bool canBuyUpgrade = _engine.GetAvailableUpgrades().Any(u => _engine.CanAffordUpgrade(u.Id));
        SetTabHighlight(UpgradesTab, canBuyUpgrade);

        // Tinfoil Shop - highlight if any tinfoil upgrade is affordable
        bool canBuyTinfoil = _engine.GetAvailableTinfoilUpgrades().Any(u => state.Tinfoil >= u.TinfoilCost);
        SetTabHighlight(TinfoilShopTab, canBuyTinfoil);

        // Conspiracies - highlight if any conspiracy is affordable
        bool canProveConspiracy = _engine.GetAvailableConspiracies().Any(c => _engine.CanAffordConspiracy(c.Id));
        SetTabHighlight(ConspiraciesTab, canProveConspiracy);

        // Quests - highlight if any quest can be started
        bool canStartQuest = QuestData.AllQuests.Any(q => _engine.CanStartQuest(q.Id));
        SetTabHighlight(QuestsTab, canStartQuest);

        // Skills - highlight if skill points available
        bool hasSkillPoints = _engine.GetAvailableSkillPoints() > 0;
        SetTabHighlight(SkillsTab, hasSkillPoints);

        // Daily - highlight if any challenge can be claimed
        bool canClaimDaily = state.DailyChallenges.Any(c => c.Completed && !c.Claimed);
        SetTabHighlight(DailyTab, canClaimDaily);

        // Illuminati - highlight if any prestige upgrade is affordable
        bool canBuyPrestige = _engine.GetAvailableIlluminatiUpgrades().Any(u => _engine.CanAffordIlluminatiUpgrade(u.Id));
        SetTabHighlight(IlluminatiTab, canBuyPrestige);
    }

    private void SetTabHighlight(TabItem tab, bool highlight)
    {
        if (tab.Visibility != Visibility.Visible) return;

        if (highlight)
        {
            tab.Foreground = TabGlowBrush;
            tab.FontWeight = FontWeights.Bold;
        }
        else
        {
            tab.Foreground = TabNormalBrush;
            tab.FontWeight = FontWeights.Normal;
        }
    }

    private void UpdateTabVisibility()
    {
        var state = _engine.State;
        int conspiracyCount = state.ProvenConspiracies.Count;

        // Tinfoil Shop: show after proving 1+ conspiracies (they reward tinfoil)
        // OR if player has tinfoil/purchases from previous sessions
        TinfoilShopTab.Visibility = (conspiracyCount >= 1 || state.Tinfoil > 0 || state.TinfoilShopPurchases.Count > 0)
            ? Visibility.Visible : Visibility.Collapsed;

        // Quests: show after proving 2+ conspiracies (unlocks the believer network)
        // OR if player has 50+ believers (backup path for slow conspiracy provers)
        QuestsTab.Visibility = (conspiracyCount >= 2 || state.Believers >= 50)
            ? Visibility.Visible : Visibility.Collapsed;

        // Skills: show after proving 3+ conspiracies
        // OR if player has earned skill points/unlocked skills from previous sessions
        SkillsTab.Visibility = (conspiracyCount >= 3 || _engine.GetTotalSkillPoints() > 0 || state.UnlockedSkills.Count > 0)
            ? Visibility.Visible : Visibility.Collapsed;

        // Daily Challenges: show after proving 2+ conspiracies
        DailyTab.Visibility = conspiracyCount >= 2
            ? Visibility.Visible : Visibility.Collapsed;

        // Illuminati: show after proving 5+ conspiracies AND have 500B+ evidence
        // OR if player already has tokens/ascended
        IlluminatiTab.Visibility = ((conspiracyCount >= 5 && state.TotalEvidenceEarned >= 500_000_000_000) ||
            state.IlluminatiTokens > 0 || state.TimesAscended > 0)
            ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateGeneratorButtons()
    {
        var state = _engine.State;
        var (_, epsMult) = _engine.GetEpsBreakdown();

        // Build generator-specific multipliers from regular upgrades (same as GameEngine.CalculateBaseEps)
        var generatorMultipliers = new Dictionary<string, double>();
        foreach (var upgradeId in state.PurchasedUpgrades)
        {
            var upgrade = UpgradeData.GetById(upgradeId);
            if (upgrade?.Type == UpgradeType.GeneratorBoost && upgrade.TargetGeneratorId != null)
            {
                if (!generatorMultipliers.ContainsKey(upgrade.TargetGeneratorId))
                    generatorMultipliers[upgrade.TargetGeneratorId] = 1.0;
                generatorMultipliers[upgrade.TargetGeneratorId] *= upgrade.Value;
            }
        }

        // Progressive reveal: show owned, affordable, and one more
        bool shownFirstUnaffordable = false;

        foreach (var gen in GeneratorData.AllGenerators)
        {
            if (!_generatorButtons.TryGetValue(gen.Id, out var button)) continue;
            if (!_generatorContainers.TryGetValue(gen.Id, out var container)) continue;

            double cost = _engine.GetGeneratorCost(gen.Id);
            int owned = state.GetGeneratorCount(gen.Id);
            bool canAfford = state.Evidence >= cost;

            // Progressive reveal: show if owned, affordable, or the next unaffordable one
            bool shouldShow = owned > 0 || canAfford || !shownFirstUnaffordable;
            if (!canAfford && owned == 0 && !shownFirstUnaffordable)
                shownFirstUnaffordable = true;

            container.Visibility = shouldShow ? Visibility.Visible : Visibility.Collapsed;
            if (!shouldShow) continue;

            // Get generator-specific multiplier from regular upgrades AND generator upgrades
            double genSpecificMult = generatorMultipliers.TryGetValue(gen.Id, out var m) ? m : 1.0;
            genSpecificMult *= _engine.GetGeneratorUpgradeProductionMultiplier(gen.Id);

            double baseProduction = gen.GetProduction(owned) * genSpecificMult;
            double multipliedProduction = baseProduction * epsMult;

            // Always update enabled state (depends on evidence which changes constantly)
            button.IsEnabled = state.Evidence >= cost;

            // Per-generator production (with all multipliers)
            double perGenBase = gen.BaseProduction * genSpecificMult;
            double perGenMultiplied = perGenBase * epsMult;

            // State tuple for dirty-checking text updates (include multiplier so upgrades trigger refresh)
            var newState = (cost, owned, multipliedProduction, genSpecificMult);

            // Always update upgrade panel (affordability depends on evidence which changes constantly)
            // Upgrade panel is now stored separately (outside the button)
            if (_generatorUpgradePanels.TryGetValue(gen.Id, out var upgradePanel))
            {
                UpdateGeneratorUpgradePanel(gen.Id, owned, upgradePanel);
            }

            // Skip text updates if nothing changed (dirty check)
            if (_lastGenState.TryGetValue(gen.Id, out var lastState) && lastState == newState)
                continue;
            _lastGenState[gen.Id] = newState;

            // Button content is now a Grid directly (not wrapped in StackPanel)
            if (button.Content is Grid grid)
            {
                foreach (var child in grid.Children)
                {
                    if (child is StackPanel stack)
                    {
                        foreach (var item in stack.Children)
                        {
                            if (item is TextBlock tb)
                            {
                                if (tb.Tag as string == "cost") tb.Text = NumberFormatter.Format(cost);
                                else if (tb.Tag as string == "owned") tb.Text = $"Owned: {owned}";
                                else if (tb.Tag as string == "prod")
                                {
                                    if (owned > 0)
                                        tb.Text = $"Total: +{NumberFormatter.FormatPerSecond(multipliedProduction)} | Each: +{NumberFormatter.FormatPerSecond(perGenMultiplied)}";
                                    else
                                        tb.Text = $"+{NumberFormatter.FormatPerSecond(perGenMultiplied)} each";
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void UpdateGeneratorUpgradePanel(string generatorId, int owned, WrapPanel panel)
    {
        var availableUpgrades = _engine.GetAvailableGeneratorUpgrades(generatorId).ToList();
        var purchasedUpgrades = _engine.GetPurchasedGeneratorUpgrades(generatorId).ToList();
        var allUpgrades = GeneratorUpgradeData.GetUpgradesForGenerator(generatorId).ToList();

        // Show panel if there are any upgrades to display
        bool hasContent = availableUpgrades.Count > 0 || purchasedUpgrades.Count > 0 || allUpgrades.Any(u => owned >= 10);
        panel.Visibility = hasContent ? Visibility.Visible : Visibility.Collapsed;

        var state = _engine.State;

        // Build state hash to check if we need to rebuild
        var stateBuilder = new System.Text.StringBuilder();
        stateBuilder.Append(owned).Append('|');
        foreach (var u in allUpgrades)
        {
            bool purchased = purchasedUpgrades.Any(p => p.Id == u.Id);
            bool available = availableUpgrades.Any(a => a.Id == u.Id);
            bool canAfford = state.Evidence >= _engine.GetGeneratorUpgradeCost(u.Id);
            stateBuilder.Append(u.Id).Append(':').Append(purchased).Append(':').Append(available).Append(':').Append(canAfford).Append(';');
        }
        var currentState = stateBuilder.ToString();

        // Only rebuild if state changed
        if (_lastUpgradePanelState.TryGetValue(generatorId, out var lastState) && lastState == currentState)
            return;
        _lastUpgradePanelState[generatorId] = currentState;

        panel.Children.Clear();

        // Show all 4 upgrade slots
        foreach (var upgrade in allUpgrades)
        {
            bool isPurchased = purchasedUpgrades.Any(u => u.Id == upgrade.Id);
            bool isAvailable = availableUpgrades.Any(u => u.Id == upgrade.Id);
            bool isLocked = owned < upgrade.UnlockLevel;
            double cost = _engine.GetGeneratorUpgradeCost(upgrade.Id);
            bool canAfford = state.Evidence >= cost;

            var border = new Border
            {
                Background = isPurchased ? new SolidColorBrush(Color.FromRgb(20, 60, 20)) :
                             isAvailable && canAfford ? new SolidColorBrush(Color.FromRgb(40, 40, 60)) :
                             new SolidColorBrush(Color.FromRgb(30, 30, 35)),
                BorderBrush = isPurchased ? GreenBrush :
                              isAvailable && canAfford ? GoldBrush :
                              isAvailable ? new SolidColorBrush(Color.FromRgb(120, 100, 60)) :
                              DimBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 4, 8, 4),
                Margin = new Thickness(0, 0, 6, 0),
                Cursor = isAvailable && canAfford ? Cursors.Hand : Cursors.Arrow,
                Tag = upgrade.Id
            };

            var stack = new StackPanel { Orientation = Orientation.Horizontal };

            var levelText = new TextBlock
            {
                Text = $"Lv{upgrade.UnlockLevel}",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = isPurchased ? GreenBrush : isAvailable && canAfford ? GoldBrush : DimBrush,
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var iconText = new TextBlock
            {
                Text = upgrade.Icon,
                FontSize = 14,
                Margin = new Thickness(0, 0, 4, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            string displayText;
            if (isPurchased)
                displayText = upgrade.Name;
            else if (isLocked)
                displayText = $"Unlock at {upgrade.UnlockLevel}";
            else
                displayText = $"{NumberFormatter.Format(cost)}";

            var descText = new TextBlock
            {
                Text = displayText,
                FontSize = 12,
                Foreground = isPurchased ? LightBrush :
                            isAvailable && canAfford ? GoldBrush :
                            isAvailable ? new SolidColorBrush(Color.FromRgb(180, 150, 100)) :
                            DimBrush,
                VerticalAlignment = VerticalAlignment.Center
            };

            stack.Children.Add(levelText);
            stack.Children.Add(iconText);
            stack.Children.Add(descText);
            border.Child = stack;

            if (isAvailable && canAfford)
            {
                var normalBg = new SolidColorBrush(Color.FromRgb(40, 40, 60));
                var hoverBg = new SolidColorBrush(Color.FromRgb(60, 60, 90));

                border.MouseEnter += (s, e) =>
                {
                    if (s is Border b) b.Background = hoverBg;
                };
                border.MouseLeave += (s, e) =>
                {
                    if (s is Border b) b.Background = normalBg;
                };
                border.PreviewMouseLeftButtonDown += (s, e) =>
                {
                    e.Handled = true; // Prevent button click from starting
                    var upgradeId = (s as Border)?.Tag as string;
                    if (upgradeId != null && _engine.PurchaseGeneratorUpgrade(upgradeId))
                    {
                        ShowFlavorMessage($"Unlocked: {upgrade.Name}!");
                    }
                };
                // Also handle MouseUp to prevent button Click event from firing
                border.PreviewMouseLeftButtonUp += (s, e) =>
                {
                    e.Handled = true;
                };
            }

            // Add tooltip
            string? costText = !isPurchased && !isLocked ? $"Cost: {NumberFormatter.Format(cost)}" : null;
            border.ToolTip = CreateStyledTooltip(upgrade.Name, upgrade.Description, costText);

            panel.Children.Add(border);
        }
    }

    private void UpdateUpgradePanel()
    {
        var state = _engine.State;
        var buttonStyle = (Style)FindResource("GeneratorButton");
        var available = _engine.GetAvailableUpgrades().OrderBy(u => u.EvidenceCost).ToList();
        int purchasedCount = state.PurchasedUpgrades.Count;

        // Only rebuild if upgrade count changed
        if (available.Count != _lastUpgradeCount || purchasedCount != _lastPurchasedUpgradeCount)
        {
            _lastUpgradeCount = available.Count;
            _lastPurchasedUpgradeCount = purchasedCount;

            // Update purchased upgrades panel
            PurchasedUpgradesPanel.Children.Clear();
            foreach (var upgradeId in state.PurchasedUpgrades)
            {
                var upgrade = UpgradeData.GetById(upgradeId);
                if (upgrade == null) continue;

                var border = new Border { Background = new SolidColorBrush(Color.FromRgb(15, 52, 96)), BorderBrush = GoldBrush, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(4), Padding = new Thickness(10, 6, 10, 6), Margin = new Thickness(5) };
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var fallbackText = IconData.UpgradeIcons.TryGetValue(upgrade.Id, out var icon) ? icon : "?";
                var iconElement = CreateStyledIconBorder(upgrade.Id, fallbackText, 32, GoldBrush);
                iconElement.Margin = new Thickness(0, 0, 10, 0);
                var stack = new StackPanel();
                stack.Children.Add(new TextBlock { Text = upgrade.Name, FontWeight = FontWeights.Bold, Foreground = GoldBrush });
                stack.Children.Add(new TextBlock { Text = upgrade.Description, FontSize = 10, Foreground = DimBrush });

                Grid.SetColumn(iconElement, 0);
                Grid.SetColumn(stack, 1);
                grid.Children.Add(iconElement);
                grid.Children.Add(stack);
                border.Child = grid;
                PurchasedUpgradesPanel.Children.Add(border);
            }

            if (PurchasedUpgradesPanel.Children.Count == 0)
                PurchasedUpgradesPanel.Children.Add(new TextBlock { Text = "No upgrades purchased yet", Foreground = DimBrush, FontStyle = FontStyles.Italic, Margin = new Thickness(5) });

            // Update available upgrades panel (show affordable + one more)
            UpgradePanel.Children.Clear();
            _upgradeButtons.Clear();

            bool shownFirstUnaffordable = false;
            foreach (var upgrade in available)
            {
                bool canAfford = _engine.CanAffordUpgrade(upgrade.Id);

                // Progressive reveal: show if affordable or the next unaffordable one
                if (!canAfford && shownFirstUnaffordable) continue;
                if (!canAfford) shownFirstUnaffordable = true;

                var button = new Button { Style = buttonStyle, Tag = upgrade.Id, HorizontalContentAlignment = HorizontalAlignment.Stretch };
                button.Click += UpgradeButton_Click;

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var fallbackText = IconData.UpgradeIcons.TryGetValue(upgrade.Id, out var icon) ? icon : "?";
                var iconElement = CreateStyledIconBorder(upgrade.Id, fallbackText, 64, GreenBrush);
                iconElement.Margin = new Thickness(0, 0, 15, 0);

                var leftStack = new StackPanel();
                leftStack.Children.Add(new TextBlock { Text = upgrade.Name, FontWeight = FontWeights.Bold, Foreground = GreenBrush, FontSize = 21 });
                leftStack.Children.Add(new TextBlock { Text = upgrade.Description, FontSize = 18, Foreground = LightBrush });
                leftStack.Children.Add(new TextBlock { Text = upgrade.FlavorText, FontSize = 15, Foreground = DimBrush, FontStyle = FontStyles.Italic, TextWrapping = TextWrapping.Wrap, MaxWidth = 420 });

                var costText = upgrade.TinfoilCost > 0 ? $"{upgrade.TinfoilCost} Tinfoil" : NumberFormatter.Format(upgrade.EvidenceCost);
                var costBrush = upgrade.TinfoilCost > 0 ? SilverBrush : GoldBrush;
                var rightStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(15, 0, 0, 0) };
                rightStack.Children.Add(new TextBlock { Text = costText, FontWeight = FontWeights.Bold, Foreground = costBrush, HorizontalAlignment = HorizontalAlignment.Right, FontSize = 21 });

                Grid.SetColumn(iconElement, 0);
                Grid.SetColumn(leftStack, 1);
                Grid.SetColumn(rightStack, 2);
                grid.Children.Add(iconElement);
                grid.Children.Add(leftStack);
                grid.Children.Add(rightStack);

                button.Content = grid;
                UpgradePanel.Children.Add(button);
                _upgradeButtons[upgrade.Id] = button;
            }

            if (UpgradePanel.Children.Count == 0)
                UpgradePanel.Children.Add(new TextBlock { Text = "Keep gathering evidence to unlock upgrades...", Foreground = DimBrush, FontStyle = FontStyles.Italic, Margin = new Thickness(10) });
        }

        // Update enabled state for existing buttons
        foreach (var (id, button) in _upgradeButtons)
            button.IsEnabled = _engine.CanAffordUpgrade(id);
    }

    private void UpdateTinfoilShopPanel()
    {
        var buttonStyle = (Style)FindResource("GeneratorButton");
        var available = _engine.GetAvailableTinfoilUpgrades().OrderBy(u => u.TinfoilCost).ToList();
        var purchased = _engine.GetPurchasedTinfoilUpgrades().ToList();

        // Only rebuild if tinfoil count changed
        if (available.Count != _lastTinfoilCount)
        {
            _lastTinfoilCount = available.Count;
            _tinfoilButtons.Clear();

            PurchasedTinfoilPanel.Children.Clear();
            foreach (var upgrade in purchased)
            {
                var border = new Border { Background = new SolidColorBrush(Color.FromRgb(15, 52, 96)), BorderBrush = SilverBrush, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(4), Padding = new Thickness(10, 6, 10, 6), Margin = new Thickness(5) };
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var iconElement = CreateStyledIconBorder(upgrade.Icon, "◆", 48, SilverBrush);
                iconElement.Margin = new Thickness(0, 0, 15, 0);
                var stack = new StackPanel();
                stack.Children.Add(new TextBlock { Text = upgrade.Name, FontWeight = FontWeights.Bold, Foreground = SilverBrush, FontSize = 21 });
                stack.Children.Add(new TextBlock { Text = upgrade.Description, FontSize = 15, Foreground = DimBrush });

                Grid.SetColumn(iconElement, 0);
                Grid.SetColumn(stack, 1);
                grid.Children.Add(iconElement);
                grid.Children.Add(stack);
                border.Child = grid;
                PurchasedTinfoilPanel.Children.Add(border);
            }

            if (PurchasedTinfoilPanel.Children.Count == 0)
                PurchasedTinfoilPanel.Children.Add(new TextBlock { Text = "No upgrades purchased yet", Foreground = DimBrush, FontStyle = FontStyles.Italic, Margin = new Thickness(5) });

            TinfoilShopPanel.Children.Clear();
            foreach (var upgrade in available)
            {
                var button = new Button { Style = buttonStyle, Tag = upgrade.Id, HorizontalContentAlignment = HorizontalAlignment.Stretch };
                button.Click += TinfoilShopButton_Click;

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var iconElement = CreateStyledIconBorder(upgrade.Icon, "◆", 64, SilverBrush);
                iconElement.Margin = new Thickness(0, 0, 15, 0);
                iconElement.Tag = "icon";

                var leftStack = new StackPanel();
                leftStack.Children.Add(new TextBlock { Text = upgrade.Name, FontWeight = FontWeights.Bold, Foreground = SilverBrush, Tag = "name", FontSize = 21 });
                leftStack.Children.Add(new TextBlock { Text = upgrade.Description, FontSize = 18, Foreground = LightBrush, Tag = "desc" });
                leftStack.Children.Add(new TextBlock { Text = upgrade.FlavorText, FontSize = 15, Foreground = DimBrush, FontStyle = FontStyles.Italic, TextWrapping = TextWrapping.Wrap, MaxWidth = 420 });

                var rightStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(15, 0, 0, 0) };
                rightStack.Children.Add(new TextBlock { Text = $"{NumberFormatter.FormatInteger(upgrade.TinfoilCost)} Tinfoil", FontWeight = FontWeights.Bold, Foreground = SilverBrush, HorizontalAlignment = HorizontalAlignment.Right, Tag = "cost", FontSize = 21 });

                Grid.SetColumn(iconElement, 0);
                Grid.SetColumn(leftStack, 1);
                Grid.SetColumn(rightStack, 2);
                grid.Children.Add(iconElement);
                grid.Children.Add(leftStack);
                grid.Children.Add(rightStack);

                button.Content = grid;
                TinfoilShopPanel.Children.Add(button);
                _tinfoilButtons[upgrade.Id] = button;
            }
        }

        // Update enabled state for existing buttons
        foreach (var (id, button) in _tinfoilButtons)
            button.IsEnabled = _engine.CanAffordTinfoilUpgrade(id);
    }

    private int _lastAvailableConspiracyCount = -1;

    private void UpdateConspiracyPanel()
    {
        var state = _engine.State;
        var buttonStyle = (Style)FindResource("GeneratorButton");
        var available = _engine.GetAvailableConspiracies().ToList();

        // Rebuild when proven count or available count changes
        if (state.ProvenConspiracies.Count != _lastProvenConspiracyCount || available.Count != _lastAvailableConspiracyCount)
        {
            _lastProvenConspiracyCount = state.ProvenConspiracies.Count;
            _lastAvailableConspiracyCount = available.Count;
            ConspiracyPanel.Children.Clear();
            _conspiracyButtons.Clear();

            // Show progress summary
            int totalConspiracies = ConspiracyData.AllConspiracies.Count;
            int provenCount = state.ProvenConspiracies.Count;

            var progressPanel = new StackPanel { Margin = new Thickness(5, 0, 5, 10) };
            progressPanel.Children.Add(new TextBlock
            {
                Text = $"Conspiracies Proven: {provenCount} / {totalConspiracies}",
                FontWeight = FontWeights.Bold,
                Foreground = GreenBrush,
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            // Progress bar - uses Grid with star sizing for proper scaling
            double progressRatio = provenCount / (double)totalConspiracies;
            var progressBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 40)),
                CornerRadius = new CornerRadius(4),
                Height = 12,
                Margin = new Thickness(0, 5, 0, 0),
                ClipToBounds = true
            };
            var progressGrid = new Grid();
            progressGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(progressRatio, GridUnitType.Star) });
            progressGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1 - progressRatio, GridUnitType.Star) });
            var progressFill = new Border
            {
                Background = GreenBrush,
                CornerRadius = new CornerRadius(4, 0, 0, 4)
            };
            Grid.SetColumn(progressFill, 0);
            progressGrid.Children.Add(progressFill);
            progressBorder.Child = progressGrid;
            progressPanel.Children.Add(progressBorder);
            ConspiracyPanel.Children.Add(progressPanel);

            // Show all proven conspiracies
            var provenConspiracies = ConspiracyData.AllConspiracies.Where(c => state.ProvenConspiracies.Contains(c.Id)).ToList();
            if (provenConspiracies.Count > 0)
            {
                // Header for proven section
                ConspiracyPanel.Children.Add(new TextBlock
                {
                    Text = "PROVEN",
                    FontWeight = FontWeights.Bold,
                    Foreground = GreenBrush,
                    FontSize = 14,
                    Margin = new Thickness(5, 10, 5, 5)
                });

                foreach (var proven in provenConspiracies)
                {
                    var border = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(15, 45, 30)),
                        BorderBrush = GreenBrush,
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(4),
                        Padding = new Thickness(10, 6, 10, 6),
                        Margin = new Thickness(5, 2, 5, 2)
                    };

                    var grid = new Grid();
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    var fallbackText = IconData.ConspiracyIcons.TryGetValue(proven.Id, out var icon) ? icon : "?";
                    var iconElement = CreateStyledIconBorder(proven.Id, fallbackText, 72, GreenBrush);
                    iconElement.Margin = new Thickness(0, 0, 10, 0);

                    var nameText = new TextBlock
                    {
                        Text = proven.Name,
                        FontWeight = FontWeights.Bold,
                        Foreground = GreenBrush,
                        FontSize = 18,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    var bonusText = proven.MultiplierBonus > 1.0 ? $"x{proven.MultiplierBonus} production" : $"+{proven.ClickBonus} click power";
                    var bonusBlock = new TextBlock
                    {
                        Text = bonusText,
                        FontSize = 14,
                        Foreground = GoldBrush,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    Grid.SetColumn(iconElement, 0);
                    Grid.SetColumn(nameText, 1);
                    Grid.SetColumn(bonusBlock, 2);
                    grid.Children.Add(iconElement);
                    grid.Children.Add(nameText);
                    grid.Children.Add(bonusBlock);
                    border.Child = grid;

                    // Tooltip with full details
                    border.ToolTip = CreateStyledTooltip(proven.Name, proven.FlavorText,
                        proven.MultiplierBonus > 1.0 ? $"x{proven.MultiplierBonus} all production" : $"+{proven.ClickBonus} click power");

                    ConspiracyPanel.Children.Add(border);
                }
            }

            // Show only the next available conspiracy (first one in the list)
            if (available.Count > 0)
            {
                // Header for next section
                ConspiracyPanel.Children.Add(new TextBlock
                {
                    Text = "NEXT",
                    FontWeight = FontWeights.Bold,
                    Foreground = GoldBrush,
                    FontSize = 14,
                    Margin = new Thickness(5, 10, 5, 5)
                });

                var nextConspiracy = available.First();

                var button = new Button { Style = buttonStyle, Tag = nextConspiracy.Id, HorizontalContentAlignment = HorizontalAlignment.Stretch };
                button.Click += ConspiracyButton_Click;

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var fallbackText2 = IconData.ConspiracyIcons.TryGetValue(nextConspiracy.Id, out var icon2) ? icon2 : "?";
                var iconElement2 = CreateStyledIconBorder(nextConspiracy.Id, fallbackText2, 96, DimBrush);
                iconElement2.Margin = new Thickness(0, 0, 15, 0);

                var leftStack = new StackPanel();
                leftStack.Children.Add(new TextBlock { Text = nextConspiracy.Name, FontWeight = FontWeights.Bold, Foreground = GreenBrush, FontSize = 21 });
                leftStack.Children.Add(new TextBlock { Text = nextConspiracy.Description, FontSize = 17, Foreground = LightBrush });
                leftStack.Children.Add(new TextBlock { Text = nextConspiracy.FlavorText, FontSize = 15, Foreground = DimBrush, FontStyle = FontStyles.Italic, TextWrapping = TextWrapping.Wrap, MaxWidth = 420 });
                var bonusText2 = nextConspiracy.MultiplierBonus > 1.0 ? $"Reward: x{nextConspiracy.MultiplierBonus} all + {NumberFormatter.FormatInteger(nextConspiracy.TinfoilReward)} Tinfoil" : $"Reward: +{NumberFormatter.FormatInteger(nextConspiracy.ClickBonus)} click + {NumberFormatter.FormatInteger(nextConspiracy.TinfoilReward)} Tinfoil";
                leftStack.Children.Add(new TextBlock { Text = bonusText2, FontSize = 15, Foreground = GoldBrush, Margin = new Thickness(0, 5, 0, 0) });

                var rightStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(15, 0, 0, 0) };
                bool canClaim = state.TotalEvidenceEarned >= nextConspiracy.EvidenceCost;
                rightStack.Children.Add(new TextBlock { Text = canClaim ? "✓ CLAIM" : $"Need {NumberFormatter.Format(nextConspiracy.EvidenceCost)}", FontWeight = FontWeights.Bold, Foreground = canClaim ? GreenBrush : DimBrush, FontSize = 21 });
                rightStack.Children.Add(new TextBlock { Text = "total evidence", FontSize = 14, Foreground = DimBrush, HorizontalAlignment = HorizontalAlignment.Center });

                Grid.SetColumn(iconElement2, 0);
                Grid.SetColumn(leftStack, 1);
                Grid.SetColumn(rightStack, 2);
                grid.Children.Add(iconElement2);
                grid.Children.Add(leftStack);
                grid.Children.Add(rightStack);

                button.Content = grid;
                ConspiracyPanel.Children.Add(button);
                _conspiracyButtons[nextConspiracy.Id] = button;
            }
            else if (provenCount == totalConspiracies)
            {
                // All conspiracies proven!
                var completeBorder = new Border { Background = new SolidColorBrush(Color.FromRgb(20, 60, 20)), BorderBrush = GoldBrush, BorderThickness = new Thickness(2), CornerRadius = new CornerRadius(8), Padding = new Thickness(20), Margin = new Thickness(5) };
                var completeStack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
                completeStack.Children.Add(new TextBlock { Text = "🏆 ALL CONSPIRACIES PROVEN! 🏆", FontWeight = FontWeights.Bold, Foreground = GoldBrush, FontSize = 24, HorizontalAlignment = HorizontalAlignment.Center });
                completeStack.Children.Add(new TextBlock { Text = "You have uncovered the ultimate truth.", FontSize = 16, Foreground = LightBrush, FontStyle = FontStyles.Italic, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 10, 0, 0) });
                completeBorder.Child = completeStack;
                ConspiracyPanel.Children.Add(completeBorder);
            }

            if (ConspiracyPanel.Children.Count == 1) // Only progress bar
                ConspiracyPanel.Children.Add(new TextBlock { Text = "Keep gathering evidence to uncover conspiracies...", Foreground = DimBrush, FontStyle = FontStyles.Italic, Margin = new Thickness(10) });
        }

        // Update enabled state for existing buttons
        foreach (var (id, button) in _conspiracyButtons)
            button.IsEnabled = _engine.CanAffordConspiracy(id);
    }

    private void UpdateQuestPanel()
    {
        UpdateAutoQuestToggle();
        var state = _engine.State;
        var buttonStyle = (Style)FindResource("GeneratorButton");

        // Active quests always need updating for progress bars
        ActiveQuestsPanel.Children.Clear();
        foreach (var activeQuest in state.ActiveQuests)
        {
            var quest = QuestData.GetById(activeQuest.QuestId);
            if (quest == null) continue;

            var border = new Border { Background = new SolidColorBrush(Color.FromRgb(30, 40, 60)), BorderBrush = GoldBrush, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(4), Padding = new Thickness(10, 8, 10, 8), Margin = new Thickness(5) };
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var iconBlock = CreateStyledIconBorder(quest.Id, quest.Icon, 48, GoldBrush);
            iconBlock.Margin = new Thickness(0, 0, 10, 0);
            iconBlock.VerticalAlignment = VerticalAlignment.Center;

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock { Text = quest.Name, FontWeight = FontWeights.Bold, Foreground = GoldBrush });
            stack.Children.Add(new TextBlock { Text = $"{activeQuest.BelieversSent} believers on mission", FontSize = 11, Foreground = DimBrush });

            var remaining = activeQuest.EndTime - DateTime.Now;
            var totalDuration = (activeQuest.EndTime - activeQuest.StartTime).TotalSeconds;
            var elapsed = totalDuration - remaining.TotalSeconds;
            var progress = Math.Max(0, Math.Min(1, elapsed / totalDuration));
            var timeText = remaining.TotalSeconds > 0 ? $"{(int)remaining.TotalMinutes}:{remaining.Seconds:D2} remaining" : "Completing...";

            // Scalable progress bar using Grid with star sizing
            var progressBar = new Border { Background = DarkBrush, CornerRadius = new CornerRadius(3), Height = 8, Margin = new Thickness(0, 4, 0, 0), HorizontalAlignment = HorizontalAlignment.Stretch, ClipToBounds = true };
            var progressGrid = new Grid();
            progressGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(progress, GridUnitType.Star) });
            progressGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1 - progress, GridUnitType.Star) });
            var progressFill = new Border { Background = GoldBrush, CornerRadius = new CornerRadius(3, 0, 0, 3) };
            Grid.SetColumn(progressFill, 0);
            progressGrid.Children.Add(progressFill);
            progressBar.Child = progressGrid;
            stack.Children.Add(progressBar);
            stack.Children.Add(new TextBlock { Text = $"{progress:P0} complete", FontSize = 10, Foreground = DimBrush, Margin = new Thickness(0, 2, 0, 0) });

            var timeBlock = new TextBlock { Text = timeText, FontSize = 12, Foreground = GoldBrush, VerticalAlignment = VerticalAlignment.Center };

            Grid.SetColumn(iconBlock, 0);
            Grid.SetColumn(stack, 1);
            Grid.SetColumn(timeBlock, 2);
            grid.Children.Add(iconBlock);
            grid.Children.Add(stack);
            grid.Children.Add(timeBlock);
            border.Child = grid;
            ActiveQuestsPanel.Children.Add(border);
        }

        if (state.ActiveQuests.Count == 0)
            ActiveQuestsPanel.Children.Add(new TextBlock { Text = "No active quests", Foreground = DimBrush, FontStyle = FontStyles.Italic, Margin = new Thickness(5) });

        // Only rebuild available quests when quest count changes
        int currentQuestCount = state.ActiveQuests.Count;
        if (currentQuestCount != _lastQuestCount)
        {
            _lastQuestCount = currentQuestCount;
            AvailableQuestsPanel.Children.Clear();
            _questButtons.Clear();

            foreach (var quest in QuestData.AllQuests.OrderBy(q => q.BelieversRequired))
            {
                if (state.ActiveQuests.Any(q => q.QuestId == quest.Id)) continue;

                var riskColor = quest.Risk switch { QuestRisk.Low => GreenBrush, QuestRisk.Medium => OrangeBrush, QuestRisk.High => RedBrush, _ => DimBrush };

                var button = new Button { Style = buttonStyle, Tag = quest.Id, HorizontalContentAlignment = HorizontalAlignment.Stretch };
                button.Click += QuestButton_Click;

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var qIconBlock = CreateStyledIconBorder(quest.Id, quest.Icon, 56, riskColor);
                qIconBlock.Margin = new Thickness(0, 0, 12, 0);
                qIconBlock.VerticalAlignment = VerticalAlignment.Center;

                var leftStack = new StackPanel();
                leftStack.Children.Add(new TextBlock { Text = quest.Name, FontWeight = FontWeights.Bold, Foreground = GreenBrush, FontSize = 18 });
                leftStack.Children.Add(new TextBlock { Text = quest.Description, FontSize = 14, Foreground = LightBrush });
                leftStack.Children.Add(new TextBlock { Text = quest.FlavorText, FontSize = 12, Foreground = DimBrush, FontStyle = FontStyles.Italic });

                var riskText = quest.Risk switch { QuestRisk.Low => "LOW RISK", QuestRisk.Medium => "MEDIUM RISK", QuestRisk.High => "HIGH RISK", _ => "" };
                double adjustedChance = Math.Min(quest.SuccessChance + _engine.GetTinfoilQuestSuccessBonus(), 0.95);
                leftStack.Children.Add(new TextBlock { Text = $"{riskText} ({adjustedChance:P0})", FontSize = 12, Foreground = riskColor, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 3, 0, 0) });

                var rightStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0) };
                rightStack.Children.Add(new TextBlock { Text = $"{quest.BelieversRequired} believers", FontWeight = FontWeights.Bold, Foreground = GoldBrush, HorizontalAlignment = HorizontalAlignment.Right, FontSize = 16 });
                var duration = TimeSpan.FromSeconds(quest.DurationSeconds);
                rightStack.Children.Add(new TextBlock { Text = duration.TotalMinutes >= 1 ? $"{(int)duration.TotalMinutes}m" : $"{duration.Seconds}s", FontSize = 13, Foreground = DimBrush, HorizontalAlignment = HorizontalAlignment.Right });
                var rewardText = quest.TinfoilReward > 0 ? $"+{quest.TinfoilReward} tinfoil" : $"~{quest.EvidenceMultiplier}s EPS";
                rightStack.Children.Add(new TextBlock { Text = rewardText, FontSize = 12, Foreground = GoldBrush, HorizontalAlignment = HorizontalAlignment.Right });

                Grid.SetColumn(qIconBlock, 0);
                Grid.SetColumn(leftStack, 1);
                Grid.SetColumn(rightStack, 2);
                grid.Children.Add(qIconBlock);
                grid.Children.Add(leftStack);
                grid.Children.Add(rightStack);

                button.Content = grid;
                AvailableQuestsPanel.Children.Add(button);
                _questButtons[quest.Id] = button;
            }
        }

        // Update enabled state for existing buttons
        foreach (var (id, button) in _questButtons)
            button.IsEnabled = _engine.CanStartQuest(id);
    }

    private string GetAchievementIconKey(Achievement achievement)
    {
        return achievement.Type switch
        {
            AchievementType.TotalEvidence => "achievement_evidence",
            AchievementType.TotalClicks => "achievement_clicks",
            AchievementType.GeneratorOwned or AchievementType.TotalGenerators => "achievement_generators",
            AchievementType.ConspiraciesProven => "achievement_conspiracies",
            AchievementType.PlayTime => "achievement_playtime",
            AchievementType.TimesAscended or AchievementType.TimesMatrixBroken => "achievement_prestige",
            AchievementType.QuestsCompleted => "achievement_quests",
            AchievementType.TotalTinfoil or AchievementType.CriticalClicks or AchievementType.TotalTokensEarned => "achievement_meta",
            _ => "achievement_evidence"
        };
    }

    private void UpdateAchievementPanel()
    {
        var state = _engine.State;
        int currentCount = state.UnlockedAchievements.Count;
        if (currentCount == _lastAchievementCount) return;
        _lastAchievementCount = currentCount;

        AchievementPanel.Children.Clear();

        foreach (var achievement in _engine.GetUnlockedAchievements())
        {
            var border = new Border { Background = new SolidColorBrush(Color.FromRgb(15, 52, 96)), BorderBrush = GoldBrush, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(4), Padding = new Thickness(15, 9, 15, 9), Margin = new Thickness(5) };
            var mainStack = new StackPanel { Orientation = Orientation.Horizontal };

            // Add icon
            var iconKey = GetAchievementIconKey(achievement);
            var iconBorder = CreateStyledIconBorder(iconKey, "★", 48, GoldBrush);
            iconBorder.Margin = new Thickness(0, 0, 12, 0);
            iconBorder.VerticalAlignment = VerticalAlignment.Center;
            mainStack.Children.Add(iconBorder);

            var textStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            textStack.Children.Add(new TextBlock { Text = achievement.Name, FontWeight = FontWeights.Bold, Foreground = GoldBrush, FontSize = 21 });
            textStack.Children.Add(new TextBlock { Text = achievement.Description, FontSize = 15, Foreground = LightBrush });
            mainStack.Children.Add(textStack);

            border.Child = mainStack;
            AchievementPanel.Children.Add(border);
        }

        foreach (var achievement in _engine.GetLockedAchievements())
        {
            double current = achievement.Type switch
            {
                AchievementType.TotalEvidence => state.TotalEvidenceEarned,
                AchievementType.TotalClicks => state.TotalClicks,
                AchievementType.GeneratorOwned => achievement.TargetId != null ? state.GetGeneratorCount(achievement.TargetId) : 0,
                AchievementType.ConspiraciesProven => state.ProvenConspiracies.Count,
                AchievementType.PlayTime => state.TotalPlayTimeSeconds,
                AchievementType.TimesAscended => state.TimesAscended,
                AchievementType.TimesMatrixBroken => state.TimesMatrixBroken,
                AchievementType.QuestsCompleted => state.QuestsCompleted,
                AchievementType.TotalTinfoil => state.Tinfoil,
                AchievementType.CriticalClicks => state.CriticalClicks,
                AchievementType.TotalTokensEarned => state.TotalIlluminatiTokensEarned,
                _ => 0
            };
            double progress = Math.Min(current / achievement.Threshold, 1.0);

            var border = new Border { Background = DarkBrush, BorderBrush = DimBrush, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(4), Padding = new Thickness(15, 9, 15, 9), Margin = new Thickness(5) };
            var mainStack = new StackPanel { Orientation = Orientation.Horizontal };

            // Add dimmed icon
            var iconKey = GetAchievementIconKey(achievement);
            var iconBorder = CreateStyledIconBorder(iconKey, "★", 48, DimBrush);
            iconBorder.Margin = new Thickness(0, 0, 12, 0);
            iconBorder.VerticalAlignment = VerticalAlignment.Center;
            iconBorder.Opacity = 0.5;
            mainStack.Children.Add(iconBorder);

            var textStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            textStack.Children.Add(new TextBlock { Text = achievement.Name, FontWeight = FontWeights.Bold, Foreground = DimBrush, FontSize = 21 });
            textStack.Children.Add(new TextBlock { Text = achievement.Description, FontSize = 15, Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)) });
            textStack.Children.Add(new TextBlock { Text = $"Progress: {progress:P0}", FontSize = 14, Foreground = DimBrush, Margin = new Thickness(0, 3, 0, 0) });
            mainStack.Children.Add(textStack);

            border.Child = mainStack;
            AchievementPanel.Children.Add(border);
        }
    }

    private void UpdateOwnedGeneratorsPanel()
    {
        var state = _engine.State;
        var sb = new System.Text.StringBuilder();
        foreach (var gen in GeneratorData.AllGenerators)
            sb.Append($"{gen.Id}:{state.GetGeneratorCount(gen.Id)};");
        sb.Append($"b:{(int)state.Believers}");
        string currentState = sb.ToString();
        if (currentState == _lastOwnedGenState) return;
        _lastOwnedGenState = currentState;

        OwnedGeneratorsPanel.Children.Clear();

        foreach (var gen in GeneratorData.AllGenerators)
        {
            int owned = state.GetGeneratorCount(gen.Id);
            if (owned > 0)
            {
                var iconText = IconData.GeneratorIcons.TryGetValue(gen.Id, out var icon) ? icon : "";
                OwnedGeneratorsPanel.Children.Add(new TextBlock { Text = $"{iconText} {gen.Name}: {owned}", Foreground = LightBrush, FontSize = 12, Margin = new Thickness(0, 2, 0, 2) });
            }
        }

        if (state.Believers > 0)
        {
            OwnedGeneratorsPanel.Children.Add(new Separator { Background = DimBrush, Margin = new Thickness(0, 10, 0, 10) });
            OwnedGeneratorsPanel.Children.Add(new TextBlock { Text = "BELIEVER SOURCES", FontSize = 12, FontWeight = FontWeights.Bold, Foreground = GoldBrush, Margin = new Thickness(0, 0, 0, 5) });

            var breakdown = _engine.GetBelieverBreakdown();
            foreach (var (genId, believers) in breakdown)
            {
                var gen = GeneratorData.GetById(genId);
                if (gen != null)
                    OwnedGeneratorsPanel.Children.Add(new TextBlock { Text = $"{gen.Name}: +{NumberFormatter.FormatInteger(believers)}", Foreground = DimBrush, FontSize = 11, Margin = new Thickness(0, 1, 0, 1) });
            }

            double tinfoilMultiplier = _engine.GetTinfoilBelieverMultiplier();
            if (tinfoilMultiplier > 1.0)
                OwnedGeneratorsPanel.Children.Add(new TextBlock { Text = $"Tinfoil Shop: ×{tinfoilMultiplier:F2}", Foreground = SilverBrush, FontSize = 11, Margin = new Thickness(0, 1, 0, 1) });
        }
    }

    private void ShowFlavorMessage(string message) => FlavorTextDisplay.Text = message;

    private ToolTip CreateStyledTooltip(string title, string description, string? bonus = null)
    {
        var stack = new StackPanel { MaxWidth = 300 };
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontWeight = FontWeights.Bold,
            Foreground = GoldBrush,
            TextWrapping = TextWrapping.Wrap
        });
        stack.Children.Add(new TextBlock
        {
            Text = description,
            Foreground = LightBrush,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 4, 0, 0)
        });
        if (!string.IsNullOrEmpty(bonus))
        {
            stack.Children.Add(new TextBlock
            {
                Text = bonus,
                Foreground = GreenBrush,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 4, 0, 0)
            });
        }

        return new ToolTip
        {
            Content = stack,
            Background = new SolidColorBrush(Color.FromArgb(230, 30, 30, 40)), // ~90% opacity
            BorderBrush = new SolidColorBrush(Color.FromArgb(200, 255, 215, 0)), // Semi-transparent gold
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10)
        };
    }

    /// <summary>
    /// Creates a styled icon with rounded corners, drop shadow, and proper clipping.
    /// </summary>
    private Border CreateStyledIconBorder(string iconKey, string fallbackText, double size, Brush foreground)
    {
        var iconImage = IconHelper.CreateIconWithFallback(iconKey, fallbackText, size - 4, foreground);

        var iconBorder = new Border
        {
            Width = size,
            Height = size,
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(60, 0, 255, 65)),
            BorderThickness = new Thickness(1),
            VerticalAlignment = VerticalAlignment.Center,
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 8,
                ShadowDepth = 2,
                Opacity = 0.4,
                Direction = 315
            }
        };

        // If it's an Image, use ImageBrush as background to clip to rounded corners
        if (iconImage is Image img && img.Source != null)
        {
            iconBorder.Background = new ImageBrush
            {
                ImageSource = img.Source,
                Stretch = Stretch.Uniform,
                AlignmentX = AlignmentX.Center,
                AlignmentY = AlignmentY.Center
            };
        }
        else
        {
            // For TextBlock fallback, keep as child
            iconBorder.Child = iconImage;
        }

        return iconBorder;
    }

    private void ShowAchievementUnlocked(Achievement achievement)
    {
        SoundManager.Play("achievement");
        ShowToast("UNLOCKED!", achievement.Name);
        AddNotification($"Achievement unlocked: {achievement.Name}", GoldBrush);
    }

    private void OnPrestigeComplete()
    {
        ShowToast("ASCENDED!", "You have joined the Illuminati!");
        AddNotification("You have ascended to the Illuminati!", PurpleBrush);
        FlavorTextDisplay.Text = "The shadows welcome you...";
    }

    private void OnDailyChallengeComplete(StoredChallenge challenge)
    {
        ShowToast("CHALLENGE!", $"{challenge.Name} completed!");
        AddNotification($"Daily challenge '{challenge.Name}' completed! Click to claim reward.", GoldBrush);
    }

    private void OnOfflineProgress(double evidenceEarned, double believersGained, TimeSpan timeAway)
    {
        string timeText = timeAway.TotalHours >= 1
            ? $"{(int)timeAway.TotalHours}h {timeAway.Minutes}m"
            : $"{(int)timeAway.TotalMinutes}m";

        string message = $"Welcome back! While you were away ({timeText}):\n";
        message += $"+{NumberFormatter.Format(evidenceEarned)} Evidence";
        if (believersGained >= 1)
            message += $"\n+{NumberFormatter.FormatInteger(believersGained)} Believers";

        ShowToast("WELCOME BACK!", $"+{NumberFormatter.Format(evidenceEarned)} Evidence");
        AddNotification($"Offline progress ({timeText}): +{NumberFormatter.Format(evidenceEarned)} evidence", GoldBrush);
    }

    private void OnSaveError(string errorMessage)
    {
        // Show error toast to user
        ShowToast("⚠️", errorMessage, 5);
        AddNotification($"⚠️ {errorMessage}", RedBrush);
    }

    private void PrestigeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_engine.CanPrestige())
        {
            int tokens = _engine.GetTokensFromPrestige();
            AscendTokensText.Text = $"{tokens} Illuminati Token{(tokens != 1 ? "s" : "")}";
            AscendOverlay.Visibility = Visibility.Visible;
        }
    }

    private void AscendConfirm_Click(object sender, RoutedEventArgs e)
    {
        AscendOverlay.Visibility = Visibility.Collapsed;
        _engine.PerformPrestige();
    }

    private void AscendCancel_Click(object sender, RoutedEventArgs e)
    {
        AscendOverlay.Visibility = Visibility.Collapsed;
    }

    // === MATRIX PRESTIGE ===
    private void MatrixBreakButton_Click(object sender, RoutedEventArgs e)
    {
        if (_engine.CanBreakMatrix())
        {
            int glitchTokens = _engine.GetGlitchTokensFromMatrix();
            if (System.Windows.MessageBox.Show(
                $"Break the Matrix?\n\nYou will earn {glitchTokens} Glitch Token(s).\n\nThis resets ALL progress including Illuminati tokens and upgrades, but you keep Glitch Tokens, Matrix upgrades, skills, and achievements.",
                "Break the Matrix",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.Yes)
            {
                _engine.BreakMatrix();
                ShowToast("MATRIX BROKEN!", "You have transcended reality!");
                AddNotification("You have broken the Matrix! Glitch Tokens earned.", GreenBrush);
            }
        }
    }

    private void MatrixUpgradeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string upgradeId)
        {
            if (_engine.PurchaseMatrixUpgrade(upgradeId))
            {
                var upgrade = MatrixData.GetById(upgradeId);
                SoundManager.Play("upgrade");
                ShowToast("POWER UNLOCKED!", upgrade?.Name ?? "Matrix Upgrade");
                AddNotification($"Matrix power unlocked: {upgrade?.Name}", GreenBrush);
            }
        }
    }

    private string _lastMatrixState = "";

    private void UpdateMatrixPanel()
    {
        var state = _engine.State;
        bool canBreak = _engine.CanBreakMatrix();
        bool hasGlitchTokens = state.GlitchTokens > 0 || state.MatrixUpgrades.Count > 0;
        bool showMatrix = canBreak || hasGlitchTokens || state.TimesAscended >= 3;

        // Show Matrix section when conditions are met
        MatrixSection.Visibility = showMatrix ? Visibility.Visible : Visibility.Collapsed;

        if (!showMatrix) return;

        // Update Matrix info
        if (canBreak)
        {
            int glitchTokens = _engine.GetGlitchTokensFromMatrix();
            MatrixInfoText.Text = $"You have ascended {state.TimesAscended} times.\nBreaking the Matrix will grant {glitchTokens} Glitch Token(s).";
            MatrixBreakButton.IsEnabled = true;
        }
        else
        {
            int ascensionsNeeded = Math.Max(0, MatrixData.MATRIX_ASCENSION_REQUIREMENT - state.TimesAscended);
            double tokensNeeded = Math.Max(0, MatrixData.MATRIX_TOKEN_REQUIREMENT - state.TotalIlluminatiTokensEarned);
            MatrixInfoText.Text = $"Requirements to break the Matrix:\n• {ascensionsNeeded} more ascensions\n• {tokensNeeded:F0} more total Illuminati tokens";
            MatrixBreakButton.IsEnabled = false;
        }

        GlitchTokensText.Text = $"⟁ {state.GlitchTokens} Glitch Token(s)";

        // Check if state changed for panel rebuild
        string currentState = $"{state.GlitchTokens}:{state.MatrixUpgrades.Count}";
        if (currentState == _lastMatrixState) return;
        _lastMatrixState = currentState;

        var buttonStyle = (Style)FindResource("GeneratorButton");

        // Show headers if we have tokens or upgrades
        bool hasContent = state.GlitchTokens > 0 || _engine.GetAvailableMatrixUpgrades().Any();
        MatrixUpgradesHeader.Visibility = hasContent ? Visibility.Visible : Visibility.Collapsed;
        PurchasedMatrixHeader.Visibility = state.MatrixUpgrades.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        // Available Matrix upgrades
        MatrixUpgradesPanel.Children.Clear();
        foreach (var upgrade in _engine.GetAvailableMatrixUpgrades().OrderBy(u => u.GlitchCost))
        {
            var button = new Button { Style = buttonStyle, Tag = upgrade.Id, HorizontalContentAlignment = HorizontalAlignment.Stretch };
            button.Click += MatrixUpgradeButton_Click;
            button.IsEnabled = _engine.CanAffordMatrixUpgrade(upgrade.Id);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var iconBlock = new TextBlock { Text = upgrade.Icon, FontSize = 20, Foreground = GreenBrush, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock { Text = upgrade.Name, FontWeight = FontWeights.Bold, Foreground = GreenBrush });
            stack.Children.Add(new TextBlock { Text = upgrade.Description, FontSize = 11, Foreground = LightBrush });
            stack.Children.Add(new TextBlock { Text = upgrade.FlavorText, FontSize = 10, Foreground = DimBrush, FontStyle = FontStyles.Italic });

            var costStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            costStack.Children.Add(new TextBlock { Text = $"{upgrade.GlitchCost} ⟁", FontWeight = FontWeights.Bold, Foreground = GreenBrush, HorizontalAlignment = HorizontalAlignment.Right });

            Grid.SetColumn(iconBlock, 0);
            Grid.SetColumn(stack, 1);
            Grid.SetColumn(costStack, 2);
            grid.Children.Add(iconBlock);
            grid.Children.Add(stack);
            grid.Children.Add(costStack);

            button.Content = grid;
            MatrixUpgradesPanel.Children.Add(button);
        }

        // Purchased Matrix upgrades
        PurchasedMatrixPanel.Children.Clear();
        foreach (var upgrade in _engine.GetPurchasedMatrixUpgrades())
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(10, 40, 20)),
                BorderBrush = GreenBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(10, 6, 10, 6),
                Margin = new Thickness(5)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var iconBlock = new TextBlock { Text = upgrade.Icon, FontSize = 16, Foreground = GreenBrush, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };
            var stack = new StackPanel();
            stack.Children.Add(new TextBlock { Text = upgrade.Name, FontWeight = FontWeights.Bold, Foreground = GreenBrush });
            stack.Children.Add(new TextBlock { Text = upgrade.Description, FontSize = 10, Foreground = DimBrush });

            Grid.SetColumn(iconBlock, 0);
            Grid.SetColumn(stack, 1);
            grid.Children.Add(iconBlock);
            grid.Children.Add(stack);
            border.Child = grid;
            PurchasedMatrixPanel.Children.Add(border);
        }
    }

    // === CHALLENGE MODES ===
    private void StartChallengeBtn_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string challengeId)
        {
            var challenge = ChallengeModeData.GetById(challengeId);
            if (challenge != null && System.Windows.MessageBox.Show(
                $"Start '{challenge.Name}'?\n\nThis will reset all your current progress!\n\nRules: {challenge.Rules}",
                "Start Challenge",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.Yes)
            {
                if (_engine.StartChallenge(challengeId))
                {
                    ShowToast("CHALLENGE STARTED!", challenge.Name);
                    AddNotification($"Challenge started: {challenge.Name}", GoldBrush);
                }
            }
        }
    }

    // === STATISTICS PANEL ===
    private string _lastStatsState = "";

    private void UpdateStatisticsPanel()
    {
        var state = _engine.State;

        // Only update if stats tab is selected or periodically (every few updates)
        string currentState = $"{state.TotalClicks}:{state.QuestsCompleted}:{state.UnlockedAchievements.Count}";
        if (currentState == _lastStatsState) return;
        _lastStatsState = currentState;

        // General Stats
        var playTime = TimeSpan.FromSeconds(state.TotalPlayTimeSeconds);
        StatPlaytime.Text = playTime.TotalHours >= 1
            ? $"{(int)playTime.TotalHours}h {playTime.Minutes}m"
            : $"{playTime.Minutes}m {playTime.Seconds}s";
        StatClicks.Text = NumberFormatter.FormatInteger(state.TotalClicks);
        StatCritClicks.Text = NumberFormatter.FormatInteger(state.CriticalClicks);
        StatTotalEvidence.Text = NumberFormatter.Format(state.TotalEvidenceEarned);
        StatCurrentEPS.Text = NumberFormatter.FormatPerSecond(_engine.CalculateEvidencePerSecond());
        StatAscensions.Text = NumberFormatter.FormatInteger(state.TimesAscended);

        // Production Breakdown
        double totalEps = _engine.CalculateEvidencePerSecond();
        StatProductionTotal.Text = $"Total: {NumberFormatter.FormatPerSecond(totalEps)}";

        ProductionBarsPanel.Children.Clear();
        var productions = new List<(string name, double eps, Color color)>();

        // Build generator multipliers from upgrades (same logic as GameEngine.CalculateBaseEps)
        var generatorMultipliers = new Dictionary<string, double>();
        foreach (var upgradeId in state.PurchasedUpgrades)
        {
            var upgrade = UpgradeData.GetById(upgradeId);
            if (upgrade?.Type == UpgradeType.GeneratorBoost && upgrade.TargetGeneratorId != null)
            {
                if (!generatorMultipliers.ContainsKey(upgrade.TargetGeneratorId))
                    generatorMultipliers[upgrade.TargetGeneratorId] = 1.0;
                generatorMultipliers[upgrade.TargetGeneratorId] *= upgrade.Value;
            }
        }

        // Get global EPS multiplier to match generators tab display
        var (_, globalEpsMult) = _engine.GetEpsBreakdown();

        // Calculate per-generator production with ALL multipliers (matching generators tab)
        double totalCalculatedEps = 0;
        int genIndex = 0;
        foreach (var gen in GeneratorData.AllGenerators)
        {
            int owned = state.GetGeneratorCount(gen.Id);
            if (owned > 0)
            {
                double genMultiplier = generatorMultipliers.TryGetValue(gen.Id, out var m) ? m : 1.0;
                // Also include generator-specific upgrades from GeneratorUpgradeData
                genMultiplier *= _engine.GetGeneratorUpgradeProductionMultiplier(gen.Id);
                // Apply global EPS multiplier to match generators tab
                double genEps = gen.GetProduction(owned) * genMultiplier * globalEpsMult;
                totalCalculatedEps += genEps;
                int tier = genIndex / 4;
                productions.Add((gen.Name, genEps, GetGeneratorColor(tier)));
            }
            genIndex++;
        }

        // Sort by production descending
        productions = productions.OrderByDescending(p => p.eps).ToList();

        foreach (var (name, eps, color) in productions)
        {
            double percentage = totalCalculatedEps > 0 ? eps / totalCalculatedEps : 0;

            var barContainer = new Grid { Margin = new Thickness(0, 4, 0, 4) };
            barContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            barContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            barContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });

            var nameBlock = new TextBlock
            {
                Text = name,
                FontSize = 16,
                Foreground = LightBrush,
                VerticalAlignment = VerticalAlignment.Center
            };

            var barBg = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 50)),
                CornerRadius = new CornerRadius(4),
                Height = 24,
                Margin = new Thickness(10, 0, 10, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            // Use a grid to properly size the fill relative to the container
            var barGrid = new Grid();
            var barFill = new Border
            {
                Background = new SolidColorBrush(color),
                CornerRadius = new CornerRadius(4),
                HorizontalAlignment = HorizontalAlignment.Left
            };

            // Bind the fill width to a percentage of the container
            barGrid.SizeChanged += (s, e) =>
            {
                barFill.Width = Math.Max(2, percentage * e.NewSize.Width);
            };
            barGrid.Children.Add(barFill);
            barBg.Child = barGrid;

            var valueBlock = new TextBlock
            {
                Text = $"{percentage:P0} ({NumberFormatter.FormatPerSecond(eps)})",
                FontSize = 14,
                Foreground = DimBrush,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Grid.SetColumn(nameBlock, 0);
            Grid.SetColumn(barBg, 1);
            Grid.SetColumn(valueBlock, 2);
            barContainer.Children.Add(nameBlock);
            barContainer.Children.Add(barBg);
            barContainer.Children.Add(valueBlock);

            ProductionBarsPanel.Children.Add(barContainer);
        }

        // Quest Statistics
        StatQuestsCompleted.Text = NumberFormatter.FormatInteger(state.QuestsCompleted);
        StatQuestsFailed.Text = NumberFormatter.FormatInteger(state.QuestsFailed);
        int totalQuests = state.QuestsCompleted + state.QuestsFailed;
        double successRate = totalQuests > 0 ? (double)state.QuestsCompleted / totalQuests : 0;
        StatQuestSuccessRate.Text = $"{successRate:P0}";

        // Quest success/fail bar (using star sizing for proper scaling)
        if (totalQuests > 0)
        {
            QuestSuccessColumn.Width = new GridLength(successRate, GridUnitType.Star);
            QuestFailColumn.Width = new GridLength(1 - successRate, GridUnitType.Star);
        }
        else
        {
            QuestSuccessColumn.Width = new GridLength(0);
            QuestFailColumn.Width = new GridLength(0);
        }

        // Believer Stats
        StatCurrentBelievers.Text = NumberFormatter.FormatInteger(state.Believers);
        StatBelieversLost.Text = NumberFormatter.FormatInteger(state.BelieversLost);

        // Achievement Progress
        int unlockedAchievements = state.UnlockedAchievements.Count;
        int totalAchievements = AchievementData.AllAchievements.Count;
        double achievementPercent = totalAchievements > 0 ? (double)unlockedAchievements / totalAchievements : 0;
        StatAchievementProgress.Text = $"{unlockedAchievements}/{totalAchievements}";
        StatAchievementPercent.Text = $"{achievementPercent:P0} Complete";
        AchievementFilledColumn.Width = new GridLength(achievementPercent, GridUnitType.Star);
        AchievementEmptyColumn.Width = new GridLength(1 - achievementPercent, GridUnitType.Star);

        // Prestige Stats
        StatIlluminatiTokens.Text = NumberFormatter.FormatInteger(state.IlluminatiTokens);
        StatTotalTokensEarned.Text = NumberFormatter.FormatInteger(state.TotalIlluminatiTokensEarned);
        StatMatrixBreaks.Text = NumberFormatter.FormatInteger(state.TimesMatrixBroken);
    }

    private static Color GetGeneratorColor(int tier)
    {
        return tier switch
        {
            1 => Color.FromRgb(100, 200, 100), // Green
            2 => Color.FromRgb(100, 150, 255), // Blue
            3 => Color.FromRgb(200, 100, 255), // Purple
            4 => Color.FromRgb(255, 200, 100), // Gold
            _ => Color.FromRgb(150, 150, 150)  // Gray
        };
    }

    // === MAIN MENU ===
    private void RefreshMainMenu()
    {
        var slots = _engine.SaveManager.GetAllSlotInfo();

        // Update slot 1
        UpdateSlotDisplay(1, slots[0], Slot1Info, Slot1Details, Slot1DeleteBtn, SaveSlot1Border);
        UpdateSlotDisplay(2, slots[1], Slot2Info, Slot2Details, Slot2DeleteBtn, SaveSlot2Border);
        UpdateSlotDisplay(3, slots[2], Slot3Info, Slot3Details, Slot3DeleteBtn, SaveSlot3Border);

        // Show quick continue if any save exists
        int? lastSlot = _engine.SaveManager.GetLastUsedSlot();
        QuickContinueBtn.Visibility = lastSlot.HasValue ? Visibility.Visible : Visibility.Collapsed;

        // Hide selected panel until slot is clicked
        SelectedSlotPanel.Visibility = Visibility.Collapsed;
        _selectedSlot = 0;

        // Populate challenge modes combo (only once)
        if (!_challengeComboPopulated)
        {
            PopulateChallengeCombo();
            _challengeComboPopulated = true;
        }
    }

    private void PopulateChallengeCombo()
    {
        ChallengeModeCombo.Items.Clear();

        // Add Normal Mode option with emoji
        var normalStack = new StackPanel { Orientation = Orientation.Horizontal };
        normalStack.Children.Add(new TextBlock
        {
            Text = "🎮",
            FontFamily = EmojiFont,
            FontSize = 24,
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Center
        });
        normalStack.Children.Add(new StackPanel
        {
            Children =
            {
                new TextBlock { Text = "Normal Mode", FontSize = 18, Foreground = LightBrush },
                new TextBlock { Text = "Standard gameplay, no restrictions", FontSize = 13, Foreground = DimBrush }
            }
        });
        ChallengeModeCombo.Items.Add(new ComboBoxItem { Content = normalStack, Tag = "", IsSelected = true });

        // Add all challenge modes with proper icons
        foreach (var challenge in ChallengeModeData.AllChallenges)
        {
            var stack = new StackPanel { Orientation = Orientation.Horizontal };

            // Try to get the sprite sheet icon, fall back to trophy emoji
            var iconElement = IconHelper.CreateIconWithFallback(challenge.Icon, "🏆", 28, LightBrush);
            if (iconElement is Image img)
            {
                img.Margin = new Thickness(0, 0, 10, 0);
                img.VerticalAlignment = VerticalAlignment.Center;
                stack.Children.Add(img);
            }
            else
            {
                var tb = iconElement as TextBlock;
                if (tb != null)
                {
                    tb.FontFamily = EmojiFont;
                    tb.Margin = new Thickness(0, 0, 10, 0);
                    tb.VerticalAlignment = VerticalAlignment.Center;
                }
                stack.Children.Add(iconElement);
            }

            // Add name and description
            stack.Children.Add(new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = challenge.Name, FontSize = 18, Foreground = LightBrush },
                    new TextBlock { Text = challenge.Description, FontSize = 13, Foreground = DimBrush }
                }
            });

            ChallengeModeCombo.Items.Add(new ComboBoxItem { Content = stack, Tag = challenge.Id });
        }
    }

    private void ChallengeModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Guard against event firing during XAML initialization
        if (ChallengeDescriptionText == null) return;

        if (ChallengeModeCombo.SelectedItem is ComboBoxItem item && item.Tag is string challengeId)
        {
            if (string.IsNullOrEmpty(challengeId))
            {
                ChallengeDescriptionText.Text = "Standard gameplay - no restrictions or time limits.";
            }
            else
            {
                var challenge = ChallengeModeData.GetById(challengeId);
                if (challenge != null)
                {
                    ChallengeDescriptionText.Text = $"{challenge.Description}\n{challenge.Rules}\nReward: {challenge.TinfoilReward} Tinfoil";
                }
            }
        }
    }

    private void UpdateSlotDisplay(int slot, SaveSlotInfo info, TextBlock infoText, TextBlock detailsText,
                                    Button deleteBtn, Border border)
    {
        if (info.Exists)
        {
            string challengeText = "";
            if (!string.IsNullOrEmpty(info.ActiveChallengeId))
            {
                var challenge = ChallengeModeData.GetById(info.ActiveChallengeId);
                if (challenge != null)
                {
                    challengeText = $" | {challenge.Icon} {challenge.Name}";
                }
            }

            infoText.Text = $"Evidence: {NumberFormatter.Format(info.TotalEvidence)} | Ascensions: {info.AscensionCount}{challengeText}";
            infoText.Foreground = LightBrush;

            var playTimeSpan = TimeSpan.FromSeconds(info.PlayTimeSeconds);
            string playTime = playTimeSpan.TotalHours >= 1
                ? $"{(int)playTimeSpan.TotalHours}h {playTimeSpan.Minutes}m"
                : $"{playTimeSpan.Minutes}m";
            string lastPlayed = info.LastPlayed.Date == DateTime.Today
                ? $"Today {info.LastPlayed:HH:mm}"
                : info.LastPlayed.ToString("MMM d, yyyy");
            detailsText.Text = $"Playtime: {playTime} | Last: {lastPlayed}";
            deleteBtn.Visibility = Visibility.Visible;
        }
        else
        {
            infoText.Text = "- Empty Slot -";
            infoText.Foreground = DimBrush;
            detailsText.Text = "Click to start a new game";
            deleteBtn.Visibility = Visibility.Collapsed;
        }
    }

    private void SaveSlot_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.Tag is string slotStr && int.TryParse(slotStr, out int slot))
        {
            _selectedSlot = slot;

            // Update visual selection
            SaveSlot1Border.BorderBrush = slot == 1 ? GreenBrush : new SolidColorBrush(Color.FromRgb(51, 51, 102));
            SaveSlot2Border.BorderBrush = slot == 2 ? GreenBrush : new SolidColorBrush(Color.FromRgb(51, 51, 102));
            SaveSlot3Border.BorderBrush = slot == 3 ? GreenBrush : new SolidColorBrush(Color.FromRgb(51, 51, 102));

            // Show action panel
            SelectedSlotPanel.Visibility = Visibility.Visible;
            SelectedSlotTitle.Text = $"SLOT {slot} SELECTED";

            var info = _engine.SaveManager.GetSlotInfo(slot);
            ContinueBtn.Visibility = info.Exists ? Visibility.Visible : Visibility.Collapsed;
            NewGameBtn.Content = info.Exists ? "OVERWRITE" : "NEW GAME";

            // Show challenge selection for empty slots or when overwriting
            bool showChallengeSelection = !info.Exists;
            ChallengeSelectionPanel.Visibility = showChallengeSelection ? Visibility.Visible : Visibility.Collapsed;

            // Reset challenge selection to Normal Mode
            if (ChallengeModeCombo.Items.Count > 0)
            {
                ChallengeModeCombo.SelectedIndex = 0;
            }
        }
    }

    private void DeleteSlot_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true; // Prevent bubbling to border click
        if (sender is Button btn && btn.Tag is string slotStr && int.TryParse(slotStr, out int slot))
        {
            // Simple confirmation - delete immediately
            _engine.SaveManager.DeleteSlot(slot);
            RefreshMainMenu();
            SoundManager.Play("error");
        }
    }

    private void MenuContinue_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedSlot > 0)
        {
            StartGameWithSlot(_selectedSlot, false);
        }
    }

    private void MenuNewGame_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedSlot > 0)
        {
            StartGameWithSlot(_selectedSlot, true);
        }
    }

    private void MenuQuickContinue_Click(object sender, RoutedEventArgs e)
    {
        int? lastSlot = _engine.SaveManager.GetLastUsedSlot();
        if (lastSlot.HasValue)
        {
            StartGameWithSlot(lastSlot.Value, false);
        }
    }

    private void MenuQuit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void ReturnToMenu_Click(object sender, RoutedEventArgs e)
    {
        // Save current game and return to main menu
        _engine.Stop();
        MainMenuOverlay.Visibility = Visibility.Visible;
        RefreshMainMenu();
    }

    private void StartGameWithSlot(int slot, bool isNewGame)
    {
        if (isNewGame)
        {
            _engine.NewGame(slot);

            // Start selected challenge mode if one was chosen
            if (ChallengeModeCombo.SelectedItem is ComboBoxItem item && item.Tag is string challengeId && !string.IsNullOrEmpty(challengeId))
            {
                _engine.StartChallenge(challengeId);
                var challenge = ChallengeModeData.GetById(challengeId);
                if (challenge != null)
                {
                    AddNotification($"Challenge started: {challenge.Name}!", GoldBrush);
                }
            }
        }
        else
        {
            _engine.LoadSlot(slot);
        }

        MainMenuOverlay.Visibility = Visibility.Collapsed;
        InitializeZenMode(); // Restore zen mode from saved state
        UpdateSettingsPanel(); // Sync settings UI with current state
        UpdateUI();
        _engine.Start();
        SoundManager.Play("achievement");

        // Show tutorial for new players who haven't seen it
        if (!_engine.State.HasSeenTutorial)
        {
            TutorialOverlay.Visibility = Visibility.Visible;
        }
    }

    private void SkillButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string skillId)
            _engine.UnlockSkill(skillId);
    }

    private void IlluminatiUpgradeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string upgradeId)
            _engine.PurchaseIlluminatiUpgrade(upgradeId);
    }

    private void DailyChallengeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string challengeId)
            _engine.ClaimDailyChallenge(challengeId);
    }

    private void UpdateSkillTreePanel()
    {
        var state = _engine.State;
        string currentState = $"{_engine.GetAvailableSkillPoints()}:{string.Join(",", state.UnlockedSkills)}";

        SkillPointsDisplay.Text = $"{_engine.GetAvailableSkillPoints()} / {_engine.GetTotalSkillPoints()}";

        if (currentState == _lastSkillTreeState) return;
        _lastSkillTreeState = currentState;

        var buttonStyle = (Style)FindResource("GeneratorButton");

        // Researcher branch
        ResearcherSkillsPanel.Children.Clear();
        foreach (var skill in SkillTreeData.GetByBranch(SkillBranch.Researcher).OrderBy(s => s.Tier))
        {
            var element = CreateSkillElement(skill, state, buttonStyle);
            ResearcherSkillsPanel.Children.Add(element);
        }

        // Influencer branch
        InfluencerSkillsPanel.Children.Clear();
        foreach (var skill in SkillTreeData.GetByBranch(SkillBranch.Influencer).OrderBy(s => s.Tier))
        {
            var element = CreateSkillElement(skill, state, buttonStyle);
            InfluencerSkillsPanel.Children.Add(element);
        }

        // Infiltrator branch
        InfiltratorSkillsPanel.Children.Clear();
        foreach (var skill in SkillTreeData.GetByBranch(SkillBranch.Infiltrator).OrderBy(s => s.Tier))
        {
            var element = CreateSkillElement(skill, state, buttonStyle);
            InfiltratorSkillsPanel.Children.Add(element);
        }
    }

    private UIElement CreateSkillElement(Skill skill, GameState state, Style buttonStyle)
    {
        bool unlocked = state.UnlockedSkills.Contains(skill.Id);
        bool canUnlock = _engine.CanUnlockSkill(skill.Id);

        var branchColor = skill.Branch switch
        {
            SkillBranch.Researcher => GreenBrush,
            SkillBranch.Influencer => GoldBrush,
            SkillBranch.Infiltrator => RedBrush,
            _ => DimBrush
        };

        if (unlocked)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(15, 52, 96)),
                BorderBrush = branchColor,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(10, 6, 10, 6),
                Margin = new Thickness(5)
            };
            var stack = new StackPanel();
            stack.Children.Add(new TextBlock { Text = $"{skill.Icon} {skill.Name}", FontWeight = FontWeights.Bold, Foreground = branchColor });
            stack.Children.Add(new TextBlock { Text = skill.Description, FontSize = 10, Foreground = LightBrush });
            stack.Children.Add(new TextBlock { Text = "[UNLOCKED]", FontSize = 9, Foreground = branchColor, FontWeight = FontWeights.Bold });
            border.Child = stack;
            return border;
        }
        else
        {
            var button = new Button { Style = buttonStyle, Tag = skill.Id, HorizontalContentAlignment = HorizontalAlignment.Stretch, IsEnabled = canUnlock };
            button.Click += SkillButton_Click;

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var iconBlock = new TextBlock { Text = skill.Icon, FontSize = 18, Foreground = canUnlock ? branchColor : DimBrush, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock { Text = skill.Name, FontWeight = FontWeights.Bold, Foreground = canUnlock ? branchColor : DimBrush });
            stack.Children.Add(new TextBlock { Text = skill.Description, FontSize = 10, Foreground = canUnlock ? LightBrush : DimBrush });
            if (skill.RequiredSkillId != null && !state.UnlockedSkills.Contains(skill.RequiredSkillId))
            {
                var req = SkillTreeData.GetById(skill.RequiredSkillId);
                stack.Children.Add(new TextBlock { Text = $"Requires: {req?.Name ?? skill.RequiredSkillId}", FontSize = 9, Foreground = OrangeBrush });
            }

            var costStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            costStack.Children.Add(new TextBlock { Text = $"{skill.SkillPointCost} SP", FontWeight = FontWeights.Bold, Foreground = canUnlock ? GoldBrush : DimBrush, HorizontalAlignment = HorizontalAlignment.Right });
            costStack.Children.Add(new TextBlock { Text = $"Tier {skill.Tier}", FontSize = 10, Foreground = DimBrush, HorizontalAlignment = HorizontalAlignment.Right });

            Grid.SetColumn(iconBlock, 0);
            Grid.SetColumn(stack, 1);
            Grid.SetColumn(costStack, 2);
            grid.Children.Add(iconBlock);
            grid.Children.Add(stack);
            grid.Children.Add(costStack);

            button.Content = grid;
            return button;
        }
    }

    private string _lastDailyChallengeState = "";

    private string GetDailyChallengeIconKey(ChallengeType type)
    {
        return type switch
        {
            ChallengeType.ClickCount => "challenge_clicks",
            ChallengeType.CriticalHits => "challenge_crits",
            ChallengeType.ComboCount => "challenge_combos",
            ChallengeType.CompleteQuests => "challenge_quests",
            ChallengeType.CollectEvidence => "challenge_evidence",
            _ => "challenge_clicks"
        };
    }

    private void UpdateDailyChallengesPanel()
    {
        var buttonStyle = (Style)FindResource("GeneratorButton");
        var challenges = _engine.GetDailyChallenges();

        // Build state string to check if we need to rebuild
        var stateBuilder = new System.Text.StringBuilder();
        foreach (var c in challenges)
            stateBuilder.Append($"{c.Id}:{c.Completed}:{c.Claimed}:{(int)c.Progress};");
        string currentState = stateBuilder.ToString();

        // Only rebuild if state changed
        if (currentState == _lastDailyChallengeState)
            return;
        _lastDailyChallengeState = currentState;

        DailyChallengesPanel.Children.Clear();
        if (challenges.Count == 0)
        {
            DailyChallengesPanel.Children.Add(new TextBlock { Text = "Loading daily challenges...", Foreground = DimBrush, FontStyle = FontStyles.Italic });
            return;
        }

        foreach (var challenge in challenges)
        {
            var progressPercent = challenge.Target > 0 ? Math.Min(challenge.Progress / challenge.Target, 1.0) : 0;

            if (challenge.Claimed)
            {
                var border = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(30, 50, 30)),
                    BorderBrush = GreenBrush,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(15, 9, 15, 9),
                    Margin = new Thickness(5)
                };
                var mainStack = new StackPanel { Orientation = Orientation.Horizontal };

                // Add icon
                var iconKey = GetDailyChallengeIconKey(challenge.Type);
                var iconBorder = CreateStyledIconBorder(iconKey, "✓", 48, GreenBrush);
                iconBorder.Margin = new Thickness(0, 0, 12, 0);
                iconBorder.VerticalAlignment = VerticalAlignment.Center;
                iconBorder.Opacity = 0.7;
                mainStack.Children.Add(iconBorder);

                var textStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                textStack.Children.Add(new TextBlock { Text = $"[CLAIMED] {challenge.Name}", FontWeight = FontWeights.Bold, Foreground = GreenBrush, FontSize = 21 });
                textStack.Children.Add(new TextBlock { Text = challenge.Description, FontSize = 15, Foreground = DimBrush });
                mainStack.Children.Add(textStack);

                border.Child = mainStack;
                DailyChallengesPanel.Children.Add(border);
            }
            else if (challenge.Completed)
            {
                var button = new Button { Style = buttonStyle, Tag = challenge.Id, HorizontalContentAlignment = HorizontalAlignment.Stretch };
                button.Click += DailyChallengeButton_Click;

                var mainStack = new StackPanel { Orientation = Orientation.Horizontal };

                // Add icon
                var iconKey = GetDailyChallengeIconKey(challenge.Type);
                var iconBorder = CreateStyledIconBorder(iconKey, "★", 48, GoldBrush);
                iconBorder.Margin = new Thickness(0, 0, 12, 0);
                iconBorder.VerticalAlignment = VerticalAlignment.Center;
                mainStack.Children.Add(iconBorder);

                var grid = new Grid { VerticalAlignment = VerticalAlignment.Center };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var stack = new StackPanel();
                stack.Children.Add(new TextBlock { Text = $"[COMPLETE!] {challenge.Name}", FontWeight = FontWeights.Bold, Foreground = GoldBrush, FontSize = 21 });
                stack.Children.Add(new TextBlock { Text = challenge.Description, FontSize = 15, Foreground = LightBrush });
                stack.Children.Add(new TextBlock { Text = "Click to claim reward!", FontSize = 15, Foreground = GreenBrush, FontWeight = FontWeights.Bold });

                var rewardStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                rewardStack.Children.Add(new TextBlock { Text = $"+{challenge.TinfoilReward} Tinfoil", FontWeight = FontWeights.Bold, Foreground = SilverBrush, FontSize = 21 });

                Grid.SetColumn(stack, 0);
                Grid.SetColumn(rewardStack, 1);
                grid.Children.Add(stack);
                grid.Children.Add(rewardStack);

                mainStack.Children.Add(grid);
                button.Content = mainStack;
                DailyChallengesPanel.Children.Add(button);
            }
            else
            {
                var border = new Border
                {
                    Background = DarkBrush,
                    BorderBrush = DimBrush,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(15, 9, 15, 9),
                    Margin = new Thickness(5)
                };

                var mainStack = new StackPanel { Orientation = Orientation.Horizontal };

                // Add icon
                var iconKey = GetDailyChallengeIconKey(challenge.Type);
                var iconBorder = CreateStyledIconBorder(iconKey, "○", 48, DimBrush);
                iconBorder.Margin = new Thickness(0, 0, 12, 0);
                iconBorder.VerticalAlignment = VerticalAlignment.Center;
                iconBorder.Opacity = 0.7;
                mainStack.Children.Add(iconBorder);

                var textStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                textStack.Children.Add(new TextBlock { Text = challenge.Name, FontWeight = FontWeights.Bold, Foreground = LightBrush, FontSize = 21 });
                textStack.Children.Add(new TextBlock { Text = challenge.Description, FontSize = 15, Foreground = DimBrush });

                var progressBar = new Border { Background = new SolidColorBrush(Color.FromRgb(30, 30, 40)), CornerRadius = new CornerRadius(5), Height = 12, Margin = new Thickness(0, 6, 0, 0), HorizontalAlignment = HorizontalAlignment.Stretch, ClipToBounds = true };
                var progressGrid = new Grid();
                progressGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(progressPercent, GridUnitType.Star) });
                progressGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1 - progressPercent, GridUnitType.Star) });
                var progressFill = new Border { Background = GreenBrush, CornerRadius = new CornerRadius(5, 0, 0, 5) };
                Grid.SetColumn(progressFill, 0);
                progressGrid.Children.Add(progressFill);
                progressBar.Child = progressGrid;
                textStack.Children.Add(progressBar);

                textStack.Children.Add(new TextBlock { Text = $"{NumberFormatter.Format(challenge.Progress)} / {NumberFormatter.Format(challenge.Target)} ({progressPercent:P0})", FontSize = 15, Foreground = DimBrush, Margin = new Thickness(0, 3, 0, 0) });
                textStack.Children.Add(new TextBlock { Text = $"Reward: +{challenge.TinfoilReward} Tinfoil", FontSize = 15, Foreground = SilverBrush });

                mainStack.Children.Add(textStack);
                border.Child = mainStack;
                DailyChallengesPanel.Children.Add(border);
            }
        }
    }

    private void UpdatePrestigePanel()
    {
        var state = _engine.State;
        string currentState = $"{state.IlluminatiTokens}:{_engine.GetPurchasedIlluminatiUpgrades().Count()}:{(int)(state.TotalEvidenceEarned / 1e9)}";

        // Always update prestige info text (cheap operation)
        bool canPrestige = _engine.CanPrestige();
        int tokensFromPrestige = _engine.GetTokensFromPrestige();

        if (canPrestige)
        {
            PrestigeInfoText.Text = $"You have earned {NumberFormatter.Format(state.TotalEvidenceEarned)} total evidence.\n" +
                                    $"Ascending will grant you {tokensFromPrestige} Illuminati Token(s).\n" +
                                    $"Tokens unlock upgrades with MASSIVE permanent multipliers (100x-500x EPS)!";
            PrestigeButton.IsEnabled = true;
        }
        else
        {
            double threshold = PrestigeData.PRESTIGE_THRESHOLD;
            double progress = state.TotalEvidenceEarned / threshold;
            PrestigeInfoText.Text = $"Progress: {NumberFormatter.Format(state.TotalEvidenceEarned)} / {NumberFormatter.Format(threshold)} ({progress:P1})\n" +
                                    $"Ascending grants Illuminati Tokens for MASSIVE permanent multipliers!";
            PrestigeButton.IsEnabled = false;
        }

        IlluminatiTokensText.Text = $"You have {state.IlluminatiTokens} Illuminati Token(s)";

        // Only rebuild panels if state changed
        if (currentState == _lastPrestigeState) return;
        _lastPrestigeState = currentState;

        var buttonStyle = (Style)FindResource("GeneratorButton");

        // Available upgrades
        IlluminatiUpgradesPanel.Children.Clear();
        foreach (var upgrade in _engine.GetAvailableIlluminatiUpgrades())
        {
            var button = new Button { Style = buttonStyle, Tag = upgrade.Id, HorizontalContentAlignment = HorizontalAlignment.Stretch };
            button.Click += IlluminatiUpgradeButton_Click;
            button.IsEnabled = _engine.CanAffordIlluminatiUpgrade(upgrade.Id);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var iconBlock = CreateStyledIconBorder(upgrade.Id, upgrade.Icon, 56, PurpleBrush);
            iconBlock.Margin = new Thickness(0, 0, 12, 0);
            iconBlock.VerticalAlignment = VerticalAlignment.Center;

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock { Text = upgrade.Name, FontWeight = FontWeights.Bold, Foreground = PurpleBrush, FontSize = 18 });
            stack.Children.Add(new TextBlock { Text = upgrade.Description, FontSize = 14, Foreground = LightBrush });
            stack.Children.Add(new TextBlock { Text = upgrade.FlavorText, FontSize = 12, Foreground = DimBrush, FontStyle = FontStyles.Italic });

            var costStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            costStack.Children.Add(new TextBlock { Text = $"{upgrade.TokenCost} Token(s)", FontWeight = FontWeights.Bold, Foreground = PurpleBrush, HorizontalAlignment = HorizontalAlignment.Right, FontSize = 18 });

            Grid.SetColumn(iconBlock, 0);
            Grid.SetColumn(stack, 1);
            Grid.SetColumn(costStack, 2);
            grid.Children.Add(iconBlock);
            grid.Children.Add(stack);
            grid.Children.Add(costStack);

            button.Content = grid;
            IlluminatiUpgradesPanel.Children.Add(button);
        }

        if (IlluminatiUpgradesPanel.Children.Count == 0)
            IlluminatiUpgradesPanel.Children.Add(new TextBlock { Text = "All upgrades purchased!", Foreground = PurpleBrush, FontStyle = FontStyles.Italic });

        // Purchased upgrades
        PurchasedIlluminatiPanel.Children.Clear();
        foreach (var upgrade in _engine.GetPurchasedIlluminatiUpgrades())
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(42, 26, 78)),
                BorderBrush = PurpleBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(10, 6, 10, 6),
                Margin = new Thickness(5)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var iconBlock = CreateStyledIconBorder(upgrade.Id, upgrade.Icon, 40, PurpleBrush);
            iconBlock.Margin = new Thickness(0, 0, 10, 0);
            iconBlock.VerticalAlignment = VerticalAlignment.Center;
            var stack = new StackPanel();
            stack.Children.Add(new TextBlock { Text = upgrade.Name, FontWeight = FontWeights.Bold, Foreground = PurpleBrush });
            stack.Children.Add(new TextBlock { Text = upgrade.Description, FontSize = 10, Foreground = DimBrush });

            Grid.SetColumn(iconBlock, 0);
            Grid.SetColumn(stack, 1);
            grid.Children.Add(iconBlock);
            grid.Children.Add(stack);
            border.Child = grid;
            PurchasedIlluminatiPanel.Children.Add(border);
        }

        if (PurchasedIlluminatiPanel.Children.Count == 0)
            PurchasedIlluminatiPanel.Children.Add(new TextBlock { Text = "No upgrades purchased yet", Foreground = DimBrush, FontStyle = FontStyles.Italic });
    }

    private void SoundToggle_Click(object sender, RoutedEventArgs e)
    {
        SoundManager.Enabled = !SoundManager.Enabled;
        _settings.SoundEnabled = SoundManager.Enabled;
        _settings.Save();
        UpdateSoundIcon();
        UpdateSettingsPanel();
    }

    // === SETTINGS TAB ===
    private void SettingsSoundToggle_Click(object sender, RoutedEventArgs e)
    {
        SoundManager.Enabled = !SoundManager.Enabled;
        _settings.SoundEnabled = SoundManager.Enabled;
        _settings.Save();
        UpdateSoundIcon();
        UpdateSettingsPanel();
    }

    private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded) return;
        double volume = e.NewValue / 100.0;
        SoundManager.Volume = volume;
        _settings.SoundVolume = volume;
        _settings.Save();
        VolumePercentText.Text = $"{(int)e.NewValue}%";
    }

    private void ClickVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded) return;
        double volume = e.NewValue / 100.0;
        SoundManager.ClickVolume = volume;
        _settings.ClickVolume = volume;
        _settings.Save();
        ClickVolumePercentText.Text = $"{(int)e.NewValue}%";
    }

    private void SettingsZenToggle_Click(object sender, RoutedEventArgs e)
    {
        _engine.State.ZenMode = !_engine.State.ZenMode;
        InitializeZenMode();
        UpdateSettingsPanel();
    }

    private void ShowTutorial_Click(object sender, RoutedEventArgs e)
    {
        TutorialOverlay.Visibility = Visibility.Visible;
    }

    private void TutorialClose_Click(object sender, RoutedEventArgs e)
    {
        TutorialOverlay.Visibility = Visibility.Collapsed;
        _engine.State.HasSeenTutorial = true;
    }

    private void UpdateSettingsPanel()
    {
        // Sound toggle
        bool soundOn = SoundManager.Enabled;
        SettingsSoundToggle.Content = soundOn ? "ON" : "OFF";
        SettingsSoundToggle.Background = soundOn ? new SolidColorBrush(Color.FromRgb(34, 85, 34)) : new SolidColorBrush(Color.FromRgb(85, 34, 34));
        SettingsSoundToggle.Foreground = soundOn ? GreenBrush : RedBrush;
        SettingsSoundToggle.BorderBrush = soundOn ? GreenBrush : RedBrush;

        // Effects volume slider
        VolumeSlider.Value = SoundManager.Volume * 100;
        VolumePercentText.Text = $"{(int)(SoundManager.Volume * 100)}%";

        // Click volume slider
        ClickVolumeSlider.Value = SoundManager.ClickVolume * 100;
        ClickVolumePercentText.Text = $"{(int)(SoundManager.ClickVolume * 100)}%";

        // Zen mode toggle
        bool zenOn = _engine.State.ZenMode;
        SettingsZenToggle.Content = zenOn ? "ON" : "OFF";
        SettingsZenToggle.Background = zenOn ? new SolidColorBrush(Color.FromRgb(34, 85, 34)) : new SolidColorBrush(Color.FromRgb(85, 34, 34));
        SettingsZenToggle.Foreground = zenOn ? GreenBrush : RedBrush;
        SettingsZenToggle.BorderBrush = zenOn ? GreenBrush : RedBrush;
    }

    // === FULLSCREEN ===
    private bool _isFullscreen = false;
    private WindowState _previousWindowState;
    private WindowStyle _previousWindowStyle;
    private ResizeMode _previousResizeMode;
    private Rect _previousBounds;

    private void FullscreenToggle_Click(object sender, RoutedEventArgs e)
    {
        ToggleFullscreen();
    }

    private void ToggleFullscreen()
    {
        if (_isFullscreen)
        {
            // Exit fullscreen
            WindowStyle = _previousWindowStyle;
            ResizeMode = _previousResizeMode;
            WindowState = _previousWindowState;
            Left = _previousBounds.Left;
            Top = _previousBounds.Top;
            Width = _previousBounds.Width;
            Height = _previousBounds.Height;
            Topmost = false;
            _isFullscreen = false;
        }
        else
        {
            // Enter fullscreen - save current state first
            _previousWindowState = WindowState;
            _previousWindowStyle = WindowStyle;
            _previousResizeMode = ResizeMode;

            // If maximized, get the restore bounds; otherwise use current bounds
            if (WindowState == WindowState.Maximized)
            {
                _previousBounds = new Rect(RestoreBounds.Left, RestoreBounds.Top, RestoreBounds.Width, RestoreBounds.Height);
            }
            else
            {
                _previousBounds = new Rect(Left, Top, Width, Height);
            }

            // Set fullscreen - order matters!
            WindowState = WindowState.Normal; // Must be normal first to set size
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            Topmost = true; // Ensures we're above taskbar

            // Use primary screen full bounds (including taskbar area)
            Left = 0;
            Top = 0;
            Width = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;

            _isFullscreen = true;
        }
        UpdateFullscreenIcon();
    }

    private void UpdateFullscreenIcon()
    {
        if (TryFindResource(_isFullscreen ? "Icon_fullscreen_exit" : "Icon_fullscreen") is ImageSource icon)
        {
            FullscreenToggleIcon.Source = icon;
        }
        FullscreenToggleButton.ToolTip = _isFullscreen ? "Exit Fullscreen (F11)" : "Toggle Fullscreen (F11)";
    }

    // === ZEN MODE ===
    private bool _zenMode = false;

    private void InitializeZenMode()
    {
        _zenMode = _engine.State.ZenMode;
        UpdateZenModeIcon();
    }

    private void ZenModeToggle_Click(object sender, RoutedEventArgs e)
    {
        _zenMode = !_zenMode;
        _engine.State.ZenMode = _zenMode; // Save to game state
        UpdateZenModeIcon();

        if (_zenMode)
        {
            // Clear any active random events when entering zen mode
            ClearActiveEvents();
            ShowToast("ZEN MODE", "Random events disabled");
        }
        else
        {
            ShowToast("ZEN MODE OFF", "Random events enabled");
        }
    }

    private void ClearActiveEvents()
    {
        // Clear golden eye state and restore normal eye visuals
        if (_engine.State.GoldenEyeActive)
        {
            _engine.State.GoldenEyeActive = false;
            OnGoldenEyeEnd(); // Properly restore eye visuals
        }

        // Clear debunker
        if (_activeDebunker != null)
        {
            if (InteractiveCanvas.Children.Contains(_activeDebunker.Element))
                InteractiveCanvas.Children.Remove(_activeDebunker.Element);
            _activeDebunker = null;
        }

        // Clear evidence thief
        if (_activeEvidenceThief != null)
        {
            if (InteractiveCanvas.Children.Contains(_activeEvidenceThief.Element))
                InteractiveCanvas.Children.Remove(_activeEvidenceThief.Element);
            _activeEvidenceThief = null;
        }

        // Clear tinfoil thief
        if (_activeTinfoilThief != null)
        {
            if (InteractiveCanvas.Children.Contains(_activeTinfoilThief.Element))
                InteractiveCanvas.Children.Remove(_activeTinfoilThief.Element);
            _activeTinfoilThief = null;
        }

        // Clear lucky drops from InteractiveCanvas
        foreach (var drop in _luckyDrops.ToList())
        {
            if (InteractiveCanvas.Children.Contains(drop.Element))
                InteractiveCanvas.Children.Remove(drop.Element);
        }
        _luckyDrops.Clear();

        // Clear escaped document
        if (_escapedDocument != null)
        {
            if (InteractiveCanvas.Children.Contains(_escapedDocument.Element))
                InteractiveCanvas.Children.Remove(_escapedDocument.Element);
            _escapedDocument = null;
        }

        // Clear evidence trail
        foreach (var orb in _evidenceTrail.ToList())
        {
            if (InteractiveCanvas.Children.Contains(orb.Element))
                InteractiveCanvas.Children.Remove(orb.Element);
        }
        _evidenceTrail.Clear();

        // Clear connection pins
        foreach (var pin in _connectionPins.ToList())
        {
            if (InteractiveCanvas.Children.Contains(pin.Element))
                InteractiveCanvas.Children.Remove(pin.Element);
        }
        _connectionPins.Clear();

        // Reset spawn timers so events don't spawn immediately when zen mode is turned off
        _debunkerSpawnTimer = 0;
        _evidenceThiefTimer = 0;
        _tinfoilThiefTimer = 0;
        _luckyDropTimer = 0;
        _specialEventTimer = 0;
    }

    private void UpdateZenModeIcon()
    {
        if (TryFindResource(_zenMode ? "Icon_zen" : "Icon_zen_off") is ImageSource icon)
        {
            ZenModeToggleIcon.Source = icon;
        }
        ZenModeToggleButton.ToolTip = _zenMode ? "Zen Mode ON - Click to enable random events" : "Zen Mode - Disable random events";
    }

    // === MINIGAMES ===
    private void SpawnRandomMinigame()
    {
        var types = new[] { MinigameType.ClickFrenzy, MinigameType.DocumentCatch, MinigameType.MemoryMatrix };
        _currentMinigame = types[_random.Next(types.Length)];

        MinigameOverlay.Visibility = Visibility.Visible;
        MinigameStartButton.Visibility = Visibility.Visible;
        MinigameCloseButton.Visibility = Visibility.Visible;
        MinigameCanvas.Children.Clear();
        _minigameElements.Clear();
        _minigameActive = false;
        _minigameScore = 0;

        switch (_currentMinigame)
        {
            case MinigameType.ClickFrenzy:
                MinigameTitle.Text = "🔥 CLICK FRENZY";
                MinigameInstructions.Text = "Click as fast as possible! 10 seconds.";
                break;
            case MinigameType.DocumentCatch:
                MinigameTitle.Text = "📄 DOCUMENT CATCH";
                MinigameInstructions.Text = "Click the falling documents before they escape! 15 seconds.";
                break;
            case MinigameType.MemoryMatrix:
                MinigameTitle.Text = "🧠 MEMORY MATRIX";
                MinigameInstructions.Text = $"Watch and repeat the pattern! ({_memoryDifficulty} cells)";
                break;
        }

        MinigameStatus.Text = _currentMinigame == MinigameType.MemoryMatrix
            ? $"Difficulty: {_memoryDifficulty}/12 - Higher = Better rewards!"
            : "Ready!";
        SoundManager.Play("achievement");

        string minigameName = _currentMinigame switch
        {
            MinigameType.ClickFrenzy => "Document Shredder",
            MinigameType.DocumentCatch => "Document Catch",
            MinigameType.MemoryMatrix => "Security Access",
            _ => "Minigame"
        };
        AddNotification($"🎮 {minigameName} minigame appeared!", PurpleBrush);
    }

    private void MinigameStart_Click(object sender, RoutedEventArgs e)
    {
        MinigameStartButton.Visibility = Visibility.Collapsed;
        MinigameCloseButton.Visibility = Visibility.Collapsed;
        _minigameActive = true;
        _minigameScore = 0;

        switch (_currentMinigame)
        {
            case MinigameType.ClickFrenzy:
                _minigameTimer = 10.0;
                SetupClickFrenzy();
                break;
            case MinigameType.DocumentCatch:
                _minigameTimer = 15.0;
                break;
            case MinigameType.MemoryMatrix:
                _minigameTimer = 0;
                SetupMemoryMatrix();
                break;
        }
    }

    private void MinigameClose_Click(object sender, RoutedEventArgs e)
    {
        MinigameOverlay.Visibility = Visibility.Collapsed;
        _minigameActive = false;
        _currentMinigame = MinigameType.None;
        MinigameCanvas.Children.Clear();
        _minigameElements.Clear();
    }

    private void SetupClickFrenzy()
    {
        // "Document Shredder" themed - shred leaked documents
        var shredderPanel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };

        // Document icon that changes with each click
        var docText = new TextBlock
        {
            Text = "📄",
            FontSize = 50,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontFamily = EmojiFont,
            Margin = new Thickness(0, 0, 0, 5)
        };
        shredderPanel.Children.Add(docText);

        var target = new Button
        {
            Width = 180,
            Height = 80,
            Background = new SolidColorBrush(Color.FromRgb(40, 20, 20)),
            Foreground = RedBrush,
            BorderBrush = new SolidColorBrush(Color.FromRgb(100, 50, 50)),
            BorderThickness = new Thickness(3),
            Cursor = Cursors.Hand,
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Content = "🔥 SHRED DOCUMENT 🔥"
        };
        target.Click += ClickFrenzyTarget_Click;
        shredderPanel.Children.Add(target);

        shredderPanel.Children.Add(new TextBlock
        {
            Text = "[CLASSIFIED LEAKS DETECTED]",
            FontSize = 11,
            Foreground = RedBrush,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontFamily = new FontFamily("Consolas"),
            Margin = new Thickness(0, 8, 0, 0)
        });

        Canvas.SetLeft(shredderPanel, (MinigameCanvas.Width - 180) / 2);
        Canvas.SetTop(shredderPanel, (MinigameCanvas.Height - 160) / 2);
        MinigameCanvas.Children.Add(shredderPanel);
    }

    private void ClickFrenzyTarget_Click(object sender, RoutedEventArgs e)
    {
        if (!_minigameActive) return;
        _minigameScore++;
        MinigameStatus.Text = $"Documents Shredded: {_minigameScore}";
        SoundManager.Play("click");
    }

    private void SetupMemoryMatrix()
    {
        MinigameCanvas.Children.Clear();
        _memoryPattern = new int[_memoryDifficulty]; // Use current difficulty
        _memoryIndex = 0;

        // "Security Keypad" themed - enter access code
        var keypadPanel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };

        // Header
        keypadPanel.Children.Add(new TextBlock
        {
            Text = "🔐 SECURITY ACCESS",
            FontSize = 14,
            Foreground = GreenBrush,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontWeight = FontWeights.Bold,
            FontFamily = new FontFamily("Consolas"),
            Margin = new Thickness(0, 0, 0, 8)
        });

        // Create 3x3 numeric keypad (1-9)
        var keypadGrid = new Grid();
        for (int i = 0; i < 3; i++)
        {
            keypadGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(55) });
            keypadGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
        }

        for (int i = 0; i < 9; i++)
        {
            int row = i / 3;
            int col = i % 3;
            int keyNum = i + 1; // 1-9

            var cell = new Border
            {
                Width = 60,
                Height = 45,
                Background = new SolidColorBrush(Color.FromRgb(25, 25, 40)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(60, 60, 80)),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(6),
                Tag = i,
                Cursor = Cursors.Hand,
                Margin = new Thickness(3),
                Child = new TextBlock
                {
                    Text = keyNum.ToString(),
                    FontSize = 22,
                    Foreground = LightBrush,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.Bold,
                    FontFamily = new FontFamily("Consolas")
                }
            };
            cell.MouseDown += MemoryCell_Click;

            Grid.SetRow(cell, row);
            Grid.SetColumn(cell, col);
            keypadGrid.Children.Add(cell);
            _minigameElements.Add(cell);
        }

        keypadPanel.Children.Add(keypadGrid);

        Canvas.SetLeft(keypadPanel, (MinigameCanvas.Width - 210) / 2);
        Canvas.SetTop(keypadPanel, 5);
        MinigameCanvas.Children.Add(keypadPanel);

        // Generate random pattern
        for (int i = 0; i < _memoryPattern.Length; i++)
            _memoryPattern[i] = _random.Next(9);

        // Show pattern with delay
        MinigameStatus.Text = "Memorize the access code...";
        ShowMemoryPattern(0);
    }

    private async void ShowMemoryPattern(int index)
    {
        if (index >= _memoryPattern!.Length)
        {
            MinigameStatus.Text = "Enter the access code!";
            _minigameTimer = 10.0;
            return;
        }

        int cellIndex = _memoryPattern[index];
        if (cellIndex < _minigameElements.Count && _minigameElements[cellIndex] is Border cell)
        {
            cell.Background = GreenBrush;
            await Task.Delay(400);
            cell.Background = new SolidColorBrush(Color.FromRgb(25, 25, 40));
            await Task.Delay(200);
        }

        ShowMemoryPattern(index + 1);
    }

    private void MemoryCell_Click(object sender, MouseButtonEventArgs e)
    {
        if (!_minigameActive || _memoryPattern == null || MinigameStatus.Text == "Watch the pattern...") return;

        if (sender is Border cell && cell.Tag is int cellIndex)
        {
            if (_memoryIndex < _memoryPattern.Length && cellIndex == _memoryPattern[_memoryIndex])
            {
                _memoryIndex++;
                cell.Background = GreenBrush;
                SoundManager.Play("click");

                // Fade back
                var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
                timer.Tick += (s, ev) =>
                {
                    cell.Background = new SolidColorBrush(Color.FromRgb(25, 25, 40));
                    timer.Stop();
                };
                timer.Start();

                if (_memoryIndex >= _memoryPattern.Length)
                {
                    _minigameScore = _memoryPattern.Length;
                    EndMinigame(true);
                }
            }
            else
            {
                // Wrong cell
                cell.Background = RedBrush;
                SoundManager.Play("error");
                EndMinigame(false);
            }
        }
    }

    private void UpdateMinigame(double deltaTime)
    {
        if (!_minigameActive) return;

        if (_currentMinigame == MinigameType.ClickFrenzy || _currentMinigame == MinigameType.DocumentCatch)
        {
            _minigameTimer -= deltaTime;
            MinigameStatus.Text = _currentMinigame == MinigameType.ClickFrenzy
                ? $"Time: {_minigameTimer:F1}s | Clicks: {_minigameScore}"
                : $"Time: {_minigameTimer:F1}s | Caught: {_minigameScore}";

            if (_currentMinigame == MinigameType.DocumentCatch)
                UpdateDocumentCatch(deltaTime);

            if (_minigameTimer <= 0)
            {
                EndMinigame(true);
            }
        }
        else if (_currentMinigame == MinigameType.MemoryMatrix && _minigameTimer > 0)
        {
            _minigameTimer -= deltaTime;
            if (_minigameTimer <= 0)
            {
                EndMinigame(false);
            }
        }
    }

    private double _docSpawnTimer = 0;
    private void UpdateDocumentCatch(double deltaTime)
    {
        _docSpawnTimer += deltaTime;
        if (_docSpawnTimer > 0.5) // Spawn every 0.5 seconds
        {
            _docSpawnTimer = 0;
            SpawnDocument();
        }

        // Update falling documents
        var toRemove = new List<UIElement>();
        foreach (var element in _minigameElements.ToList())
        {
            if (element is Button doc)
            {
                double top = Canvas.GetTop(doc);
                top += deltaTime * 120; // Fall speed
                Canvas.SetTop(doc, top);

                if (top > MinigameCanvas.Height)
                {
                    toRemove.Add(doc);
                }
            }
        }

        foreach (var element in toRemove)
        {
            MinigameCanvas.Children.Remove(element);
            _minigameElements.Remove(element);
        }
    }

    private void SpawnDocument()
    {
        var doc = new Button
        {
            Content = _random.Next(3) == 0 ? "📜" : "📄",
            FontSize = 28,
            Width = 50,
            Height = 50,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand
        };
        doc.Click += Document_Click;

        Canvas.SetLeft(doc, _random.NextDouble() * (MinigameCanvas.Width - 50));
        Canvas.SetTop(doc, -50);
        MinigameCanvas.Children.Add(doc);
        _minigameElements.Add(doc);
    }

    private void Document_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button doc)
        {
            _minigameScore++;
            MinigameCanvas.Children.Remove(doc);
            _minigameElements.Remove(doc);
            SoundManager.Play("drop");
        }
    }

    private void EndMinigame(bool success)
    {
        _minigameActive = false;
        MinigameStartButton.Visibility = Visibility.Collapsed;
        MinigameCloseButton.Visibility = Visibility.Visible;

        double reward = 0;
        int tinfoil = 0;

        if (success)
        {
            switch (_currentMinigame)
            {
                case MinigameType.ClickFrenzy:
                    // Score based on clicks per second
                    double cps = _minigameScore / 10.0;
                    reward = _engine.CalculateEvidencePerSecond() * 30 * cps;
                    tinfoil = Math.Max(1, _minigameScore / 10);
                    MinigameStatus.Text = $"Amazing! {_minigameScore} clicks ({cps:F1} CPS)";
                    break;
                case MinigameType.DocumentCatch:
                    reward = _engine.CalculateEvidencePerSecond() * 20 * _minigameScore;
                    tinfoil = _minigameScore / 2;
                    MinigameStatus.Text = $"Great! Caught {_minigameScore} documents!";
                    break;
                case MinigameType.MemoryMatrix:
                    // Rewards scale with difficulty: base * (difficulty / 4)^2
                    double difficultyMultiplier = Math.Pow(_memoryDifficulty / 4.0, 2);
                    reward = _engine.CalculateEvidencePerSecond() * 60 * difficultyMultiplier;
                    tinfoil = (int)(_memoryDifficulty * 3 * difficultyMultiplier);
                    MinigameStatus.Text = $"Perfect! Level {_memoryDifficulty} complete!";

                    // Increase difficulty for next time (max 12)
                    if (_memoryDifficulty < 12)
                    {
                        _memoryDifficulty++;
                        MinigameStatus.Text += $"\nNext level: {_memoryDifficulty} cells!";
                    }
                    else
                    {
                        MinigameStatus.Text += "\nMAX LEVEL MASTERED!";
                        tinfoil += 25; // Bonus for max level
                    }
                    break;
            }

            SoundManager.Play("achievement");
        }
        else
        {
            if (_currentMinigame == MinigameType.MemoryMatrix)
            {
                // Decrease difficulty for next time (min 3)
                if (_memoryDifficulty > 3)
                {
                    _memoryDifficulty--;
                    MinigameStatus.Text = $"Failed! Next time: {_memoryDifficulty} cells.";
                }
                else
                {
                    MinigameStatus.Text = "Failed! Keep practicing at level 3.";
                }
            }
            else
            {
                MinigameStatus.Text = "Better luck next time!";
            }
            SoundManager.Play("error");
        }

        if (reward > 0 || tinfoil > 0)
        {
            _engine.State.Evidence += reward;
            _engine.State.TotalEvidenceEarned += reward;
            _engine.State.Tinfoil += tinfoil;

            string rewardText = "";
            if (reward > 0) rewardText += $"+{NumberFormatter.Format(reward)} evidence ";
            if (tinfoil > 0) rewardText += $"+{tinfoil} tinfoil";
            MinigameStatus.Text += $"\n{rewardText}";

            AddNotification($"🎮 Minigame complete! {rewardText.Trim()}", GoldBrush);
        }
        else
        {
            AddNotification("🎮 Minigame failed - better luck next time!", DimBrush);
        }

        MinigameCloseButton.Content = "Claim & Close";
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        _engine.Stop();
        SaveWindowSettings();
    }

    private void ApplyWindowSettings()
    {
        // Apply window size
        if (_settings.WindowWidth > 0 && _settings.WindowHeight > 0)
        {
            this.Width = _settings.WindowWidth;
            this.Height = _settings.WindowHeight;
        }

        // Apply window position (if not default)
        if (_settings.WindowLeft >= 0 && _settings.WindowTop >= 0)
        {
            this.Left = _settings.WindowLeft;
            this.Top = _settings.WindowTop;
            this.WindowStartupLocation = WindowStartupLocation.Manual;
        }

        // Apply maximized state
        if (_settings.WindowMaximized)
        {
            this.WindowState = WindowState.Maximized;
        }
    }

    private void SaveWindowSettings()
    {
        // Save window state
        _settings.WindowMaximized = this.WindowState == WindowState.Maximized;

        // Only save size/position if not maximized
        if (this.WindowState == WindowState.Normal)
        {
            _settings.WindowWidth = this.Width;
            _settings.WindowHeight = this.Height;
            _settings.WindowLeft = this.Left;
            _settings.WindowTop = this.Top;
        }

        _settings.Save();
    }
}
