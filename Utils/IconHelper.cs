using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ConspiracyClicker.Utils;

/// <summary>
/// Helper class for accessing SVG-based icons as WPF elements.
/// Icons are defined in Resources/Icons.xaml as DrawingImage resources.
/// </summary>
public static class IconHelper
{
    private static ResourceDictionary? _iconResources;

    /// <summary>
    /// Ensures the icon resources are loaded.
    /// </summary>
    private static void EnsureResourcesLoaded()
    {
        if (_iconResources == null)
        {
            _iconResources = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/Resources/Icons.xaml", UriKind.Absolute)
            };
        }
    }

    /// <summary>
    /// Gets a DrawingImage icon by its key.
    /// </summary>
    /// <param name="iconKey">The icon key (e.g., "red_string", "flat_earth")</param>
    /// <returns>The DrawingImage, or null if not found.</returns>
    public static DrawingImage? GetIconImage(string iconKey)
    {
        EnsureResourcesLoaded();
        var key = $"Icon_{iconKey}";
        if (_iconResources!.Contains(key))
        {
            return _iconResources[key] as DrawingImage;
        }
        return null;
    }

    /// <summary>
    /// Creates an Image control with the specified icon.
    /// </summary>
    /// <param name="iconKey">The icon key (e.g., "red_string", "flat_earth")</param>
    /// <param name="size">The size of the icon (width and height)</param>
    /// <returns>An Image control, or null if icon not found.</returns>
    public static Image? CreateIcon(string iconKey, double size = 24)
    {
        var drawingImage = GetIconImage(iconKey);
        if (drawingImage == null) return null;

        return new Image
        {
            Source = drawingImage,
            Width = size,
            Height = size,
            Stretch = Stretch.Uniform
        };
    }

    /// <summary>
    /// Creates an Image control with the specified icon and custom dimensions.
    /// </summary>
    /// <param name="iconKey">The icon key</param>
    /// <param name="width">The width of the icon</param>
    /// <param name="height">The height of the icon</param>
    /// <returns>An Image control, or null if icon not found.</returns>
    public static Image? CreateIcon(string iconKey, double width, double height)
    {
        var drawingImage = GetIconImage(iconKey);
        if (drawingImage == null) return null;

        return new Image
        {
            Source = drawingImage,
            Width = width,
            Height = height,
            Stretch = Stretch.Uniform
        };
    }

    /// <summary>
    /// Creates an icon element - returns Image if SVG icon exists, otherwise falls back to TextBlock.
    /// </summary>
    /// <param name="iconKey">The icon key</param>
    /// <param name="fallbackText">Fallback text if icon not found</param>
    /// <param name="size">Icon size</param>
    /// <param name="foreground">Foreground brush for fallback text</param>
    /// <returns>UIElement containing the icon</returns>
    public static UIElement CreateIconWithFallback(string iconKey, string fallbackText, double size, Brush foreground)
    {
        var icon = CreateIcon(iconKey, size);
        if (icon != null) return icon;

        return new TextBlock
        {
            Text = fallbackText,
            FontSize = size * 0.7,
            Foreground = foreground,
            VerticalAlignment = VerticalAlignment.Center
        };
    }

    /// <summary>
    /// Gets a list of all available icon keys.
    /// </summary>
    public static IEnumerable<string> GetAvailableIconKeys()
    {
        EnsureResourcesLoaded();
        return _iconResources!.Keys
            .Cast<object>()
            .Where(k => k is string s && s.StartsWith("Icon_"))
            .Cast<string>()
            .Select(k => k.Substring(5)); // Remove "Icon_" prefix
    }

    /// <summary>
    /// Checks if an icon exists for the given key.
    /// </summary>
    public static bool IconExists(string iconKey)
    {
        EnsureResourcesLoaded();
        return _iconResources!.Contains($"Icon_{iconKey}");
    }

    // Pre-defined icon keys for generators
    public static class Generators
    {
        // Tier 1
        public const string RedString = "red_string";
        public const string SuspiciousNeighbor = "suspicious_neighbor";
        public const string BasementResearcher = "basement_researcher";
        public const string BlogspotBlog = "blogspot_blog";
        public const string YoutubeChannel = "youtube_channel";
        public const string DiscordServer = "discord_server";
        public const string AmRadio = "am_radio";
        public const string Podcast = "podcast";
        public const string TruthConference = "truth_conference";
        public const string NetflixDoc = "netflix_doc";
        public const string SpySatellite = "spy_satellite";
        public const string ShadowGovernment = "shadow_government";

        // Tier 2
        public const string MindControlTower = "mind_control_tower";
        public const string WeatherMachine = "weather_machine";
        public const string CloneFacility = "clone_facility";
        public const string TimeMachine = "time_machine";
        public const string HollowEarthBase = "hollow_earth_base";

        // Tier 3
        public const string MoonBase = "moon_base";
        public const string AlienAlliance = "alien_alliance";
        public const string DimensionPortal = "dimension_portal";
        public const string SimulationAdmin = "simulation_admin";
        public const string RealityEditor = "reality_editor";

        // Tier 4
        public const string MultiverseNetwork = "multiverse_network";
        public const string CosmicConsciousness = "cosmic_consciousness";
        public const string TruthSingularityGen = "truth_singularity_gen";
        public const string OmniscienceEngine = "omniscience_engine";
        public const string UniverseCreator = "universe_creator";
    }

    // Pre-defined icon keys for conspiracies
    public static class Conspiracies
    {
        public const string BirdsArentReal = "birds_arent_real";
        public const string FlatEarth = "flat_earth";
        public const string MoonLanding = "moon_landing";
        public const string LizardPeople = "lizard_people";
        public const string AustraliaFake = "australia_fake";
        public const string FinlandMyth = "finland_myth";
        public const string MattressLaundering = "mattress_laundering";
        public const string TimeInvention = "time_invention";
        public const string Simulation = "simulation";
        public const string YouAreConspiracy = "you_are_conspiracy";
    }

    // Pre-defined icon keys for UI elements
    public static class UI
    {
        public const string Tinfoil = "tinfoil";
        public const string Evidence = "evidence";
        public const string Believer = "believer";
        public const string Quest = "quest";
        public const string Upgrade = "upgrade";
        public const string Prestige = "prestige";
        public const string Achievement = "achievement";
    }

    // Pre-defined icon keys for upgrades
    public static class Upgrades
    {
        // Click Power
        public const string ReinforcedTinfoil = "reinforced_tinfoil";
        public const string MagnifyingGlass = "magnifying_glass";
        public const string RedMarker = "red_marker";
        public const string CorkBoard = "cork_board";
        public const string NightVision = "night_vision";
        public const string MechanicalKeyboard = "mechanical_keyboard";

        // Click Multipliers
        public const string ThirdEyeDrops = "third_eye_drops";
        public const string CaffeineIv = "caffeine_iv";
        public const string QuantumFingers = "quantum_fingers";

        // EPS to Click
        public const string MomentumTheory = "momentum_theory";
        public const string SynergyDoctrine = "synergy_doctrine";
        public const string UnifiedField = "unified_field";
        public const string InfiniteRecursion = "infinite_recursion";

        // Global Boosts
        public const string ViralMomentum = "viral_momentum";
        public const string MassAwakening = "mass_awakening";
        public const string TruthSingularity = "truth_singularity";
    }

    // Pre-defined icon keys for quests
    public static class Quests
    {
        public const string ReconMission = "recon_mission";
        public const string DocumentRecovery = "document_recovery";
        public const string SignalIntercept = "signal_intercept";
        public const string FacilityInfiltration = "facility_infiltration";
        public const string UndergroundBunker = "underground_bunker";
        public const string CorporateMole = "corporate_mole";
    }

    // Pre-defined icon keys for skill tree
    public static class Skills
    {
        public const string Researcher = "skill_researcher";
        public const string Influencer = "skill_influencer";
        public const string Infiltrator = "skill_infiltrator";
    }

    // Pre-defined icon keys for prestige/illuminati upgrades
    public static class Prestige
    {
        public const string PyramidScheme = "pyramid_scheme";
        public const string SecretHandshake = "secret_handshake";
        public const string NewWorldOrder = "new_world_order";
        public const string ReptilianDna = "reptilian_dna";
        public const string MoonBase = "moon_base";
        public const string TimeManipulation = "time_manipulation";
    }

    // Pre-defined icon keys for the All-Seeing Eye (main clicker)
    public static class Eye
    {
        public const string AllSeeingEye = "all_seeing_eye";
        public const string AllSeeingEyeSmall = "all_seeing_eye_small";
        public const string GoldenEye = "golden_eye";
    }

    // Pre-defined icon keys for orbit display (generator icons around the eye)
    public static class Orbit
    {
        public const string Pin = "orbit_pin";              // Red String
        public const string Eye = "orbit_eye";              // Suspicious Neighbor
        public const string Computer = "orbit_computer";    // Basement Researcher
        public const string Blog = "orbit_blog";            // Blogspot Blog
        public const string Play = "orbit_play";            // YouTube Channel
        public const string Chat = "orbit_chat";            // Discord Server
        public const string Radio = "orbit_radio";          // AM Radio
        public const string Microphone = "orbit_microphone"; // Podcast
        public const string Stage = "orbit_stage";          // Truth Conference
        public const string Film = "orbit_film";            // Netflix Doc
        public const string Satellite = "orbit_satellite";  // Spy Satellite
        public const string Capitol = "orbit_capitol";      // Shadow Government
    }

    // Mapping from generator IDs to orbit icon keys
    public static readonly Dictionary<string, string> GeneratorOrbitIcons = new()
    {
        ["red_string"] = Orbit.Pin,
        ["suspicious_neighbor"] = Orbit.Eye,
        ["basement_researcher"] = Orbit.Computer,
        ["blogspot_blog"] = Orbit.Blog,
        ["youtube_channel"] = Orbit.Play,
        ["discord_server"] = Orbit.Chat,
        ["am_radio"] = Orbit.Radio,
        ["podcast"] = Orbit.Microphone,
        ["truth_conference"] = Orbit.Stage,
        ["netflix_doc"] = Orbit.Film,
        ["spy_satellite"] = Orbit.Satellite,
        ["shadow_government"] = Orbit.Capitol
    };

    // Pre-defined icon keys for Tinfoil Shop items
    public static class TinfoilShop
    {
        // Tinfoil hats
        public const string HatBasic = "tinfoil_hat_basic";
        public const string HatReinforced = "tinfoil_hat_reinforced";
        public const string Crown = "tinfoil_crown";
        public const string Bodysuit = "tinfoil_bodysuit";
        public const string Bunker = "tinfoil_bunker";

        // Deep State
        public const string DeepStateContact = "deep_state_contact";
        public const string DeepStateInsider = "deep_state_insider";
        public const string DeepStateOperative = "deep_state_operative";

        // Lucky items
        public const string RabbitFoot = "rabbit_foot";
        public const string FourLeafClover = "four_leaf_clover";
        public const string Horseshoe = "horseshoe";

        // Believer/Charisma
        public const string Charisma = "charisma";
        public const string CultLeadership = "cult_leadership";
        public const string Hypnosis = "hypnosis";
        public const string MindControlCrown = "mind_control_crown";

        // Auto-clickers
        public const string ClickingIntern = "clicking_intern";
        public const string ClickingRobot = "clicking_robot";
        public const string QuantumClicker = "quantum_clicker";
        public const string NeuralClicker = "neural_clicker";

        // Critical chance
        public const string LuckyStar = "lucky_star";
        public const string PropheticVision = "prophetic_vision";
        public const string ThirdEyeOpen = "third_eye_open";

        // Elite
        public const string RealityDistortion = "reality_distortion";
        public const string FateWeaver = "fate_weaver";
    }

    // Pre-defined icon keys for Random Events
    public static class Events
    {
        public const string GoldenEye = "event_golden_eye";
        public const string WhistleBlower = "event_whistleblower";
        public const string MysteryPackage = "event_mystery_package";
        public const string TinfoilRain = "event_tinfoil_rain";
        public const string DeepStateLeak = "event_deep_state_leak";
        public const string Automated = "event_automated";
        public const string Viral = "event_viral";
        public const string Insider = "event_insider";
    }

    // Pre-defined icon keys for Matrix Upgrades
    public static class Matrix
    {
        public const string RealityWarp = "matrix_reality_warp";
        public const string NeoClicking = "matrix_neo_clicking";
        public const string Agent = "matrix_agent";
        public const string SourceCode = "matrix_source_code";
        public const string BulletTime = "matrix_bullet_time";
        public const string Architect = "matrix_architect";
        public const string RedPill = "matrix_red_pill";
        public const string Oracle = "matrix_oracle";
        public const string Zion = "matrix_zion";
        public const string TheOne = "matrix_the_one";
    }

    // Pre-defined icon keys for Daily Challenges
    public static class Challenges
    {
        public const string Clicks = "challenge_clicks";
        public const string Crits = "challenge_crits";
        public const string Combos = "challenge_combos";
        public const string Quests = "challenge_quests";
        public const string Evidence = "challenge_evidence";
    }

    // Pre-defined icon keys for Achievement categories
    public static class Achievements
    {
        public const string Evidence = "achievement_evidence";
        public const string Clicks = "achievement_clicks";
        public const string Generators = "achievement_generators";
        public const string Conspiracies = "achievement_conspiracies";
        public const string Playtime = "achievement_playtime";
        public const string Prestige = "achievement_prestige";
        public const string Quests = "achievement_quests";
        public const string Meta = "achievement_meta";
    }

    // Pre-defined icon keys for Challenge Modes
    public static class ChallengeModes
    {
        public const string Speedrun = "challenge_speedrun";
        public const string Lightning = "challenge_lightning";
        public const string Observer = "challenge_observer";
        public const string Minimalist = "challenge_minimalist";
        public const string NoPrestige = "challenge_no_prestige";
        public const string Risky = "challenge_risky";
        public const string ClickMaster = "challenge_click_master";
    }

    // Pre-defined icon keys for Quest Risk Levels
    public static class RiskLevels
    {
        public const string Low = "risk_low";
        public const string Medium = "risk_medium";
        public const string High = "risk_high";
    }
}
