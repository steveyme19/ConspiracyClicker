using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
    private readonly Random _random = new();
    private readonly Dictionary<string, Button> _generatorButtons = new();
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
    private readonly Dictionary<string, (double cost, int owned, double prod)> _lastGenState = new();
    private int _lastTinfoilCount = -1;
    private int _lastQuestCount = -1;

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

    // Pupil size constants for combo visual
    private const double PUPIL_MIN_SIZE = 60;
    private const double PUPIL_MAX_SIZE = 150; // Well under outer circle (180) - triggers before reaching edge

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
    private static readonly string[] AmbientSymbols = { "📄", "🔺", "👁", "❓", "📎", "🔗" };

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
    private bool _gameStarted = false;

    public MainWindow()
    {
        InitializeComponent();

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

        // Hook into rendering for smooth 60fps animations
        CompositionTarget.Rendering += OnRendering;

        // Initialize sound system
        SoundManager.Initialize();

        InitializeGeneratorButtons();

        // Show main menu on startup
        RefreshMainMenu();

        // Add keyboard shortcuts
        this.KeyDown += MainWindow_KeyDown;
        this.Focusable = true;
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
        SoundToggleIcon.Text = SoundManager.IsMuted ? "🔇" : "🔊";
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        double deltaTime = (now - _lastRenderTime).TotalSeconds;
        _lastRenderTime = now;

        // Cap delta time to avoid huge jumps
        if (deltaTime > 0.1) deltaTime = 0.1;

        UpdateOrbits(deltaTime);
        UpdateAmbientParticles(deltaTime);
        UpdateNewsTicker(deltaTime);
        UpdateLuckyDrops(deltaTime);
        UpdateDebunker(deltaTime);
        UpdateEvidenceThief(deltaTime);
        UpdateTinfoilThief(deltaTime);
        UpdateSpecialEvents(deltaTime);
        UpdateMinigame(deltaTime);

        // Random minigame spawn (every 2-4 minutes)
        if (!_minigameActive && MinigameOverlay.Visibility != Visibility.Visible)
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

        // Spawn new drops randomly
        _luckyDropTimer += deltaTime;
        if (_luckyDropTimer > 8 + _random.NextDouble() * 12 && _luckyDrops.Count < 3)
        {
            _luckyDropTimer = 0;
            SpawnLuckyDrop(width, height);
        }

        // Update existing drops using reusable list
        _dropsToRemove.Clear();
        foreach (var drop in _luckyDrops)
        {
            drop.Life -= deltaTime;
            drop.X += drop.VelocityX * deltaTime;
            drop.Y += drop.VelocityY * deltaTime;

            // Gentle floating motion
            drop.VelocityY += Math.Sin(drop.Life * 3) * 0.5;

            if (drop.Life <= 0 || drop.X < -50 || drop.X > width + 50)
            {
                _dropsToRemove.Add(drop);
                continue;
            }

            Canvas.SetLeft(drop.Element, drop.X);
            Canvas.SetTop(drop.Element, drop.Y);

            // Pulse effect
            drop.Element.Opacity = 0.8 + 0.2 * Math.Sin(drop.Life * 5);
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
                ShowToast("😱", $"Debunked! Lost {NumberFormatter.Format(loss)} believers!");
                AddNotification($"🕵️ Debunker escaped! Lost {NumberFormatter.Format(loss)} believers.", RedBrush);
                SoundManager.Play("error");
                InteractiveCanvas.Children.Remove(_activeDebunker.Element);
                _activeDebunker = null;
            }
        }
        else
        {
            // Maybe spawn a new debunker (not during minigames)
            if (!_minigameActive && MinigameOverlay.Visibility != Visibility.Visible)
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
            TimeLeft = 5.0,
            ClicksRequired = 10,
            ClicksReceived = 0
        };

        ShowToast("🚨", "DEBUNKER INCOMING! Click to defeat them!");
        AddNotification("🚨 DEBUNKER ALERT! Click 10 times in 5 seconds to defeat!", OrangeBrush);
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

            // Bounce off edges
            if (_activeEvidenceThief.X < 10)
            {
                _activeEvidenceThief.X = 10;
                _activeEvidenceThief.VelocityX = Math.Abs(_activeEvidenceThief.VelocityX);
            }
            if (_activeEvidenceThief.X > width - 90)
            {
                _activeEvidenceThief.X = width - 90;
                _activeEvidenceThief.VelocityX = -Math.Abs(_activeEvidenceThief.VelocityX);
            }
            if (_activeEvidenceThief.Y < 10)
            {
                _activeEvidenceThief.Y = 10;
                _activeEvidenceThief.VelocityY = Math.Abs(_activeEvidenceThief.VelocityY);
            }
            if (_activeEvidenceThief.Y > height - 70)
            {
                _activeEvidenceThief.Y = height - 70;
                _activeEvidenceThief.VelocityY = -Math.Abs(_activeEvidenceThief.VelocityY);
            }

            Canvas.SetLeft(_activeEvidenceThief.Element, _activeEvidenceThief.X);
            Canvas.SetTop(_activeEvidenceThief.Element, _activeEvidenceThief.Y);

            // Update timer display
            var timerText = _activeEvidenceThief.Element.Child as StackPanel;
            if (timerText?.Children.Count > 1 && timerText.Children[1] is TextBlock tb)
            {
                tb.Text = $"⏱️ {_activeEvidenceThief.TimeLeft:F1}s - CATCH ME!";
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
            // Maybe spawn a new evidence thief
            if (!_minigameActive && MinigameOverlay.Visibility != Visibility.Visible && _activeDebunker == null)
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

        // Start from a random edge
        double startX, startY;
        int edge = _random.Next(4);
        switch (edge)
        {
            case 0: startX = 10; startY = _random.NextDouble() * (height - 80); break;
            case 1: startX = width - 90; startY = _random.NextDouble() * (height - 80); break;
            case 2: startX = _random.NextDouble() * (width - 100); startY = 10; break;
            default: startX = _random.NextDouble() * (width - 100); startY = height - 70; break;
        }

        // Calculate steal amount (15-25% of current evidence)
        double stealPercent = 0.15 + _random.NextDouble() * 0.10;
        double stealAmount = _engine.State.Evidence * stealPercent;

        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(230, 100, 50, 20)),
            BorderBrush = OrangeBrush,
            BorderThickness = new Thickness(3),
            CornerRadius = new CornerRadius(25),
            Padding = new Thickness(10),
            Cursor = Cursors.Hand,
            Width = 80,
            Height = 60
        };

        var stack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        stack.Children.Add(new TextBlock
        {
            Text = "🦹",
            FontSize = 24,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontFamily = EmojiFont
        });
        stack.Children.Add(new TextBlock
        {
            Text = $"⏱️ 6.0s - CATCH ME!",
            FontSize = 9,
            Foreground = LightBrush,
            HorizontalAlignment = HorizontalAlignment.Center
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
                int stolen = Math.Min(_engine.State.Tinfoil, _activeTinfoilThief.StealAmount);
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
            // Maybe spawn a new tinfoil thief (rarer than evidence thief)
            if (!_minigameActive && MinigameOverlay.Visibility != Visibility.Visible &&
                _activeDebunker == null && _activeEvidenceThief == null)
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
        int tinfoil = _engine.State.Tinfoil;

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

        // Spawn new events randomly (only one at a time)
        if (_escapedDocument == null && _evidenceTrail.Count == 0 && _connectionPins.Count == 0)
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

        // Create 3 pins at random positions
        for (int i = 1; i <= 3; i++)
        {
            double angle = _random.NextDouble() * Math.PI * 2;
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
        string symbol = AmbientSymbols[_random.Next(AmbientSymbols.Length)];
        var rotateTransform = new RotateTransform();

        var element = new TextBlock
        {
            Text = symbol,
            FontSize = 14 + _random.Next(8),
            Foreground = AmbientBrush,
            FontFamily = EmojiFont,
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


    private void InitializeGeneratorButtons()
    {
        var buttonStyle = (Style)FindResource("GeneratorButton");

        foreach (var gen in GeneratorData.AllGenerators)
        {
            var button = CreateGeneratorButton(gen, buttonStyle);
            GeneratorPanel.Children.Add(button);
            _generatorButtons[gen.Id] = button;
        }
    }

    private Button CreateGeneratorButton(Generator gen, Style buttonStyle)
    {
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
        var iconElement = IconHelper.CreateIconWithFallback(gen.Id, fallbackText, 28, GreenBrush);
        iconElement.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
        iconElement.SetValue(MarginProperty, new Thickness(0, 0, 10, 0));

        var leftStack = new StackPanel();
        leftStack.Children.Add(new TextBlock { Text = gen.Name, FontWeight = FontWeights.Bold, Foreground = GreenBrush });
        leftStack.Children.Add(new TextBlock { Text = gen.FlavorText, FontSize = 10, Foreground = DimBrush, TextWrapping = TextWrapping.Wrap, MaxWidth = 250 });
        leftStack.Children.Add(new TextBlock { Tag = "prod", FontSize = 11, Foreground = LightBrush, Margin = new Thickness(0, 3, 0, 0) });
        if (gen.BelieverBonus > 0)
            leftStack.Children.Add(new TextBlock { Text = $"+{gen.BelieverBonus} believers each", FontSize = 10, Foreground = GoldBrush });

        var rightStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(10, 0, 0, 0) };
        rightStack.Children.Add(new TextBlock { Tag = "cost", FontWeight = FontWeights.Bold, Foreground = GoldBrush, HorizontalAlignment = HorizontalAlignment.Right });
        rightStack.Children.Add(new TextBlock { Tag = "owned", FontSize = 11, Foreground = DimBrush, HorizontalAlignment = HorizontalAlignment.Right });

        Grid.SetColumn(iconElement, 0);
        Grid.SetColumn(leftStack, 1);
        Grid.SetColumn(rightStack, 2);
        grid.Children.Add(iconElement);
        grid.Children.Add(leftStack);
        grid.Children.Add(rightStack);

        button.Content = grid;
        return button;
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
        var scaleAnim = new DoubleAnimation { From = 1.0, To = 1.3, Duration = TimeSpan.FromMilliseconds(100), AutoReverse = true };
        var scaleTransform = new ScaleTransform(1, 1, EyePupil.Width / 2, EyePupil.Height / 2);
        EyePupil.RenderTransform = scaleTransform;
        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);

        var glowColor = _engine.State.GoldenEyeActive ? Colors.Gold : Color.FromRgb(0, 255, 65);
        EyeGlow.Fill = new RadialGradientBrush(glowColor, Colors.Transparent);
        var glowAnim = new DoubleAnimation { From = 0.5, To = 0, Duration = TimeSpan.FromMilliseconds(200) };
        EyeGlow.BeginAnimation(OpacityProperty, glowAnim);
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

        // Flash the eye bright white/gold
        var flashAnim = new DoubleAnimation { From = 1.0, To = 0, Duration = TimeSpan.FromMilliseconds(400) };
        EyeGlow.Fill = new RadialGradientBrush(Colors.White, Colors.Gold);
        EyeGlow.BeginAnimation(OpacityProperty, flashAnim);

        // Reset pupil to default size immediately (no animation - let UpdateUI handle sizing)
        EyePupil.BeginAnimation(WidthProperty, null);
        EyePupil.BeginAnimation(HeightProperty, null);
        EyePupil.Width = PUPIL_MIN_SIZE;
        EyePupil.Height = PUPIL_MIN_SIZE;

        // Combo text
        var text = new TextBlock { Text = $"COMBO! +{NumberFormatter.Format(amount)}", FontSize = 24, FontWeight = FontWeights.Bold, Foreground = GoldBrush };
        double centerX = ClickCanvas.ActualWidth / 2;
        double centerY = ClickCanvas.ActualHeight / 2;

        Canvas.SetLeft(text, centerX - 80);
        Canvas.SetTop(text, centerY - 60);
        ClickCanvas.Children.Add(text);

        var scaleTransform = new ScaleTransform(0.5, 0.5, 80, 12);
        text.RenderTransform = scaleTransform;

        var scaleAnim = new DoubleAnimation { From = 0.5, To = 1.5, Duration = TimeSpan.FromMilliseconds(600), EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
        var fadeAnim = new DoubleAnimation { From = 1, To = 0, BeginTime = TimeSpan.FromMilliseconds(400), Duration = TimeSpan.FromMilliseconds(400) };

        fadeAnim.Completed += (s, e) => ClickCanvas.Children.Remove(text);

        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
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
        EyeOuter.Stroke = GoldBrush;
        EyePupil.Fill = GoldBrush;
        FlavorTextDisplay.Text = "GOLDEN EYE! 10x clicks for 10 seconds!";
        ShowToast("GOLDEN EYE!", "10x click power for 10 seconds!");

        EyeGlow.Fill = new RadialGradientBrush(Colors.Gold, Colors.Transparent);
        var glowAnim = new DoubleAnimation { From = 0.3, To = 0.7, Duration = TimeSpan.FromMilliseconds(500), AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever };
        EyeGlow.BeginAnimation(OpacityProperty, glowAnim);
    }

    private void OnGoldenEyeEnd()
    {
        EyeOuter.Stroke = GreenBrush;
        EyePupil.Fill = GreenBrush;
        EyePupil.Opacity = 0.3;
        EyeGlow.BeginAnimation(OpacityProperty, null);
        EyeGlow.Opacity = 0;
        FlavorTextDisplay.Text = FlavorText.GetRandomClickMessage();
    }

    private void OnQuestComplete(string questId, bool success, double evidence, int tinfoil)
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

    private void ShowToast(string icon, string message, double durationSeconds = 5)
    {
        ToastIcon.Text = icon;
        ToastMessage.Text = message;
        ToastNotification.Visibility = Visibility.Visible;

        var fadeIn = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(200) };
        var fadeOut = new DoubleAnimation { From = 1, To = 0, BeginTime = TimeSpan.FromSeconds(durationSeconds), Duration = TimeSpan.FromMilliseconds(500) };
        fadeOut.Completed += (s, e) => ToastNotification.Visibility = Visibility.Collapsed;

        ToastNotification.BeginAnimation(OpacityProperty, fadeIn);
        ToastNotification.BeginAnimation(OpacityProperty, fadeOut);
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

        // Green pulse on eye
        var pulseAnim = new DoubleAnimation { From = 1.2, To = 1.0, Duration = TimeSpan.FromMilliseconds(200) };
        var scaleTransform = new ScaleTransform(1, 1, EyeOuter.Width / 2, EyeOuter.Height / 2);
        EyeOuter.RenderTransform = scaleTransform;
        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, pulseAnim);
        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, pulseAnim);

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
                SoundManager.Play("upgrade");
        }
    }

    private void UpdateUI()
    {
        var state = _engine.State;

        EvidenceDisplay.Text = $"{NumberFormatter.Format(state.Evidence)} Evidence";
        EpsDisplay.Text = NumberFormatter.FormatPerSecond(_engine.CalculateEvidencePerSecond());
        TinfoilDisplay.Text = state.Tinfoil.ToString();
        IlluminatiTokenDisplay.Text = state.IlluminatiTokens.ToString();
        TokensPanel.Visibility = state.IlluminatiTokens > 0 ? Visibility.Visible : Visibility.Collapsed;
        BelieversDisplay.Text = NumberFormatter.Format(state.Believers);
        AvailableBelieversDisplay.Text = NumberFormatter.Format(state.AvailableBelievers);

        // Click power label
        double clickPower = _engine.CalculateClickPower();
        double critChance = _engine.GetCriticalChance();
        string clickText = $"+{NumberFormatter.Format(clickPower)}/click";
        if (critChance > 0) clickText += $" ({critChance:P0} crit)";
        ClickPowerLabel.Text = clickText;

        double autoRate = _engine.GetAutoClickRate();
        if (autoRate > 0)
            EpsDisplay.Text += $" (+{autoRate}/s auto)";

        // Combo visual - pupil grows and changes color
        // Clear any running animations so we can set size directly
        EyePupil.BeginAnimation(WidthProperty, null);
        EyePupil.BeginAnimation(HeightProperty, null);

        double comboProgress = Math.Min(state.ComboMeter, 1.0);
        double pupilSize = PUPIL_MIN_SIZE + (PUPIL_MAX_SIZE - PUPIL_MIN_SIZE) * comboProgress;
        EyePupil.Width = pupilSize;
        EyePupil.Height = pupilSize;

        // Color gradient: green (0%) -> yellow (50%) -> gold (100%)
        Color comboColor;
        if (comboProgress < 0.5)
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
        EyePupil.Fill = new SolidColorBrush(comboColor);
        EyePupil.Opacity = 0.3 + 0.5 * comboProgress; // Gets brighter as combo builds

        // Combo label
        ComboLabel.Text = state.ComboClicks > 0 ? $"x{state.ComboClicks} COMBO" : "";

        TotalEvidenceDisplay.Text = NumberFormatter.Format(state.TotalEvidenceEarned);
        TotalClicksDisplay.Text = state.TotalClicks.ToString("N0");

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
        UpdateTabVisibility();
        UpdateTabHighlights();
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

        // Tinfoil Shop: show when user has earned any tinfoil or has 10+ tinfoil shop purchases available
        // ~50% of usefulness = having some tinfoil to spend
        TinfoilShopTab.Visibility = (state.Tinfoil > 0 || state.TinfoilShopPurchases.Count > 0)
            ? Visibility.Visible : Visibility.Collapsed;

        // Quests: show when user has ~10 believers (first quest needs 20)
        QuestsTab.Visibility = state.Believers >= 10
            ? Visibility.Visible : Visibility.Collapsed;

        // Skills: show when user has 5+ achievements (halfway to first skill point at 10)
        // OR has any skill points, OR has unlocked skills
        SkillsTab.Visibility = (state.UnlockedAchievements.Count >= 5 || _engine.GetTotalSkillPoints() > 0 || state.UnlockedSkills.Count > 0)
            ? Visibility.Visible : Visibility.Collapsed;

        // Illuminati: show when user has 500B+ total evidence (prestige useful at 1T)
        IlluminatiTab.Visibility = state.TotalEvidenceEarned >= 500_000_000_000 || state.IlluminatiTokens > 0 || state.TimesAscended > 0
            ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateGeneratorButtons()
    {
        var state = _engine.State;
        var (_, epsMult) = _engine.GetEpsBreakdown();

        foreach (var gen in GeneratorData.AllGenerators)
        {
            if (!_generatorButtons.TryGetValue(gen.Id, out var button)) continue;

            double cost = _engine.GetGeneratorCost(gen.Id);
            int owned = state.GetGeneratorCount(gen.Id);
            double baseProduction = gen.GetProduction(owned);
            double multipliedProduction = baseProduction * epsMult;

            // Always update enabled state (depends on evidence which changes constantly)
            button.IsEnabled = state.Evidence >= cost;

            // Dirty-check: skip text updates if nothing changed
            var newState = (cost, owned, multipliedProduction);
            if (_lastGenState.TryGetValue(gen.Id, out var lastState) && lastState == newState)
                continue;
            _lastGenState[gen.Id] = newState;

            // Per-generator production (with multipliers)
            double perGenBase = gen.BaseProduction;
            double perGenMultiplied = perGenBase * epsMult;

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

                var iconText = IconData.UpgradeIcons.TryGetValue(upgrade.Id, out var icon) ? icon : "?";
                var iconBlock = new TextBlock { Text = iconText, FontSize = 16, Foreground = GoldBrush, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };
                var stack = new StackPanel();
                stack.Children.Add(new TextBlock { Text = upgrade.Name, FontWeight = FontWeights.Bold, Foreground = GoldBrush });
                stack.Children.Add(new TextBlock { Text = upgrade.Description, FontSize = 10, Foreground = DimBrush });

                Grid.SetColumn(iconBlock, 0);
                Grid.SetColumn(stack, 1);
                grid.Children.Add(iconBlock);
                grid.Children.Add(stack);
                border.Child = grid;
                PurchasedUpgradesPanel.Children.Add(border);
            }

            if (PurchasedUpgradesPanel.Children.Count == 0)
                PurchasedUpgradesPanel.Children.Add(new TextBlock { Text = "No upgrades purchased yet", Foreground = DimBrush, FontStyle = FontStyles.Italic, Margin = new Thickness(5) });

            // Update available upgrades panel
            UpgradePanel.Children.Clear();
            _upgradeButtons.Clear();

            foreach (var upgrade in available)
            {
                var button = new Button { Style = buttonStyle, Tag = upgrade.Id, HorizontalContentAlignment = HorizontalAlignment.Stretch };
                button.Click += UpgradeButton_Click;

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var iconText = IconData.UpgradeIcons.TryGetValue(upgrade.Id, out var icon) ? icon : "?";
                var iconBlock = new TextBlock { Text = iconText, FontSize = 18, Foreground = GreenBrush, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };

                var leftStack = new StackPanel();
                leftStack.Children.Add(new TextBlock { Text = upgrade.Name, FontWeight = FontWeights.Bold, Foreground = GreenBrush });
                leftStack.Children.Add(new TextBlock { Text = upgrade.Description, FontSize = 12, Foreground = LightBrush });
                leftStack.Children.Add(new TextBlock { Text = upgrade.FlavorText, FontSize = 10, Foreground = DimBrush, FontStyle = FontStyles.Italic, TextWrapping = TextWrapping.Wrap, MaxWidth = 280 });

                var costText = upgrade.TinfoilCost > 0 ? $"{upgrade.TinfoilCost} Tinfoil" : NumberFormatter.Format(upgrade.EvidenceCost);
                var costBrush = upgrade.TinfoilCost > 0 ? SilverBrush : GoldBrush;
                var rightStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0) };
                rightStack.Children.Add(new TextBlock { Text = costText, FontWeight = FontWeights.Bold, Foreground = costBrush, HorizontalAlignment = HorizontalAlignment.Right });

                Grid.SetColumn(iconBlock, 0);
                Grid.SetColumn(leftStack, 1);
                Grid.SetColumn(rightStack, 2);
                grid.Children.Add(iconBlock);
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

                var iconBlock = new TextBlock { Text = upgrade.Icon, FontSize = 16, Foreground = SilverBrush, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };
                var stack = new StackPanel();
                stack.Children.Add(new TextBlock { Text = upgrade.Name, FontWeight = FontWeights.Bold, Foreground = SilverBrush });
                stack.Children.Add(new TextBlock { Text = upgrade.Description, FontSize = 10, Foreground = DimBrush });

                Grid.SetColumn(iconBlock, 0);
                Grid.SetColumn(stack, 1);
                grid.Children.Add(iconBlock);
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

                var iconBlock = new TextBlock { Text = upgrade.Icon, FontSize = 18, Foreground = SilverBrush, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0), Tag = "icon" };

                var leftStack = new StackPanel();
                leftStack.Children.Add(new TextBlock { Text = upgrade.Name, FontWeight = FontWeights.Bold, Foreground = SilverBrush, Tag = "name" });
                leftStack.Children.Add(new TextBlock { Text = upgrade.Description, FontSize = 12, Foreground = LightBrush, Tag = "desc" });
                leftStack.Children.Add(new TextBlock { Text = upgrade.FlavorText, FontSize = 10, Foreground = DimBrush, FontStyle = FontStyles.Italic, TextWrapping = TextWrapping.Wrap, MaxWidth = 280 });

                var rightStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0) };
                rightStack.Children.Add(new TextBlock { Text = $"{upgrade.TinfoilCost} Tinfoil", FontWeight = FontWeights.Bold, Foreground = SilverBrush, HorizontalAlignment = HorizontalAlignment.Right, Tag = "cost" });

                Grid.SetColumn(iconBlock, 0);
                Grid.SetColumn(leftStack, 1);
                Grid.SetColumn(rightStack, 2);
                grid.Children.Add(iconBlock);
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

            foreach (var conspiracy in ConspiracyData.AllConspiracies.Where(c => state.ProvenConspiracies.Contains(c.Id)))
            {
                var border = new Border { Background = new SolidColorBrush(Color.FromRgb(15, 52, 96)), BorderBrush = GreenBrush, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(4), Padding = new Thickness(10, 8, 10, 8), Margin = new Thickness(5) };
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var fallbackText = IconData.ConspiracyIcons.TryGetValue(conspiracy.Id, out var icon) ? icon : "?";
                var iconElement = IconHelper.CreateIconWithFallback(conspiracy.Id, fallbackText, 32, GreenBrush);
                iconElement.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
                iconElement.SetValue(MarginProperty, new Thickness(0, 0, 12, 0));

                var stack = new StackPanel();
                stack.Children.Add(new TextBlock { Text = $"[PROVEN] {conspiracy.Name}", FontWeight = FontWeights.Bold, Foreground = GreenBrush });
                stack.Children.Add(new TextBlock { Text = conspiracy.FlavorText, FontSize = 10, Foreground = DimBrush, FontStyle = FontStyles.Italic });
                var bonusText = conspiracy.MultiplierBonus > 1.0 ? $"x{conspiracy.MultiplierBonus} all production" : $"+{conspiracy.ClickBonus} click power";
                stack.Children.Add(new TextBlock { Text = bonusText, FontSize = 11, Foreground = GoldBrush, Margin = new Thickness(0, 3, 0, 0) });

                Grid.SetColumn(iconElement, 0);
                Grid.SetColumn(stack, 1);
                grid.Children.Add(iconElement);
                grid.Children.Add(stack);
                border.Child = grid;
                ConspiracyPanel.Children.Add(border);
            }

            foreach (var conspiracy in available)
            {
                var button = new Button { Style = buttonStyle, Tag = conspiracy.Id, HorizontalContentAlignment = HorizontalAlignment.Stretch };
                button.Click += ConspiracyButton_Click;

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var fallbackText2 = IconData.ConspiracyIcons.TryGetValue(conspiracy.Id, out var icon2) ? icon2 : "?";
                var iconElement2 = IconHelper.CreateIconWithFallback(conspiracy.Id, fallbackText2, 28, DimBrush);
                iconElement2.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
                iconElement2.SetValue(MarginProperty, new Thickness(0, 0, 10, 0));

                var leftStack = new StackPanel();
                leftStack.Children.Add(new TextBlock { Text = conspiracy.Name, FontWeight = FontWeights.Bold, Foreground = GreenBrush });
                leftStack.Children.Add(new TextBlock { Text = conspiracy.Description, FontSize = 11, Foreground = LightBrush });
                leftStack.Children.Add(new TextBlock { Text = conspiracy.FlavorText, FontSize = 10, Foreground = DimBrush, FontStyle = FontStyles.Italic, TextWrapping = TextWrapping.Wrap, MaxWidth = 280 });
                var bonusText2 = conspiracy.MultiplierBonus > 1.0 ? $"Reward: x{conspiracy.MultiplierBonus} all + {conspiracy.TinfoilReward} Tinfoil" : $"Reward: +{conspiracy.ClickBonus} click + {conspiracy.TinfoilReward} Tinfoil";
                leftStack.Children.Add(new TextBlock { Text = bonusText2, FontSize = 10, Foreground = GoldBrush, Margin = new Thickness(0, 3, 0, 0) });

                var rightStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0) };
                bool canClaim = state.TotalEvidenceEarned >= conspiracy.EvidenceCost;
                rightStack.Children.Add(new TextBlock { Text = canClaim ? "✓ CLAIM" : $"Need {NumberFormatter.Format(conspiracy.EvidenceCost)}", FontWeight = FontWeights.Bold, Foreground = canClaim ? GreenBrush : DimBrush });
                rightStack.Children.Add(new TextBlock { Text = "total evidence", FontSize = 9, Foreground = DimBrush, HorizontalAlignment = HorizontalAlignment.Center });

                Grid.SetColumn(iconElement2, 0);
                Grid.SetColumn(leftStack, 1);
                Grid.SetColumn(rightStack, 2);
                grid.Children.Add(iconElement2);
                grid.Children.Add(leftStack);
                grid.Children.Add(rightStack);

                button.Content = grid;
                ConspiracyPanel.Children.Add(button);
                _conspiracyButtons[conspiracy.Id] = button;
            }

            if (ConspiracyPanel.Children.Count == 0)
                ConspiracyPanel.Children.Add(new TextBlock { Text = "Keep gathering evidence to uncover conspiracies...", Foreground = DimBrush, FontStyle = FontStyles.Italic, Margin = new Thickness(10) });
        }

        // Update enabled state for existing buttons
        foreach (var (id, button) in _conspiracyButtons)
            button.IsEnabled = _engine.CanAffordConspiracy(id);
    }

    private void UpdateQuestPanel()
    {
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

            var iconBlock = new TextBlock { Text = quest.Icon, FontSize = 20, Foreground = GoldBrush, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock { Text = quest.Name, FontWeight = FontWeights.Bold, Foreground = GoldBrush });
            stack.Children.Add(new TextBlock { Text = $"{activeQuest.BelieversSent} believers on mission", FontSize = 11, Foreground = DimBrush });

            var remaining = activeQuest.EndTime - DateTime.Now;
            var totalDuration = (activeQuest.EndTime - activeQuest.StartTime).TotalSeconds;
            var elapsed = totalDuration - remaining.TotalSeconds;
            var progress = Math.Max(0, Math.Min(1, elapsed / totalDuration));
            var timeText = remaining.TotalSeconds > 0 ? $"{(int)remaining.TotalMinutes}:{remaining.Seconds:D2} remaining" : "Completing...";

            // Fixed width progress bar
            var progressBar = new Border { Background = DarkBrush, CornerRadius = new CornerRadius(3), Height = 8, Margin = new Thickness(0, 4, 0, 0), Width = 200 };
            var progressFill = new Border { Background = GoldBrush, CornerRadius = new CornerRadius(3), HorizontalAlignment = HorizontalAlignment.Left, Width = progress * 200 };
            progressBar.Child = progressFill;
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

            foreach (var quest in QuestData.AllQuests)
            {
                if (state.ActiveQuests.Any(q => q.QuestId == quest.Id)) continue;

                var riskColor = quest.Risk switch { QuestRisk.Low => GreenBrush, QuestRisk.Medium => OrangeBrush, QuestRisk.High => RedBrush, _ => DimBrush };

                var button = new Button { Style = buttonStyle, Tag = quest.Id, HorizontalContentAlignment = HorizontalAlignment.Stretch };
                button.Click += QuestButton_Click;

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var qIconBlock = new TextBlock { Text = quest.Icon, FontSize = 18, Foreground = riskColor, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };

                var leftStack = new StackPanel();
                leftStack.Children.Add(new TextBlock { Text = quest.Name, FontWeight = FontWeights.Bold, Foreground = GreenBrush });
                leftStack.Children.Add(new TextBlock { Text = quest.Description, FontSize = 11, Foreground = LightBrush });
                leftStack.Children.Add(new TextBlock { Text = quest.FlavorText, FontSize = 10, Foreground = DimBrush, FontStyle = FontStyles.Italic });

                var riskText = quest.Risk switch { QuestRisk.Low => "LOW RISK", QuestRisk.Medium => "MEDIUM RISK", QuestRisk.High => "HIGH RISK", _ => "" };
                double adjustedChance = Math.Min(quest.SuccessChance + _engine.GetTinfoilQuestSuccessBonus(), 0.95);
                leftStack.Children.Add(new TextBlock { Text = $"{riskText} ({adjustedChance:P0})", FontSize = 10, Foreground = riskColor, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 3, 0, 0) });

                var rightStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0) };
                rightStack.Children.Add(new TextBlock { Text = $"{quest.BelieversRequired} believers", FontWeight = FontWeights.Bold, Foreground = GoldBrush, HorizontalAlignment = HorizontalAlignment.Right });
                var duration = TimeSpan.FromSeconds(quest.DurationSeconds);
                rightStack.Children.Add(new TextBlock { Text = duration.TotalMinutes >= 1 ? $"{(int)duration.TotalMinutes}m" : $"{duration.Seconds}s", FontSize = 11, Foreground = DimBrush, HorizontalAlignment = HorizontalAlignment.Right });
                var rewardText = quest.TinfoilReward > 0 ? $"+{quest.TinfoilReward} tinfoil" : $"~{quest.EvidenceMultiplier}s EPS";
                rightStack.Children.Add(new TextBlock { Text = rewardText, FontSize = 10, Foreground = GoldBrush, HorizontalAlignment = HorizontalAlignment.Right });

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

    private void UpdateAchievementPanel()
    {
        var state = _engine.State;
        AchievementPanel.Children.Clear();

        foreach (var achievement in _engine.GetUnlockedAchievements())
        {
            var border = new Border { Background = new SolidColorBrush(Color.FromRgb(15, 52, 96)), BorderBrush = GoldBrush, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(4), Padding = new Thickness(10, 6, 10, 6), Margin = new Thickness(5) };
            var stack = new StackPanel();
            stack.Children.Add(new TextBlock { Text = achievement.Name, FontWeight = FontWeights.Bold, Foreground = GoldBrush });
            stack.Children.Add(new TextBlock { Text = achievement.Description, FontSize = 10, Foreground = LightBrush });
            border.Child = stack;
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
                _ => 0
            };
            double progress = Math.Min(current / achievement.Threshold, 1.0);

            var border = new Border { Background = DarkBrush, BorderBrush = DimBrush, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(4), Padding = new Thickness(10, 6, 10, 6), Margin = new Thickness(5) };
            var stack = new StackPanel();
            stack.Children.Add(new TextBlock { Text = achievement.Name, FontWeight = FontWeights.Bold, Foreground = DimBrush });
            stack.Children.Add(new TextBlock { Text = achievement.Description, FontSize = 10, Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)) });
            stack.Children.Add(new TextBlock { Text = $"Progress: {progress:P0}", FontSize = 9, Foreground = DimBrush, Margin = new Thickness(0, 2, 0, 0) });
            border.Child = stack;
            AchievementPanel.Children.Add(border);
        }
    }

    private void UpdateOwnedGeneratorsPanel()
    {
        OwnedGeneratorsPanel.Children.Clear();
        var state = _engine.State;

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
                    OwnedGeneratorsPanel.Children.Add(new TextBlock { Text = $"{gen.Name}: +{NumberFormatter.Format(believers)}", Foreground = DimBrush, FontSize = 11, Margin = new Thickness(0, 1, 0, 1) });
            }

            double tinfoilMultiplier = _engine.GetTinfoilBelieverMultiplier();
            if (tinfoilMultiplier > 1.0)
                OwnedGeneratorsPanel.Children.Add(new TextBlock { Text = $"Tinfoil Shop: ×{tinfoilMultiplier:F2}", Foreground = SilverBrush, FontSize = 11, Margin = new Thickness(0, 1, 0, 1) });
        }
    }

    private void ShowFlavorMessage(string message) => FlavorTextDisplay.Text = message;

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
    }

    private void UpdateSlotDisplay(int slot, SaveSlotInfo info, TextBlock infoText, TextBlock detailsText,
                                    Button deleteBtn, Border border)
    {
        if (info.Exists)
        {
            infoText.Text = $"Evidence: {NumberFormatter.Format(info.TotalEvidence)} | Ascensions: {info.AscensionCount}";
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
        }
        else
        {
            _engine.LoadSlot(slot);
        }

        MainMenuOverlay.Visibility = Visibility.Collapsed;
        _gameStarted = true;
        UpdateUI();
        _engine.Start();
        SoundManager.Play("achievement");
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
        var buttonStyle = (Style)FindResource("GeneratorButton");

        SkillPointsDisplay.Text = $"{_engine.GetAvailableSkillPoints()} / {_engine.GetTotalSkillPoints()}";

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
                    Padding = new Thickness(10, 6, 10, 6),
                    Margin = new Thickness(5)
                };
                var stack = new StackPanel();
                stack.Children.Add(new TextBlock { Text = $"[CLAIMED] {challenge.Name}", FontWeight = FontWeights.Bold, Foreground = GreenBrush });
                stack.Children.Add(new TextBlock { Text = challenge.Description, FontSize = 10, Foreground = DimBrush });
                border.Child = stack;
                DailyChallengesPanel.Children.Add(border);
            }
            else if (challenge.Completed)
            {
                var button = new Button { Style = buttonStyle, Tag = challenge.Id, HorizontalContentAlignment = HorizontalAlignment.Stretch };
                button.Click += DailyChallengeButton_Click;

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var stack = new StackPanel();
                stack.Children.Add(new TextBlock { Text = $"[COMPLETE!] {challenge.Name}", FontWeight = FontWeights.Bold, Foreground = GoldBrush });
                stack.Children.Add(new TextBlock { Text = challenge.Description, FontSize = 10, Foreground = LightBrush });
                stack.Children.Add(new TextBlock { Text = "Click to claim reward!", FontSize = 10, Foreground = GreenBrush, FontWeight = FontWeights.Bold });

                var rewardStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                rewardStack.Children.Add(new TextBlock { Text = $"+{challenge.TinfoilReward} Tinfoil", FontWeight = FontWeights.Bold, Foreground = SilverBrush });

                Grid.SetColumn(stack, 0);
                Grid.SetColumn(rewardStack, 1);
                grid.Children.Add(stack);
                grid.Children.Add(rewardStack);

                button.Content = grid;
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
                    Padding = new Thickness(10, 6, 10, 6),
                    Margin = new Thickness(5)
                };

                var stack = new StackPanel();
                stack.Children.Add(new TextBlock { Text = challenge.Name, FontWeight = FontWeights.Bold, Foreground = LightBrush });
                stack.Children.Add(new TextBlock { Text = challenge.Description, FontSize = 10, Foreground = DimBrush });

                var progressBar = new Border { Background = new SolidColorBrush(Color.FromRgb(30, 30, 40)), CornerRadius = new CornerRadius(3), Height = 8, Width = 200, Margin = new Thickness(0, 4, 0, 0) };
                var progressFill = new Border { Background = GreenBrush, CornerRadius = new CornerRadius(3), HorizontalAlignment = HorizontalAlignment.Left, Width = progressPercent * 200 };
                progressBar.Child = progressFill;
                stack.Children.Add(progressBar);

                stack.Children.Add(new TextBlock { Text = $"{NumberFormatter.Format(challenge.Progress)} / {NumberFormatter.Format(challenge.Target)} ({progressPercent:P0})", FontSize = 10, Foreground = DimBrush, Margin = new Thickness(0, 2, 0, 0) });
                stack.Children.Add(new TextBlock { Text = $"Reward: +{challenge.TinfoilReward} Tinfoil", FontSize = 10, Foreground = SilverBrush });

                border.Child = stack;
                DailyChallengesPanel.Children.Add(border);
            }
        }
    }

    private void UpdatePrestigePanel()
    {
        var state = _engine.State;
        var buttonStyle = (Style)FindResource("GeneratorButton");

        // Prestige info
        bool canPrestige = _engine.CanPrestige();
        int tokensFromPrestige = _engine.GetTokensFromPrestige();

        if (canPrestige)
        {
            PrestigeInfoText.Text = $"You have earned {NumberFormatter.Format(state.TotalEvidenceEarned)} total evidence.\n" +
                                    $"Ascending will grant you {tokensFromPrestige} Illuminati Token(s).";
            PrestigeButton.IsEnabled = true;
        }
        else
        {
            double threshold = PrestigeData.PRESTIGE_THRESHOLD;
            double progress = state.TotalEvidenceEarned / threshold;
            PrestigeInfoText.Text = $"Progress: {NumberFormatter.Format(state.TotalEvidenceEarned)} / {NumberFormatter.Format(threshold)} ({progress:P1})\n" +
                                    $"Reach 1 trillion total evidence to unlock prestige.";
            PrestigeButton.IsEnabled = false;
        }

        IlluminatiTokensText.Text = $"You have {state.IlluminatiTokens} Illuminati Token(s)";

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

            var iconBlock = new TextBlock { Text = upgrade.Icon, FontSize = 20, Foreground = PurpleBrush, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };

            var stack = new StackPanel();
            stack.Children.Add(new TextBlock { Text = upgrade.Name, FontWeight = FontWeights.Bold, Foreground = PurpleBrush });
            stack.Children.Add(new TextBlock { Text = upgrade.Description, FontSize = 11, Foreground = LightBrush });
            stack.Children.Add(new TextBlock { Text = upgrade.FlavorText, FontSize = 10, Foreground = DimBrush, FontStyle = FontStyles.Italic });

            var costStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            costStack.Children.Add(new TextBlock { Text = $"{upgrade.TokenCost} Token(s)", FontWeight = FontWeights.Bold, Foreground = PurpleBrush, HorizontalAlignment = HorizontalAlignment.Right });

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

            var iconBlock = new TextBlock { Text = upgrade.Icon, FontSize = 16, Foreground = PurpleBrush, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };
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
        SoundToggleIcon.Text = SoundManager.Enabled ? "🔊" : "🔇";
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
            MinigameType.ClickFrenzy => "Click Frenzy",
            MinigameType.DocumentCatch => "Document Catch",
            MinigameType.MemoryMatrix => "Memory Matrix",
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
        // Big click target in center
        var target = new Button
        {
            Content = "👁️",
            FontSize = 60,
            Width = 150,
            Height = 150,
            Background = new SolidColorBrush(Color.FromRgb(15, 52, 96)),
            Foreground = GreenBrush,
            BorderBrush = GreenBrush,
            BorderThickness = new Thickness(3),
            Cursor = Cursors.Hand
        };
        target.Click += ClickFrenzyTarget_Click;
        Canvas.SetLeft(target, (MinigameCanvas.Width - 150) / 2);
        Canvas.SetTop(target, (MinigameCanvas.Height - 150) / 2);
        MinigameCanvas.Children.Add(target);
    }

    private void ClickFrenzyTarget_Click(object sender, RoutedEventArgs e)
    {
        if (!_minigameActive) return;
        _minigameScore++;
        MinigameStatus.Text = $"Clicks: {_minigameScore}";
        SoundManager.Play("click");
    }

    private void SetupMemoryMatrix()
    {
        MinigameCanvas.Children.Clear();
        _memoryPattern = new int[_memoryDifficulty]; // Use current difficulty
        _memoryIndex = 0;

        // Create 3x3 grid
        for (int i = 0; i < 9; i++)
        {
            int row = i / 3;
            int col = i % 3;

            var cell = new Border
            {
                Width = 80,
                Height = 50,
                Background = new SolidColorBrush(Color.FromRgb(42, 42, 62)),
                BorderBrush = DimBrush,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(4),
                Tag = i,
                Cursor = Cursors.Hand
            };
            cell.MouseDown += MemoryCell_Click;

            Canvas.SetLeft(cell, 50 + col * 90);
            Canvas.SetTop(cell, 20 + row * 60);
            MinigameCanvas.Children.Add(cell);
            _minigameElements.Add(cell);
        }

        // Generate random pattern
        for (int i = 0; i < _memoryPattern.Length; i++)
            _memoryPattern[i] = _random.Next(9);

        // Show pattern with delay
        MinigameStatus.Text = "Watch the pattern...";
        ShowMemoryPattern(0);
    }

    private async void ShowMemoryPattern(int index)
    {
        if (index >= _memoryPattern!.Length)
        {
            MinigameStatus.Text = "Your turn! Repeat the pattern.";
            _minigameTimer = 10.0;
            return;
        }

        int cellIndex = _memoryPattern[index];
        if (cellIndex < _minigameElements.Count && _minigameElements[cellIndex] is Border cell)
        {
            cell.Background = GreenBrush;
            await Task.Delay(400);
            cell.Background = new SolidColorBrush(Color.FromRgb(42, 42, 62));
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
                    cell.Background = new SolidColorBrush(Color.FromRgb(42, 42, 62));
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

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => _engine.Stop();
}
