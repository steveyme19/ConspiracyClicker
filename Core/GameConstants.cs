namespace ConspiracyClicker.Core;

/// <summary>
/// Central location for all game balance constants and configuration values.
/// Modifying these values will affect game balance without changing code logic.
/// </summary>
public static class GameConstants
{
    // === TIMING ===
    public const int TICK_RATE_MS = 100;
    public const int AUTO_SAVE_INTERVAL_MS = 15000; // 15 seconds

    // === COMBO SYSTEM ===
    public const double COMBO_DECAY_RATE = 0.15;        // How fast combo meter drains per tick
    public const double COMBO_FILL_PER_CLICK = 0.08;    // How much each click fills the combo meter
    public const int COMBO_BURST_CLICKS = 10;           // Combo burst gives value of X clicks

    // === CRITICAL HITS ===
    public const double CRIT_MULTIPLIER_MIN = 5.0;      // Minimum critical hit multiplier
    public const double CRIT_MULTIPLIER_MAX = 10.0;     // Maximum critical hit multiplier
    public const double CRIT_MULTIPLIER_AVG = 7.5;      // Average for display purposes

    // === PRESTIGE ===
    public const double PRESTIGE_THRESHOLD = 50_000; // 50K total evidence to prestige (first ascension ~2h)
    public const double STARTING_EVIDENCE_BONUS = 1_000_000_000;    // 1B starting evidence with upgrade

    // === OFFLINE PROGRESS ===
    public const double OFFLINE_EARNINGS_RATE = 0.25;   // 25% of normal earnings while offline
    public const double MAX_OFFLINE_HOURS = 24;         // Cap offline earnings at 24 hours

    // === BASE CLICK POWER ===
    public const double BASE_CLICK_POWER = 1.0;
    public const double BASE_EPS_TO_CLICK_BONUS = 0.01; // Clicks always give 1% of EPS as bonus

    // === SKILL MULTIPLIERS ===
    public static class Skills
    {
        // Click Power Skills
        public const double QUICK_FINGERS = 1.15;
        public const double ONE_WITH_THE_CLICK = 2.0;

        // Research Skills
        public const double RESEARCH_BASICS = 1.10;
        public const double SPEED_READING = 1.15;
        public const double DATA_MINING = 1.25;
        public const double QUANTUM_ANALYSIS = 1.50;
        public const double OMNISCIENCE = 2.0;

        // Critical Skills
        public const double PRECISION_CLICKING_CRIT_BONUS = 0.05;
        public const double DEADLY_PRECISION_CRIT_BONUS = 0.10;
        public const double DEADLY_PRECISION_CRIT_MIN = 10.0;
        public const double DEADLY_PRECISION_CRIT_MAX = 15.0;

        // Believer Skills
        public const double CHARISMA = 1.10;
        public const double PERSUASION = 1.15;
        public const double CULT_OF_PERSONALITY = 1.50;
        public const double MIND_CONTROL = 2.0;
        public const double VIRAL_MARKETING_BONUS = 0.10;
    }

    // === ILLUMINATI UPGRADE MULTIPLIERS ===
    public static class Illuminati
    {
        public const double SECRET_HANDSHAKE = 1.10;
        public const double TOKEN_BONUS_PER_TOKEN = 0.01;   // 1% per token
        public const double PYRAMID_SCHEME = 1.05;
        public const double REPTILIAN_DNA = 2.0;
        public const double BELIEVER_MAGNETISM = 1.25;
        public const double MOON_BASE_ALPHA = 1.50;
        public const double NEW_WORLD_ORDER_DISCOUNT = 0.9; // 10% off
    }

    // === MATRIX UPGRADE MULTIPLIERS ===
    public static class Matrix
    {
        public const double REALITY_WARP = 3.0;
        public const double THE_ONE = 10.0;
        public const double BULLET_TIME = 5.0;
        public const double SOURCE_CODE_ACCESS = 2.0;
        public const double ARCHITECT_MEETING = 0.5;        // Reduces cost
        public const double AGENT_INFILTRATION_BONUS = 1.0;
    }

    // === CONSPIRACY BONUSES ===
    public static class Conspiracies
    {
        public const double YOU_ARE_CONSPIRACY = 2.0;
    }

    // === QUEST REWARDS ===
    public static class Quests
    {
        public const double CULT_OF_PERSONALITY_REWARD = 1.25;
    }

    // === RANDOM EVENTS ===
    public static class Events
    {
        public const double GOLDEN_EYE_SPAWN_CHANCE = 0.001;    // Per tick
        public const double WHISTLE_BLOWER_SPAWN_CHANCE = 0.0005;
        public const int GOLDEN_EYE_MIN_CLICKS = 50;
        public const int WHISTLE_BLOWER_MIN_CLICKS = 100;
    }
}
