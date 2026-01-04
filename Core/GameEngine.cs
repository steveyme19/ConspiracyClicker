using System.Windows.Threading;
using ConspiracyClicker.Data;
using ConspiracyClicker.Models;

namespace ConspiracyClicker.Core;

public class GameEngine
{
    private GameState _state;
    private readonly SaveManager _saveManager;
    private readonly DispatcherTimer _gameLoop;
    private readonly DispatcherTimer _autoSaveTimer;
    private readonly Random _random = new();

    private double _autoClickAccumulator = 0;

    public event Action? OnTick;
    public event Action<Achievement>? OnAchievementUnlocked;
    public event Action<string>? OnFlavorMessage;
    public event Action<double>? OnComboBurst;
    public event Action<double, bool>? OnClickProcessed; // clickPower, isCritical
    public event Action<string, bool, double, long>? OnQuestComplete;
    public event Action? OnGoldenEyeStart;
    public event Action? OnGoldenEyeEnd;
    public event Action? OnWhistleBlowerSpawn;
    public event Action? OnPrestigeAvailable;
    public event Action<StoredChallenge>? OnDailyChallengeComplete;
    public event Action? OnPrestigeComplete;
    public event Action<double, double, TimeSpan>? OnOfflineProgress; // evidenceEarned, believersGained, timeAway

    private bool _prestigeNotified = false;
    private DateTime _lastDailyCheck = DateTime.MinValue;
    private int _normalizationCounter = 0;

    public GameState State => _state;

    /// <summary>
    /// Aggressively reduces precision based on magnitude.
    /// Larger numbers get fewer significant figures since precision doesn't matter.
    /// </summary>
    private static double NormalizeLargeNumber(double value)
    {
        if (value <= 0 || value < 1_000) return value;

        // Calculate magnitude (power of 10)
        double magnitude = Math.Floor(Math.Log10(value));

        // Adaptive precision: fewer sig figs for larger numbers
        // < 1M: 6 sig figs, < 1B: 5 sig figs, < 1T: 4 sig figs, >= 1T: 3 sig figs
        int sigFigs = magnitude switch
        {
            < 6 => 6,   // < 1M
            < 9 => 5,   // < 1B
            < 12 => 4,  // < 1T
            _ => 3      // >= 1T (3 sig figs is plenty for huge numbers)
        };

        double scale = Math.Pow(10, magnitude - (sigFigs - 1));
        return Math.Round(value / scale) * scale;
    }
    public SaveManager SaveManager => _saveManager;

    public GameEngine()
    {
        _saveManager = new SaveManager();
        _state = new GameState(); // Start with empty state, load via menu

        _gameLoop = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(GameConstants.TICK_RATE_MS) };
        _gameLoop.Tick += GameLoop_Tick;

        _autoSaveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(GameConstants.AUTO_SAVE_INTERVAL_MS) };
        _autoSaveTimer.Tick += (s, e) => Save();
    }

    public void LoadSlot(int slot)
    {
        // Stop timers without saving - we're about to replace the state
        _gameLoop.Stop();
        _autoSaveTimer.Stop();

        var loadedState = _saveManager.LoadFromSlot(slot);
        if (loadedState != null)
        {
            // Replace state entirely with loaded state
            _state = loadedState;
            _saveManager.SetCurrentSlot(slot);

            // Apply offline progress if this is a real save
            if (_state.TotalPlayTimeSeconds > 0)
            {
                CalculateOfflineProgress();
            }
        }
        // If loadedState is null, keep current state
    }

    private void CalculateOfflineProgress()
    {
        if (_state.LastSaveTime == DateTime.MinValue) return;
        if (_state.ActiveChallengeId != null) return; // No offline progress during challenges

        TimeSpan timeAway = DateTime.Now - _state.LastSaveTime;
        if (timeAway.TotalMinutes < 1) return; // Minimum 1 minute away

        // Cap at max hours
        double secondsAway = Math.Min(timeAway.TotalSeconds, GameConstants.MAX_OFFLINE_HOURS * 3600);

        // Calculate base EPS (without active events)
        double eps = CalculateEvidencePerSecond();
        if (eps <= 0) return;

        // Apply offline earnings (reduced rate)
        double evidenceEarned = eps * secondsAway * GameConstants.OFFLINE_EARNINGS_RATE;
        _state.Evidence += evidenceEarned;
        _state.TotalEvidenceEarned += evidenceEarned;

        // Apply believer growth (generators that produce believers)
        double believersGained = 0;
        foreach (var gen in GeneratorData.AllGenerators)
        {
            if (gen.BelieverBonus > 0)
            {
                int owned = _state.GetGeneratorCount(gen.Id);
                if (owned > 0)
                {
                    // Believer growth at reduced offline rate
                    double growth = gen.BelieverBonus * owned * (secondsAway / 60.0) * GameConstants.OFFLINE_EARNINGS_RATE;
                    believersGained += growth;
                }
            }
        }
        _state.Believers += believersGained;

        // Passive tinfoil from Matrix upgrade
        if (_state.MatrixUpgrades.Contains("red_pill_factory"))
        {
            int tinfoilGained = (int)(secondsAway / 60.0 * 5 * GameConstants.OFFLINE_EARNINGS_RATE); // 5 per minute at reduced rate
            _state.Tinfoil += tinfoilGained;
        }

        // Notify UI
        OnOfflineProgress?.Invoke(evidenceEarned, believersGained, TimeSpan.FromSeconds(secondsAway));
    }

    public void NewGame(int slot)
    {
        Stop();
        _saveManager.SetCurrentSlot(slot);
        var newState = new GameState();
        CopyState(newState);
        Save(); // Save the new empty state
    }

    private void CopyState(GameState source)
    {
        // Copy all properties from source to current state
        _state.Evidence = source.Evidence;
        _state.TotalEvidenceEarned = source.TotalEvidenceEarned;
        _state.Believers = source.Believers;
        _state.BusyBelievers = source.BusyBelievers;
        _state.Tinfoil = source.Tinfoil;
        _state.IlluminatiTokens = source.IlluminatiTokens;
        _state.GlitchTokens = source.GlitchTokens;
        _state.TimesAscended = source.TimesAscended;
        _state.TimesMatrixBroken = source.TimesMatrixBroken;
        _state.TotalIlluminatiTokensEarned = source.TotalIlluminatiTokensEarned;
        _state.TotalPlayTimeSeconds = source.TotalPlayTimeSeconds;
        _state.LastSaveTime = source.LastSaveTime;
        _state.ComboMeter = source.ComboMeter;
        _state.ComboClicks = source.ComboClicks;
        _state.TotalClicks = source.TotalClicks;
        _state.CriticalClicks = source.CriticalClicks;
        _state.QuestsCompleted = source.QuestsCompleted;
        _state.QuestsFailed = source.QuestsFailed;
        _state.BelieversLost = source.BelieversLost;
        _state.SkillPoints = source.SkillPoints;

        // Daily challenge tracking
        _state.LastDailyChallengeDate = source.LastDailyChallengeDate;
        _state.TodayClicks = source.TodayClicks;
        _state.TodayEvidence = source.TodayEvidence;
        _state.TodayQuestsCompleted = source.TodayQuestsCompleted;
        _state.TodayCriticalHits = source.TodayCriticalHits;
        _state.TodayCombos = source.TodayCombos;

        // Active events
        _state.GoldenEyeActive = source.GoldenEyeActive;
        _state.GoldenEyeEndTime = source.GoldenEyeEndTime;
        _state.WhistleBlowerActive = source.WhistleBlowerActive;
        _state.WhistleBlowerEndTime = source.WhistleBlowerEndTime;
        _state.WhistleBlowerX = source.WhistleBlowerX;
        _state.WhistleBlowerY = source.WhistleBlowerY;

        _state.Generators.Clear();
        foreach (var (k, v) in source.Generators) _state.Generators[k] = v;

        _state.PurchasedUpgrades.Clear();
        foreach (var u in source.PurchasedUpgrades) _state.PurchasedUpgrades.Add(u);

        _state.ProvenConspiracies.Clear();
        foreach (var c in source.ProvenConspiracies) _state.ProvenConspiracies.Add(c);

        _state.UnlockedAchievements.Clear();
        foreach (var a in source.UnlockedAchievements) _state.UnlockedAchievements.Add(a);

        _state.TinfoilShopPurchases.Clear();
        foreach (var t in source.TinfoilShopPurchases) _state.TinfoilShopPurchases.Add(t);

        _state.ActiveQuests.Clear();
        foreach (var q in source.ActiveQuests) _state.ActiveQuests.Add(q);

        _state.MatrixUpgrades.Clear();
        foreach (var m in source.MatrixUpgrades) _state.MatrixUpgrades.Add(m);

        _state.IlluminatiUpgrades.Clear();
        foreach (var i in source.IlluminatiUpgrades) _state.IlluminatiUpgrades.Add(i);

        _state.UnlockedSkills.Clear();
        foreach (var s in source.UnlockedSkills) _state.UnlockedSkills.Add(s);

        _state.DailyChallenges.Clear();
        foreach (var d in source.DailyChallenges) _state.DailyChallenges.Add(d);

        // Generator-specific upgrades
        _state.GeneratorUpgrades.Clear();
        foreach (var g in source.GeneratorUpgrades) _state.GeneratorUpgrades.Add(g);

        // Settings
        _state.ZenMode = source.ZenMode;

        // Challenge mode state
        _state.ActiveChallengeId = source.ActiveChallengeId;
        _state.ChallengeStartTime = source.ChallengeStartTime;
        _state.ChallengeProgress = source.ChallengeProgress;
        _state.ChallengeClickCount = source.ChallengeClickCount;
        _state.ChallengeHighRiskQuestsCompleted = source.ChallengeHighRiskQuestsCompleted;
        _state.CompletedChallenges.Clear();
        foreach (var c in source.CompletedChallenges) _state.CompletedChallenges.Add(c);

        _prestigeNotified = false;
    }

    public void Start()
    {
        var eps = CalculateEvidencePerSecond();
        if (eps > 0)
        {
            var (offlineEvidence, offlineTime) = _saveManager.CalculateOfflineProgress(_state, eps);
            if (offlineEvidence > 0 && offlineTime.TotalMinutes >= 1)
            {
                _state.Evidence += offlineEvidence;
                _state.TotalEvidenceEarned += offlineEvidence;
                OnFlavorMessage?.Invoke($"While you were gone ({offlineTime.Hours}h {offlineTime.Minutes}m), you gathered {Utils.NumberFormatter.Format(offlineEvidence)} evidence!");
            }
        }

        _gameLoop.Start();
        _autoSaveTimer.Start();
    }

    public void Stop()
    {
        _gameLoop.Stop();
        _autoSaveTimer.Stop();
        Save();
    }

    public void Save() => _saveManager.Save(_state);

    private void GameLoop_Tick(object? sender, EventArgs e)
    {
        double eps = CalculateEvidencePerSecond();
        double evidenceThisTick = eps * (GameConstants.TICK_RATE_MS / 1000.0);

        _state.Evidence += evidenceThisTick;
        _state.TotalEvidenceEarned += evidenceThisTick;
        _state.TotalPlayTimeSeconds += GameConstants.TICK_RATE_MS / 1000.0;

        // Normalize large numbers every 10 ticks to reduce floating-point overhead
        if (++_normalizationCounter >= 10)
        {
            _normalizationCounter = 0;
            _state.Evidence = NormalizeLargeNumber(_state.Evidence);
            _state.TotalEvidenceEarned = NormalizeLargeNumber(_state.TotalEvidenceEarned);
        }

        UpdateBelievers();
        UpdateComboMeter();
        ProcessAutoClicks();
        TryAutoStartQuests();
        CheckQuests();
        CheckRandomEvents();
        CheckEventExpiry();
        CheckDailyChallenges();
        CheckAchievements();
        CheckPrestige();
        OnTick?.Invoke();
    }

    private void UpdateBelievers()
    {
        double totalBelievers = 0;
        foreach (var (genId, count) in _state.Generators)
        {
            var generator = GeneratorData.GetById(genId);
            if (generator != null)
            {
                // Apply generator-specific believer multiplier from upgrades (level 75, 200)
                double believerMult = GetGeneratorUpgradeBelieverMultiplier(genId);
                totalBelievers += generator.BelieverBonus * count * believerMult;
            }
        }

        // Apply Tinfoil Shop believer multiplier at the end
        totalBelievers *= GetTinfoilBelieverMultiplier();

        // Apply Skill Tree believer multiplier
        totalBelievers *= GetSkillBelieverMultiplier();

        // Apply Prestige believer bonus
        if (_state.IlluminatiUpgrades.Contains("believer_magnetism")) totalBelievers *= 6.0; // +500%
        if (_state.IlluminatiUpgrades.Contains("global_awakening")) totalBelievers *= 21.0; // +2000%
        if (_state.IlluminatiUpgrades.Contains("believer_singularity")) totalBelievers *= 251.0; // +25000%

        // Apply Matrix believer bonus
        totalBelievers *= GetMatrixBelieverMultiplier();

        // Apply generator upgrade believer bonus
        totalBelievers *= GetGeneratorUpgradeGlobalBelieverMultiplier();

        // Add permanent bonus believers from quests
        totalBelievers += _state.BonusBelievers;

        _state.Believers = totalBelievers;
    }

    private void UpdateComboMeter()
    {
        double timeSinceClick = (DateTime.Now - _state.LastClickTime).TotalSeconds;
        if (timeSinceClick > 0.5)
        {
            _state.ComboMeter -= GameConstants.COMBO_DECAY_RATE * (GameConstants.TICK_RATE_MS / 1000.0);
            if (_state.ComboMeter < 0) _state.ComboMeter = 0;
            if (timeSinceClick > 2) _state.ComboClicks = 0;
        }
    }

    private void ProcessAutoClicks()
    {
        double autoClickRate = GetAutoClickRate();
        if (autoClickRate <= 0) return;

        _autoClickAccumulator += autoClickRate * (GameConstants.TICK_RATE_MS / 1000.0);

        while (_autoClickAccumulator >= 1.0)
        {
            _autoClickAccumulator -= 1.0;
            ProcessClick(isAutoClick: true);
        }
    }

    // === CLICK PROCESSING ===
    public void ProcessClick(bool isAutoClick = false, double externalMultiplier = 1.0)
    {
        double clickPower = CalculateClickPower();
        bool isCritical = false;

        // Check for critical hit
        double critChance = GetCriticalChance();
        if (critChance > 0 && _random.NextDouble() < critChance)
        {
            double critMultiplier = GetCriticalMultiplier();
            clickPower *= critMultiplier;
            isCritical = true;
            _state.CriticalClicks++;
            _state.TodayCriticalHits++;
        }

        // Golden Eye bonus
        if (_state.GoldenEyeActive && DateTime.Now < _state.GoldenEyeEndTime)
            clickPower *= 5;

        // External multiplier (from lucky drops, etc.)
        clickPower *= externalMultiplier;

        _state.Evidence += clickPower;
        _state.TotalEvidenceEarned += clickPower;
        _state.TodayEvidence += clickPower;
        _state.TotalClicks++;
        _state.TodayClicks++;

        if (!isAutoClick)
        {
            _state.LastClickTime = DateTime.Now;
            _state.ComboClicks++;

            // Fill combo meter
            _state.ComboMeter += GameConstants.COMBO_FILL_PER_CLICK;
            if (_state.ComboMeter >= 1.0)
            {
                TriggerComboBurst();
            }
        }

        OnClickProcessed?.Invoke(clickPower, isCritical);
        CheckAchievements();
        OnTick?.Invoke();
    }

    private void TriggerComboBurst()
    {
        double burstAmount = CalculateClickPower() * GameConstants.COMBO_BURST_CLICKS;

        // Apply Golden Eye bonus to combo burst
        if (_state.GoldenEyeActive && DateTime.Now < _state.GoldenEyeEndTime)
            burstAmount *= 10;

        _state.Evidence += burstAmount;
        _state.TotalEvidenceEarned += burstAmount;
        _state.TodayEvidence += burstAmount;
        _state.ComboMeter = 0;
        _state.ComboClicks = 0;
        _state.TodayCombos++;
        OnComboBurst?.Invoke(burstAmount);
    }

    public double CalculateClickPower()
    {
        var (basePower, multiplier, epsComponent) = GetClickPowerBreakdown();
        return (basePower * multiplier) + epsComponent;
    }

    public (double basePower, double multiplier, double epsComponent) GetClickPowerBreakdown()
    {
        double basePower = 1.0;
        double multiplier = 1.0;
        double epsBonus = 0.001; // Baseline: clicks always give 0.1% of EPS

        foreach (var upgradeId in _state.PurchasedUpgrades)
        {
            var upgrade = UpgradeData.GetById(upgradeId);
            if (upgrade == null) continue;

            switch (upgrade.Type)
            {
                case UpgradeType.ClickPower: basePower += upgrade.Value; break;
                case UpgradeType.ClickMultiplier: multiplier *= upgrade.Value; break;
                case UpgradeType.EpsToClick: epsBonus += upgrade.Value; break;
            }
        }

        foreach (var conspiracyId in _state.ProvenConspiracies)
        {
            var conspiracy = ConspiracyData.GetById(conspiracyId);
            if (conspiracy != null)
            {
                basePower += conspiracy.ClickBonus;
                multiplier *= conspiracy.MultiplierBonus;
            }
        }

        foreach (var achievementId in _state.UnlockedAchievements)
        {
            var achievement = AchievementData.GetById(achievementId);
            if (achievement != null)
            {
                basePower += achievement.ClickBonusReward;
                multiplier *= achievement.MultiplierReward;
            }
        }

        // Tinfoil Shop click power bonuses (applied at end)
        multiplier *= GetTinfoilClickMultiplier();

        // Skill tree bonuses
        if (_state.UnlockedSkills.Contains("quick_fingers")) multiplier *= 1.15;
        if (_state.UnlockedSkills.Contains("one_with_the_click")) multiplier *= 2.0;

        // Prestige bonuses
        if (_state.IlluminatiUpgrades.Contains("secret_handshake")) multiplier *= 50.0;
        if (_state.IlluminatiUpgrades.Contains("reality_distortion")) multiplier *= 100.0;
        if (_state.IlluminatiUpgrades.Contains("reality_overwrite")) multiplier *= 25.0;
        if (_state.IlluminatiUpgrades.Contains("click_transcendence")) multiplier *= 500.0;
        if (_state.IlluminatiUpgrades.Contains("final_truth")) multiplier *= 200.0;
        if (_state.IlluminatiUpgrades.Contains("all_seeing_investment"))
            multiplier *= 1.0 + (_state.IlluminatiTokens * 0.25); // Boosted from 0.05 to 0.25

        // Matrix bonuses
        multiplier *= GetMatrixClickMultiplier();
        epsBonus += GetMatrixEpsToClickBonus();

        // Generator upgrade global click power bonus
        multiplier *= GetGeneratorUpgradeGlobalClickMultiplier();

        // EPS to click component from upgrades
        double epsComponent = epsBonus > 0 ? CalculateBaseEps() * GetEpsMultiplier() * epsBonus : 0;
        return (basePower, multiplier, epsComponent);
    }

    public double CalculateEvidencePerSecond()
    {
        return CalculateBaseEps() * GetEpsMultiplier();
    }

    public double CalculateBaseEps()
    {
        double total = 0;

        var generatorMultipliers = new Dictionary<string, double>();
        foreach (var upgradeId in _state.PurchasedUpgrades)
        {
            var upgrade = UpgradeData.GetById(upgradeId);
            if (upgrade == null) continue;

            if (upgrade.Type == UpgradeType.GeneratorBoost && upgrade.TargetGeneratorId != null)
            {
                if (!generatorMultipliers.ContainsKey(upgrade.TargetGeneratorId))
                    generatorMultipliers[upgrade.TargetGeneratorId] = 1.0;
                generatorMultipliers[upgrade.TargetGeneratorId] *= upgrade.Value;
            }
        }

        foreach (var (genId, count) in _state.Generators)
        {
            var generator = GeneratorData.GetById(genId);
            if (generator != null)
            {
                double genMultiplier = generatorMultipliers.TryGetValue(genId, out var m) ? m : 1.0;
                // Apply generator-specific upgrades
                genMultiplier *= GetGeneratorUpgradeProductionMultiplier(genId);
                total += generator.GetProduction(count) * genMultiplier;
            }
        }

        return total;
    }

    public double GetEpsMultiplier()
    {
        double multiplier = 1.0;

        // Global boost upgrades
        foreach (var upgradeId in _state.PurchasedUpgrades)
        {
            var upgrade = UpgradeData.GetById(upgradeId);
            if (upgrade?.Type == UpgradeType.GlobalBoost)
                multiplier *= upgrade.Value;
        }

        // Tinfoil Shop EPS bonuses (applied at end)
        multiplier *= GetTinfoilEpsMultiplier();

        // Skill tree bonuses
        if (_state.UnlockedSkills.Contains("research_basics")) multiplier *= 1.10;
        if (_state.UnlockedSkills.Contains("speed_reading")) multiplier *= 1.15;
        if (_state.UnlockedSkills.Contains("data_mining")) multiplier *= 1.25;
        if (_state.UnlockedSkills.Contains("quantum_analysis")) multiplier *= 1.50;
        if (_state.UnlockedSkills.Contains("omniscience")) multiplier *= 2.0;

        // Prestige bonuses - massively boosted for faster progression
        // Tier 1 (1-3 tokens)
        if (_state.IlluminatiUpgrades.Contains("pyramid_scheme")) multiplier *= 100.0;
        if (_state.IlluminatiUpgrades.Contains("reptilian_dna")) multiplier *= 100.0;
        if (_state.IlluminatiUpgrades.Contains("deep_state_connections")) multiplier *= 50.0;
        // Tier 2 (4-10 tokens)
        if (_state.IlluminatiUpgrades.Contains("ancient_knowledge")) multiplier *= 100.0;
        // Tier 3 (12-30 tokens)
        if (_state.IlluminatiUpgrades.Contains("parallel_universe_access")) multiplier *= 10.0;
        if (_state.IlluminatiUpgrades.Contains("cosmic_alignment")) multiplier *= 200.0;
        // Tier 4 (40-150 tokens)
        if (_state.IlluminatiUpgrades.Contains("illuminati_council_seat")) multiplier *= 500.0;
        if (_state.IlluminatiUpgrades.Contains("eternal_conspiracy")) multiplier *= 1000.0;
        if (_state.IlluminatiUpgrades.Contains("reality_overwrite")) multiplier *= 25.0;
        // Tier 5 (200-300 tokens)
        if (_state.IlluminatiUpgrades.Contains("entropy_mastery")) multiplier *= 2000.0;
        // Tier 6 (350+ tokens)
        if (_state.IlluminatiUpgrades.Contains("evidence_singularity")) multiplier *= 5000.0;
        if (_state.IlluminatiUpgrades.Contains("omnipresent_network")) multiplier *= 50.0;
        if (_state.IlluminatiUpgrades.Contains("final_truth")) multiplier *= 200.0;

        // Conspiracy bonus
        if (_state.ProvenConspiracies.Contains("you_are_conspiracy")) multiplier *= 2.0;

        // Matrix bonuses
        multiplier *= GetMatrixEpsMultiplier();

        // Generator upgrade global EPS bonus
        multiplier *= GetGeneratorUpgradeGlobalEpsMultiplier();

        return multiplier;
    }

    public (double baseEps, double multiplier) GetEpsBreakdown()
    {
        return (CalculateBaseEps(), GetEpsMultiplier());
    }

    // === TINFOIL SHOP ===
    public double GetTinfoilClickMultiplier()
    {
        double multiplier = 1.0;
        foreach (var purchaseId in _state.TinfoilShopPurchases)
        {
            var upgrade = TinfoilShopData.GetById(purchaseId);
            if (upgrade?.Type == TinfoilUpgradeType.ClickPower)
                multiplier *= upgrade.Value;
        }
        return multiplier;
    }

    public double GetTinfoilEpsMultiplier()
    {
        double multiplier = 1.0;
        foreach (var purchaseId in _state.TinfoilShopPurchases)
        {
            var upgrade = TinfoilShopData.GetById(purchaseId);
            if (upgrade?.Type == TinfoilUpgradeType.EpsMultiplier)
                multiplier *= upgrade.Value;
        }
        return multiplier;
    }

    public double GetTinfoilQuestSuccessBonus()
    {
        double bonus = 0;
        foreach (var purchaseId in _state.TinfoilShopPurchases)
        {
            var upgrade = TinfoilShopData.GetById(purchaseId);
            if (upgrade?.Type == TinfoilUpgradeType.QuestSuccess)
                bonus += upgrade.Value;
        }
        return bonus;
    }

    public double GetTinfoilBelieverMultiplier()
    {
        double multiplier = 1.0;
        foreach (var purchaseId in _state.TinfoilShopPurchases)
        {
            var upgrade = TinfoilShopData.GetById(purchaseId);
            if (upgrade?.Type == TinfoilUpgradeType.BelieverBonus)
                multiplier *= upgrade.Value;
        }
        return multiplier;
    }

    public double GetAutoClickRate()
    {
        double rate = 0;
        foreach (var purchaseId in _state.TinfoilShopPurchases)
        {
            var upgrade = TinfoilShopData.GetById(purchaseId);
            if (upgrade?.Type == TinfoilUpgradeType.AutoClicker)
                rate += upgrade.Value;
        }
        // Cap auto-click rate at 50 CPS
        return Math.Min(rate, 50.0);
    }

    public double GetCriticalChance()
    {
        double chance = 0;
        foreach (var purchaseId in _state.TinfoilShopPurchases)
        {
            var upgrade = TinfoilShopData.GetById(purchaseId);
            if (upgrade?.Type == TinfoilUpgradeType.CriticalChance)
                chance += upgrade.Value;
        }
        chance += GetSkillCriticalChanceBonus();
        if (_state.IlluminatiUpgrades.Contains("third_eye_awakening")) chance += 0.50;
        chance += GetGeneratorUpgradeGlobalCritChance();
        return chance;
    }

    public IEnumerable<TinfoilUpgrade> GetAvailableTinfoilUpgrades()
    {
        return TinfoilShopData.AllUpgrades.Where(u =>
            !_state.TinfoilShopPurchases.Contains(u.Id) &&
            // AutoQuest requires 8+ auto CPS to unlock
            (u.Type != TinfoilUpgradeType.AutoQuest || GetAutoClickRate() >= 8));
    }

    public bool HasAutoQuest()
    {
        return _state.TinfoilShopPurchases.Contains("quest_autopilot");
    }

    public void ToggleAutoQuest()
    {
        _state.AutoQuestEnabled = !_state.AutoQuestEnabled;
    }

    public bool IsAutoQuestEnabled => _state.AutoQuestEnabled;

    public IEnumerable<TinfoilUpgrade> GetPurchasedTinfoilUpgrades()
    {
        return TinfoilShopData.AllUpgrades.Where(u => _state.TinfoilShopPurchases.Contains(u.Id));
    }

    public bool CanAffordTinfoilUpgrade(string upgradeId)
    {
        var upgrade = TinfoilShopData.GetById(upgradeId);
        if (upgrade == null || _state.TinfoilShopPurchases.Contains(upgradeId)) return false;
        return _state.Tinfoil >= upgrade.TinfoilCost;
    }

    public bool PurchaseTinfoilUpgrade(string upgradeId)
    {
        if (!CanAffordTinfoilUpgrade(upgradeId)) return false;
        var upgrade = TinfoilShopData.GetById(upgradeId);
        if (upgrade == null) return false;

        _state.Tinfoil -= upgrade.TinfoilCost;
        _state.TinfoilShopPurchases.Add(upgradeId);
        OnTick?.Invoke();
        return true;
    }

    // === QUESTS ===
    public IEnumerable<Quest> GetAvailableQuests()
    {
        return QuestData.GetAvailable(_state.AvailableBelievers);
    }

    public bool CanStartQuest(string questId)
    {
        var quest = QuestData.GetById(questId);
        if (quest == null) return false;
        if (_state.ActiveQuests.Any(q => q.QuestId == questId)) return false;
        return _state.AvailableBelievers >= quest.BelieversRequired;
    }

    public bool StartQuest(string questId)
    {
        if (!CanStartQuest(questId)) return false;

        var quest = QuestData.GetById(questId);
        if (quest == null) return false;

        // Apply quest duration bonuses
        double durationMultiplier = 1.0;
        if (_state.IlluminatiUpgrades.Contains("time_manipulation")) durationMultiplier *= 0.10; // -90%
        if (_state.IlluminatiUpgrades.Contains("temporal_fold")) durationMultiplier *= 0.02; // -98%
        if (_state.IlluminatiUpgrades.Contains("probability_control")) durationMultiplier *= 0.10; // -90%
        durationMultiplier *= GetGeneratorUpgradeGlobalQuestSpeed();
        double adjustedDuration = quest.DurationSeconds * durationMultiplier;

        var activeQuest = new ActiveQuest
        {
            QuestId = questId,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddSeconds(adjustedDuration),
            BelieversSent = quest.BelieversRequired
        };

        _state.ActiveQuests.Add(activeQuest);
        _state.BusyBelievers += quest.BelieversRequired;

        OnTick?.Invoke();
        return true;
    }

    private void TryAutoStartQuests()
    {
        if (!HasAutoQuest() || !_state.AutoQuestEnabled) return;

        // Get all available quests that can be started
        var availableQuests = QuestData.GetAvailable(_state.AvailableBelievers)
            .Where(q => CanStartQuest(q.Id))
            .OrderByDescending(q => q.EvidenceReward) // Prioritize higher reward quests
            .ToList();

        // Start as many quests as possible
        foreach (var quest in availableQuests)
        {
            if (CanStartQuest(quest.Id))
            {
                StartQuest(quest.Id);
            }
        }
    }

    private void CheckQuests()
    {
        var completedQuests = _state.ActiveQuests.Where(q => q.IsComplete).ToList();

        foreach (var activeQuest in completedQuests)
        {
            var quest = QuestData.GetById(activeQuest.QuestId);
            if (quest == null) continue;

            _state.ActiveQuests.Remove(activeQuest);
            _state.BusyBelievers -= activeQuest.BelieversSent;

            // Apply quest success bonus from Tinfoil Shop, Skills, Matrix, and Illuminati
            double adjustedSuccessChance = quest.SuccessChance + GetTinfoilQuestSuccessBonus() + GetSkillQuestSuccessBonus() + GetMatrixQuestSuccessBonus() + GetIlluminatiQuestSuccessBonus();
            adjustedSuccessChance = Math.Min(adjustedSuccessChance, 0.95);
            bool success = SkillQuestsNeverFail() || _random.NextDouble() < adjustedSuccessChance;
            double evidenceReward = 0;
            long tinfoilReward = 0;

            if (success)
            {
                evidenceReward = quest.EvidenceReward + (CalculateEvidencePerSecond() * quest.EvidenceMultiplier);

                // Apply prestige and skill quest reward bonuses
                double rewardMultiplier = 1.0;
                if (_state.IlluminatiUpgrades.Contains("moon_base_alpha")) rewardMultiplier *= 6.0; // +500%
                if (_state.UnlockedSkills.Contains("cult_of_personality")) rewardMultiplier *= 1.25;
                evidenceReward *= rewardMultiplier;

                tinfoilReward = quest.TinfoilReward;
                _state.Evidence += evidenceReward;
                _state.TotalEvidenceEarned += evidenceReward;
                _state.Tinfoil += tinfoilReward;

                // Award bonus believers from quest (recruited during mission) - permanent addition
                if (quest.BelieverReward > 0)
                {
                    _state.BonusBelievers += quest.BelieverReward * rewardMultiplier;
                }

                _state.QuestsCompleted++;
                _state.TodayQuestsCompleted++;
            }
            else
            {
                _state.QuestsFailed++;

                switch (quest.Risk)
                {
                    case QuestRisk.Low:
                        evidenceReward = (quest.EvidenceReward + (CalculateEvidencePerSecond() * quest.EvidenceMultiplier)) * quest.FailEvidenceMultiplier;
                        _state.Evidence += evidenceReward;
                        _state.TotalEvidenceEarned += evidenceReward;
                        break;

                    case QuestRisk.Medium:
                        break;

                    case QuestRisk.High:
                        // Believers are detained/lost permanently on high-risk failure
                        _state.Believers -= activeQuest.BelieversSent;
                        _state.BelieversLost += activeQuest.BelieversSent;
                        break;
                }
            }

            OnQuestComplete?.Invoke(activeQuest.QuestId, success, evidenceReward, tinfoilReward);
        }
    }

    // === RANDOM EVENTS ===
    private void CheckRandomEvents()
    {
        // Only start spawning after the first conspiracy is proven
        if (_state.ProvenConspiracies.Count == 0) return;

        // Don't spawn during challenge modes
        if (IsInChallenge) return;

        double spawnChance = 0.001 * GetGeneratorUpgradeGlobalGoldenEyeFrequency();
        if (_random.NextDouble() > spawnChance) return;
        if (_state.GoldenEyeActive || _state.WhistleBlowerActive) return;

        if (_random.NextDouble() < 0.5)
            StartGoldenEye();
        else
            SpawnWhistleBlower();
    }

    private void StartGoldenEye()
    {
        _state.GoldenEyeActive = true;
        _state.GoldenEyeEndTime = DateTime.Now.AddSeconds(10);
        OnGoldenEyeStart?.Invoke();
    }

    private void SpawnWhistleBlower()
    {
        _state.WhistleBlowerActive = true;
        _state.WhistleBlowerEndTime = DateTime.Now.AddSeconds(5);
        _state.WhistleBlowerX = _random.NextDouble();
        _state.WhistleBlowerY = _random.NextDouble();
        OnWhistleBlowerSpawn?.Invoke();
    }

    public bool ClickWhistleBlower()
    {
        if (!_state.WhistleBlowerActive) return false;

        double bonus = CalculateEvidencePerSecond() * 120;
        _state.Evidence += bonus;
        _state.TotalEvidenceEarned += bonus;
        _state.Tinfoil += 5;

        _state.WhistleBlowerActive = false;
        OnFlavorMessage?.Invoke($"Whistle-blower caught! +{Utils.NumberFormatter.Format(bonus)} evidence + 5 Tinfoil!");
        return true;
    }

    private void CheckEventExpiry()
    {
        if (_state.GoldenEyeActive && DateTime.Now >= _state.GoldenEyeEndTime)
        {
            _state.GoldenEyeActive = false;
            OnGoldenEyeEnd?.Invoke();
        }

        if (_state.WhistleBlowerActive && DateTime.Now >= _state.WhistleBlowerEndTime)
        {
            _state.WhistleBlowerActive = false;
        }
    }

    // === PRESTIGE ===
    private void CheckPrestige()
    {
        if (!_prestigeNotified && _state.TotalEvidenceEarned >= GameConstants.PRESTIGE_THRESHOLD)
        {
            _prestigeNotified = true;
            OnPrestigeAvailable?.Invoke();
        }
    }

    public bool CanPrestige() => _state.TotalEvidenceEarned >= GameConstants.PRESTIGE_THRESHOLD;

    // === BELIEVER INFO ===
    public Dictionary<string, double> GetBelieverBreakdown()
    {
        var breakdown = new Dictionary<string, double>();
        foreach (var (genId, count) in _state.Generators)
        {
            var generator = GeneratorData.GetById(genId);
            if (generator != null && generator.BelieverBonus > 0)
            {
                breakdown[genId] = generator.BelieverBonus * count;
            }
        }
        return breakdown;
    }

    // === UPGRADES ===
    public IEnumerable<Upgrade> GetAvailableUpgrades()
    {
        return UpgradeData.GetAvailable(_state.TotalEvidenceEarned, _state.Generators, _state.PurchasedUpgrades);
    }

    public bool CanAffordUpgrade(string upgradeId)
    {
        var upgrade = UpgradeData.GetById(upgradeId);
        if (upgrade == null || _state.PurchasedUpgrades.Contains(upgradeId)) return false;
        return upgrade.TinfoilCost > 0 ? _state.Tinfoil >= upgrade.TinfoilCost : _state.Evidence >= upgrade.EvidenceCost;
    }

    public bool PurchaseUpgrade(string upgradeId)
    {
        if (!CanAffordUpgrade(upgradeId)) return false;
        var upgrade = UpgradeData.GetById(upgradeId);
        if (upgrade == null) return false;

        if (upgrade.TinfoilCost > 0) _state.Tinfoil -= upgrade.TinfoilCost;
        else _state.Evidence -= upgrade.EvidenceCost;

        _state.PurchasedUpgrades.Add(upgradeId);
        OnTick?.Invoke();
        return true;
    }

    // === CONSPIRACIES ===
    public IEnumerable<Conspiracy> GetAvailableConspiracies()
    {
        return ConspiracyData.GetAvailable(_state.TotalEvidenceEarned, _state.ProvenConspiracies);
    }

    public bool CanAffordConspiracy(string conspiracyId)
    {
        var conspiracy = ConspiracyData.GetById(conspiracyId);
        if (conspiracy == null || _state.ProvenConspiracies.Contains(conspiracyId)) return false;
        // Unlocked by total evidence earned, not current evidence
        return _state.TotalEvidenceEarned >= conspiracy.EvidenceCost;
    }

    public bool ProveConspiracy(string conspiracyId)
    {
        if (!CanAffordConspiracy(conspiracyId)) return false;
        var conspiracy = ConspiracyData.GetById(conspiracyId);
        if (conspiracy == null) return false;

        // No evidence cost - just claim if you've earned enough total evidence
        _state.ProvenConspiracies.Add(conspiracyId);
        _state.Tinfoil += conspiracy.TinfoilReward;

        CheckAchievements();
        OnTick?.Invoke();
        return true;
    }

    // === ACHIEVEMENTS ===
    private void CheckAchievements()
    {
        foreach (var achievement in AchievementData.AllAchievements)
        {
            if (_state.UnlockedAchievements.Contains(achievement.Id)) continue;

            bool unlocked = achievement.Type switch
            {
                AchievementType.TotalEvidence => _state.TotalEvidenceEarned >= achievement.Threshold,
                AchievementType.TotalClicks => _state.TotalClicks >= achievement.Threshold,
                AchievementType.GeneratorOwned => achievement.TargetId != null &&
                    _state.GetGeneratorCount(achievement.TargetId) >= achievement.Threshold,
                AchievementType.ConspiraciesProven => _state.ProvenConspiracies.Count >= achievement.Threshold,
                AchievementType.PlayTime => _state.TotalPlayTimeSeconds >= achievement.Threshold,
                AchievementType.TimesAscended => _state.TimesAscended >= achievement.Threshold,
                AchievementType.TimesMatrixBroken => _state.TimesMatrixBroken >= achievement.Threshold,
                AchievementType.QuestsCompleted => _state.QuestsCompleted >= achievement.Threshold,
                AchievementType.TotalTinfoil => _state.Tinfoil >= achievement.Threshold,
                AchievementType.CriticalClicks => _state.CriticalClicks >= achievement.Threshold,
                AchievementType.TotalTokensEarned => _state.TotalIlluminatiTokensEarned >= achievement.Threshold,
                _ => false
            };

            if (unlocked)
            {
                _state.UnlockedAchievements.Add(achievement.Id);
                _state.Tinfoil += achievement.TinfoilReward;
                OnAchievementUnlocked?.Invoke(achievement);
            }
        }
    }

    public IEnumerable<Achievement> GetUnlockedAchievements() =>
        AchievementData.AllAchievements.Where(a => _state.UnlockedAchievements.Contains(a.Id));

    public IEnumerable<Achievement> GetLockedAchievements() =>
        AchievementData.AllAchievements.Where(a => !_state.UnlockedAchievements.Contains(a.Id));

    // === GENERATORS ===
    public double GetGeneratorCost(string generatorId)
    {
        var generator = GeneratorData.GetById(generatorId);
        if (generator == null) return double.MaxValue;
        int owned = _state.GetGeneratorCount(generatorId);
        double cost = generator.GetCost(owned);
        if (_state.IlluminatiUpgrades.Contains("new_world_order_discount")) cost *= 0.10; // -90%
        if (_state.IlluminatiUpgrades.Contains("shadow_network")) cost *= 0.05; // -95%
        cost *= GetMatrixCostMultiplier();
        cost *= GetGeneratorUpgradeCostMultiplier(generatorId); // Generator-specific discount
        return cost;
    }

    public bool CanAffordGenerator(string generatorId) => _state.Evidence >= GetGeneratorCost(generatorId);

    public bool PurchaseGenerator(string generatorId)
    {
        if (!CanAffordGenerator(generatorId)) return false;
        _state.Evidence -= GetGeneratorCost(generatorId);
        _state.AddGenerator(generatorId);
        CheckAchievements();
        OnTick?.Invoke();
        return true;
    }

    public int GetMaxAffordable(string generatorId)
    {
        var generator = GeneratorData.GetById(generatorId);
        if (generator == null) return 0;

        int owned = _state.GetGeneratorCount(generatorId);
        double available = _state.Evidence;
        int count = 0;

        double genUpgradeCostMult = GetGeneratorUpgradeCostMultiplier(generatorId);
        while (count < 1000)
        {
            double cost = generator.GetCost(owned + count);
            if (_state.IlluminatiUpgrades.Contains("new_world_order_discount")) cost *= 0.10; // -90%
            if (_state.IlluminatiUpgrades.Contains("shadow_network")) cost *= 0.05; // -95%
            cost *= genUpgradeCostMult;
            if (available >= cost) { available -= cost; count++; }
            else break;
        }
        return count;
    }

    public bool PurchaseMaxGenerators(string generatorId)
    {
        int max = GetMaxAffordable(generatorId);
        if (max == 0) return false;
        for (int i = 0; i < max; i++)
            if (!PurchaseGenerator(generatorId)) break;
        return true;
    }

    // === CRITICAL HITS ===
    public double GetCriticalMultiplier()
    {
        double baseMultiplier = 5.0 + _random.NextDouble() * 5.0; // 5x to 10x
        if (_state.UnlockedSkills.Contains("deadly_precision")) baseMultiplier = 10.0 + _random.NextDouble() * 5.0; // 10x to 15x
        baseMultiplier *= GetGeneratorUpgradeGlobalCritDamage();
        return baseMultiplier;
    }

    // === SKILL TREE ===
    public int GetTotalSkillPoints()
    {
        // 1 per 10 achievements + 1 per prestige
        int fromAchievements = _state.UnlockedAchievements.Count / 10;
        int fromPrestiges = _state.TimesAscended;
        return fromAchievements + fromPrestiges;
    }

    public int GetAvailableSkillPoints()
    {
        return GetTotalSkillPoints() - _state.UnlockedSkills.Count;
    }

    public bool CanUnlockSkill(string skillId)
    {
        var skill = SkillTreeData.GetById(skillId);
        if (skill == null || _state.UnlockedSkills.Contains(skillId)) return false;
        if (GetAvailableSkillPoints() < skill.SkillPointCost) return false;

        // Check prerequisite
        if (skill.RequiredSkillId != null && !_state.UnlockedSkills.Contains(skill.RequiredSkillId))
            return false;

        return true;
    }

    public bool UnlockSkill(string skillId)
    {
        if (!CanUnlockSkill(skillId)) return false;
        _state.UnlockedSkills.Add(skillId);
        OnTick?.Invoke();
        return true;
    }

    public IEnumerable<Skill> GetAvailableSkills()
    {
        return SkillTreeData.AllSkills.Where(s =>
            !_state.UnlockedSkills.Contains(s.Id) &&
            (s.RequiredSkillId == null || _state.UnlockedSkills.Contains(s.RequiredSkillId)));
    }

    public double GetSkillBelieverMultiplier()
    {
        double multiplier = 1.0;
        if (_state.UnlockedSkills.Contains("charisma")) multiplier *= 1.10;
        if (_state.UnlockedSkills.Contains("persuasion")) multiplier *= 1.15;
        if (_state.UnlockedSkills.Contains("cult_of_personality")) multiplier *= 1.50;
        if (_state.UnlockedSkills.Contains("mind_control")) multiplier *= 2.0;
        return multiplier;
    }

    public double GetSkillQuestSuccessBonus()
    {
        double bonus = 0;
        if (_state.UnlockedSkills.Contains("viral_marketing")) bonus += 0.10;
        return bonus;
    }

    public double GetIlluminatiQuestSuccessBonus()
    {
        double bonus = 0;
        if (_state.IlluminatiUpgrades.Contains("omniscient_vision")) bonus += 0.80; // +80%
        if (_state.IlluminatiUpgrades.Contains("probability_control")) bonus += 1.0; // Always succeed
        return bonus;
    }

    public bool SkillQuestsNeverFail() => _state.UnlockedSkills.Contains("mind_control");

    public double GetSkillComboFillBonus()
    {
        if (_state.UnlockedSkills.Contains("combo_master")) return 1.25;
        return 1.0;
    }

    public double GetSkillCriticalChanceBonus()
    {
        double bonus = 0;
        if (_state.UnlockedSkills.Contains("precision_clicking")) bonus += 0.05;
        if (_state.UnlockedSkills.Contains("deadly_precision")) bonus += 0.10;
        return bonus;
    }

    // === PRESTIGE (ILLUMINATI ASCENSION) ===
    public int GetTokensFromPrestige()
    {
        return PrestigeData.CalculateTokensEarned(_state.TotalEvidenceEarned);
    }

    public bool PerformPrestige()
    {
        if (!CanPrestige()) return false;

        int tokensEarned = GetTokensFromPrestige();
        _state.IlluminatiTokens += tokensEarned;
        _state.TotalIlluminatiTokensEarned += tokensEarned;
        _state.TimesAscended++;

        // Reset progress but keep permanent upgrades
        // Ascensions always reset to zero - multipliers make the difference
        _state.Evidence = 0;
        _state.TotalEvidenceEarned = 0;
        _state.Believers = 0;
        _state.BusyBelievers = 0;
        _state.Generators.Clear();
        _state.PurchasedUpgrades.Clear();
        _state.ProvenConspiracies.Clear();
        _state.ActiveQuests.Clear();
        _state.ComboMeter = 0;
        _state.ComboClicks = 0;

        // Also reset tinfoil and tinfoil upgrades
        _state.Tinfoil = 0;
        _state.TinfoilShopPurchases.Clear();

        // Keep: IlluminatiTokens, IlluminatiUpgrades, UnlockedSkills, Achievements

        _prestigeNotified = false;
        OnPrestigeComplete?.Invoke();
        OnTick?.Invoke();
        return true;
    }

    public bool CanAffordIlluminatiUpgrade(string upgradeId)
    {
        var upgrade = PrestigeData.GetById(upgradeId);
        if (upgrade == null || _state.IlluminatiUpgrades.Contains(upgradeId)) return false;
        return _state.IlluminatiTokens >= upgrade.TokenCost;
    }

    public bool PurchaseIlluminatiUpgrade(string upgradeId)
    {
        if (!CanAffordIlluminatiUpgrade(upgradeId)) return false;
        var upgrade = PrestigeData.GetById(upgradeId);
        if (upgrade == null) return false;

        _state.IlluminatiTokens -= upgrade.TokenCost;
        _state.IlluminatiUpgrades.Add(upgradeId);
        OnTick?.Invoke();
        return true;
    }

    public IEnumerable<IlluminatiUpgrade> GetAvailableIlluminatiUpgrades()
    {
        return PrestigeData.IlluminatiUpgrades.Where(u => !_state.IlluminatiUpgrades.Contains(u.Id));
    }

    public IEnumerable<IlluminatiUpgrade> GetPurchasedIlluminatiUpgrades()
    {
        return PrestigeData.IlluminatiUpgrades.Where(u => _state.IlluminatiUpgrades.Contains(u.Id));
    }

    // === DAILY CHALLENGES ===
    private void CheckDailyChallenges()
    {
        // Reset daily challenges if it's a new day
        if (_state.LastDailyChallengeDate.Date != DateTime.Today)
        {
            GenerateDailyChallenges();
        }

        // Update progress for each stored challenge
        foreach (var challenge in _state.DailyChallenges)
        {
            if (challenge.Completed) continue;

            double currentProgress = challenge.Type switch
            {
                ChallengeType.ClickCount => _state.TodayClicks,
                ChallengeType.CompleteQuests => _state.TodayQuestsCompleted,
                ChallengeType.CriticalHits => _state.TodayCriticalHits,
                ChallengeType.ComboCount => _state.TodayCombos,
                _ => 0
            };

            challenge.Progress = currentProgress;
            if (currentProgress >= challenge.Target && !challenge.Completed)
            {
                challenge.Completed = true;
                OnDailyChallengeComplete?.Invoke(challenge);
            }
        }
    }

    private void GenerateDailyChallenges()
    {
        _state.LastDailyChallengeDate = DateTime.Today;
        _state.TodayClicks = 0;
        _state.TodayEvidence = 0;
        _state.TodayQuestsCompleted = 0;
        _state.TodayCriticalHits = 0;
        _state.TodayCombos = 0;

        // Generate challenges using date seed for consistency
        _state.DailyChallenges = DailyChallengeData.GenerateDailyChallenges(DateTime.Today);
    }

    public List<StoredChallenge> GetDailyChallenges()
    {
        // Ensure challenges exist
        if (_state.DailyChallenges.Count == 0 && _state.LastDailyChallengeDate.Date != DateTime.Today)
        {
            GenerateDailyChallenges();
        }
        return _state.DailyChallenges;
    }

    public bool ClaimDailyChallenge(string challengeId)
    {
        var challenge = _state.DailyChallenges.FirstOrDefault(c => c.Id == challengeId);
        if (challenge == null || !challenge.Completed || challenge.Claimed) return false;

        challenge.Claimed = true;
        _state.Tinfoil += challenge.TinfoilReward;
        OnTick?.Invoke();
        return true;
    }

    // === MATRIX PRESTIGE (2nd Layer) ===
    public bool CanBreakMatrix()
    {
        return MatrixData.CanBreakMatrix(_state.TimesAscended, _state.TotalIlluminatiTokensEarned);
    }

    public int GetGlitchTokensFromMatrix()
    {
        // Calculate total Illuminati tokens ever spent on upgrades
        int tokensSpent = 0;
        foreach (var upgradeId in _state.IlluminatiUpgrades)
        {
            var upgrade = PrestigeData.GetById(upgradeId);
            if (upgrade != null) tokensSpent += upgrade.TokenCost;
        }
        return MatrixData.CalculateGlitchTokensEarned(tokensSpent + _state.IlluminatiTokens);
    }

    public bool BreakMatrix()
    {
        if (!CanBreakMatrix()) return false;

        int glitchTokensEarned = GetGlitchTokensFromMatrix();
        _state.GlitchTokens += glitchTokensEarned;
        _state.TimesMatrixBroken++;

        // Full reset including Illuminati tokens and upgrades
        _state.Evidence = 0;
        _state.TotalEvidenceEarned = 0;
        _state.Believers = 0;
        _state.BusyBelievers = 0;
        _state.Generators.Clear();
        _state.PurchasedUpgrades.Clear();
        _state.ProvenConspiracies.Clear();
        _state.ActiveQuests.Clear();
        _state.ComboMeter = 0;
        _state.ComboClicks = 0;
        _state.Tinfoil = 0;
        _state.TinfoilShopPurchases.Clear();
        _state.IlluminatiTokens = 0;
        _state.IlluminatiUpgrades.Clear();
        _state.TimesAscended = 0;
        _state.TotalIlluminatiTokensEarned = 0;

        // Keep: GlitchTokens, MatrixUpgrades, UnlockedSkills, Achievements

        _prestigeNotified = false;
        OnPrestigeComplete?.Invoke();
        OnTick?.Invoke();
        return true;
    }

    public bool CanAffordMatrixUpgrade(string upgradeId)
    {
        var upgrade = MatrixData.GetById(upgradeId);
        if (upgrade == null || _state.MatrixUpgrades.Contains(upgradeId)) return false;
        return _state.GlitchTokens >= upgrade.GlitchCost;
    }

    public bool PurchaseMatrixUpgrade(string upgradeId)
    {
        if (!CanAffordMatrixUpgrade(upgradeId)) return false;
        var upgrade = MatrixData.GetById(upgradeId);
        if (upgrade == null) return false;

        _state.GlitchTokens -= upgrade.GlitchCost;
        _state.MatrixUpgrades.Add(upgradeId);
        OnTick?.Invoke();
        return true;
    }

    public IEnumerable<MatrixUpgrade> GetAvailableMatrixUpgrades()
    {
        return MatrixData.MatrixUpgrades.Where(u => !_state.MatrixUpgrades.Contains(u.Id));
    }

    public IEnumerable<MatrixUpgrade> GetPurchasedMatrixUpgrades()
    {
        return MatrixData.MatrixUpgrades.Where(u => _state.MatrixUpgrades.Contains(u.Id));
    }

    public double GetMatrixEpsMultiplier()
    {
        double multiplier = 1.0;
        if (_state.MatrixUpgrades.Contains("reality_warp")) multiplier *= 3.0;
        if (_state.MatrixUpgrades.Contains("the_one")) multiplier *= 10.0;
        return multiplier;
    }

    public double GetMatrixClickMultiplier()
    {
        double multiplier = 1.0;
        if (_state.MatrixUpgrades.Contains("bullet_time")) multiplier *= 5.0;
        return multiplier;
    }

    public double GetMatrixBelieverMultiplier()
    {
        double multiplier = 1.0;
        if (_state.MatrixUpgrades.Contains("source_code_access")) multiplier *= 2.0;
        return multiplier;
    }

    public double GetMatrixCostMultiplier()
    {
        double multiplier = 1.0;
        if (_state.MatrixUpgrades.Contains("architect_meeting")) multiplier *= 0.5;
        return multiplier;
    }

    public double GetMatrixQuestSuccessBonus()
    {
        double bonus = 0;
        if (_state.MatrixUpgrades.Contains("agent_infiltration")) bonus += 1.0;
        return bonus;
    }

    public double GetMatrixEpsToClickBonus()
    {
        // neo_clicking adds extra 1% EPS to clicks (total 2%)
        if (_state.MatrixUpgrades.Contains("neo_clicking")) return 0.01;
        return 0;
    }

    // === CHALLENGE MODES ===
    public bool IsInChallenge => _state.ActiveChallengeId != null;

    public ChallengeMode? GetActiveChallenge()
    {
        return _state.ActiveChallengeId != null ? ChallengeModeData.GetById(_state.ActiveChallengeId) : null;
    }

    public bool StartChallenge(string challengeId)
    {
        if (IsInChallenge) return false;
        var challenge = ChallengeModeData.GetById(challengeId);
        if (challenge == null || _state.CompletedChallenges.Contains(challengeId)) return false;

        // Reset game state for challenge (similar to prestige but keeps nothing)
        _state.Evidence = 0;
        _state.TotalEvidenceEarned = 0;
        _state.Believers = 0;
        _state.BusyBelievers = 0;
        _state.Generators.Clear();
        _state.PurchasedUpgrades.Clear();
        _state.ProvenConspiracies.Clear();
        _state.ActiveQuests.Clear();
        _state.Tinfoil = 0;
        _state.TinfoilShopPurchases.Clear();
        _state.IlluminatiTokens = 0;
        _state.IlluminatiUpgrades.Clear();
        _state.ComboMeter = 0;
        _state.ComboClicks = 0;

        // Set challenge state
        _state.ActiveChallengeId = challengeId;
        _state.ChallengeStartTime = DateTime.Now;
        _state.ChallengeProgress = 0;
        _state.ChallengeClickCount = 0;
        _state.ChallengeHighRiskQuestsCompleted = 0;

        OnTick?.Invoke();
        return true;
    }

    public void AbandonChallenge()
    {
        _state.ActiveChallengeId = null;
        _state.ChallengeProgress = 0;
        OnTick?.Invoke();
    }

    public (bool completed, double progress, double timeRemaining) GetChallengeStatus()
    {
        var challenge = GetActiveChallenge();
        if (challenge == null) return (false, 0, 0);

        double elapsed = (DateTime.Now - _state.ChallengeStartTime).TotalSeconds;
        double timeRemaining = challenge.TimeLimit > 0 ? Math.Max(0, challenge.TimeLimit - elapsed) : -1;

        double progress = challenge.Type switch
        {
            ChallengeModeType.Speedrun => _state.TotalEvidenceEarned / challenge.TargetValue,
            ChallengeModeType.NoClick => _state.TotalEvidenceEarned / challenge.TargetValue,
            ChallengeModeType.Minimalist => _state.TotalEvidenceEarned / challenge.TargetValue,
            ChallengeModeType.NoPrestige => _state.TotalEvidenceEarned / challenge.TargetValue,
            ChallengeModeType.RiskyBusiness => _state.ChallengeHighRiskQuestsCompleted / challenge.TargetValue,
            ChallengeModeType.ClickMaster => _state.ChallengeClickCount / challenge.TargetValue,
            _ => 0
        };

        _state.ChallengeProgress = progress;
        bool completed = progress >= 1.0 && (challenge.TimeLimit <= 0 || timeRemaining > 0);

        return (completed, Math.Min(progress, 1.0), timeRemaining);
    }

    public bool CompleteChallenge()
    {
        var challenge = GetActiveChallenge();
        if (challenge == null) return false;

        var (completed, _, _) = GetChallengeStatus();
        if (!completed) return false;

        // Grant rewards
        _state.Tinfoil += challenge.TinfoilReward;
        _state.IlluminatiTokens += challenge.IlluminatiTokenReward;
        _state.CompletedChallenges.Add(challenge.Id);

        // End challenge
        _state.ActiveChallengeId = null;
        _state.ChallengeProgress = 0;

        OnTick?.Invoke();
        return true;
    }

    public bool IsChallengeViolation(ChallengeModeType checkType)
    {
        var challenge = GetActiveChallenge();
        if (challenge == null) return false;

        return challenge.Type switch
        {
            ChallengeModeType.NoClick when checkType == ChallengeModeType.NoClick => true,
            ChallengeModeType.Minimalist when checkType == ChallengeModeType.Minimalist => true,
            _ => false
        };
    }

    public void RecordChallengeClick()
    {
        if (IsInChallenge)
        {
            _state.ChallengeClickCount++;
        }
    }

    public void RecordChallengeHighRiskQuest()
    {
        if (IsInChallenge)
        {
            _state.ChallengeHighRiskQuestsCompleted++;
        }
    }

    // === GENERATOR UPGRADES ===
    public double GetGeneratorUpgradeProductionMultiplier(string generatorId)
    {
        double multiplier = 1.0;
        foreach (var upgradeId in _state.GeneratorUpgrades)
        {
            var upgrade = GeneratorUpgradeData.GetById(upgradeId);
            if (upgrade != null && upgrade.GeneratorId == generatorId && upgrade.Type == GeneratorUpgradeType.ProductionMultiplier)
                multiplier *= upgrade.Value;
        }
        return multiplier;
    }

    public double GetGeneratorUpgradeCostMultiplier(string generatorId)
    {
        double multiplier = 1.0;
        foreach (var upgradeId in _state.GeneratorUpgrades)
        {
            var upgrade = GeneratorUpgradeData.GetById(upgradeId);
            if (upgrade != null && upgrade.GeneratorId == generatorId && upgrade.Type == GeneratorUpgradeType.CostReduction)
                multiplier *= upgrade.Value;
        }
        return multiplier;
    }

    public double GetGeneratorUpgradeBelieverMultiplier(string generatorId)
    {
        double multiplier = 1.0;
        foreach (var upgradeId in _state.GeneratorUpgrades)
        {
            var upgrade = GeneratorUpgradeData.GetById(upgradeId);
            if (upgrade != null && upgrade.GeneratorId == generatorId && upgrade.Type == GeneratorUpgradeType.BelieverBonus)
                multiplier *= upgrade.Value;
        }
        return multiplier;
    }

    public double GetGeneratorUpgradeGlobalClickMultiplier()
    {
        double multiplier = 1.0;
        foreach (var upgradeId in _state.GeneratorUpgrades)
        {
            var upgrade = GeneratorUpgradeData.GetById(upgradeId);
            if (upgrade?.Type == GeneratorUpgradeType.GlobalClickPower)
                multiplier *= upgrade.Value;
        }
        return multiplier;
    }

    public double GetGeneratorUpgradeGlobalQuestSpeed()
    {
        double multiplier = 1.0;
        foreach (var upgradeId in _state.GeneratorUpgrades)
        {
            var upgrade = GeneratorUpgradeData.GetById(upgradeId);
            if (upgrade?.Type == GeneratorUpgradeType.GlobalQuestSpeed)
                multiplier *= upgrade.Value;
        }
        return multiplier;
    }

    public double GetGeneratorUpgradeGlobalTinfoilMultiplier()
    {
        double multiplier = 1.0;
        foreach (var upgradeId in _state.GeneratorUpgrades)
        {
            var upgrade = GeneratorUpgradeData.GetById(upgradeId);
            if (upgrade?.Type == GeneratorUpgradeType.GlobalTinfoilGain)
                multiplier *= upgrade.Value;
        }
        return multiplier;
    }

    public double GetGeneratorUpgradeGlobalCritChance()
    {
        double bonus = 0;
        foreach (var upgradeId in _state.GeneratorUpgrades)
        {
            var upgrade = GeneratorUpgradeData.GetById(upgradeId);
            if (upgrade?.Type == GeneratorUpgradeType.GlobalCritChance)
                bonus += upgrade.Value;
        }
        return bonus;
    }

    public double GetGeneratorUpgradeGlobalCritDamage()
    {
        double multiplier = 1.0;
        foreach (var upgradeId in _state.GeneratorUpgrades)
        {
            var upgrade = GeneratorUpgradeData.GetById(upgradeId);
            if (upgrade?.Type == GeneratorUpgradeType.GlobalCritDamage)
                multiplier *= upgrade.Value;
        }
        return multiplier;
    }

    public double GetGeneratorUpgradeGlobalGoldenEyeFrequency()
    {
        double multiplier = 1.0;
        foreach (var upgradeId in _state.GeneratorUpgrades)
        {
            var upgrade = GeneratorUpgradeData.GetById(upgradeId);
            if (upgrade?.Type == GeneratorUpgradeType.GlobalGoldenEye)
                multiplier *= upgrade.Value;
        }
        return multiplier;
    }

    public double GetGeneratorUpgradeGlobalBelieverMultiplier()
    {
        double multiplier = 1.0;
        foreach (var upgradeId in _state.GeneratorUpgrades)
        {
            var upgrade = GeneratorUpgradeData.GetById(upgradeId);
            if (upgrade?.Type == GeneratorUpgradeType.GlobalBelieverGain)
                multiplier *= upgrade.Value;
        }
        return multiplier;
    }

    public double GetGeneratorUpgradeGlobalEpsMultiplier()
    {
        double multiplier = 1.0;
        foreach (var upgradeId in _state.GeneratorUpgrades)
        {
            var upgrade = GeneratorUpgradeData.GetById(upgradeId);
            if (upgrade?.Type == GeneratorUpgradeType.GlobalEpsMultiplier)
                multiplier *= upgrade.Value;
        }
        return multiplier;
    }

    public IEnumerable<GeneratorUpgrade> GetAvailableGeneratorUpgrades(string generatorId)
    {
        int level = _state.GetGeneratorCount(generatorId);
        return GeneratorUpgradeData.GetAvailableUpgrades(generatorId, level, _state.GeneratorUpgrades);
    }

    public IEnumerable<GeneratorUpgrade> GetPurchasedGeneratorUpgrades(string generatorId)
    {
        return GeneratorUpgradeData.GetPurchasedUpgrades(generatorId, _state.GeneratorUpgrades);
    }

    public double GetGeneratorUpgradeCost(string upgradeId)
    {
        var upgrade = GeneratorUpgradeData.GetById(upgradeId);
        if (upgrade == null) return double.MaxValue;

        var generator = GeneratorData.GetById(upgrade.GeneratorId);
        if (generator == null) return double.MaxValue;

        // Cost is 3x the generator's production value at the unlock level
        // This accounts for buying multiple levels at once - cost scales with production at milestone
        double baseCost = generator.GetProduction(upgrade.UnlockLevel) * 3.0;

        // Apply cost reductions from prestige
        if (_state.IlluminatiUpgrades.Contains("new_world_order_discount")) baseCost *= 0.10;
        if (_state.IlluminatiUpgrades.Contains("shadow_network")) baseCost *= 0.05;
        baseCost *= GetMatrixCostMultiplier();

        return baseCost;
    }

    public bool CanAffordGeneratorUpgrade(string upgradeId)
    {
        return _state.Evidence >= GetGeneratorUpgradeCost(upgradeId);
    }

    public bool PurchaseGeneratorUpgrade(string upgradeId)
    {
        var upgrade = GeneratorUpgradeData.GetById(upgradeId);
        if (upgrade == null) return false;
        if (_state.GeneratorUpgrades.Contains(upgradeId)) return false;

        int level = _state.GetGeneratorCount(upgrade.GeneratorId);
        if (level < upgrade.UnlockLevel) return false;

        double cost = GetGeneratorUpgradeCost(upgradeId);
        if (_state.Evidence < cost) return false;

        _state.Evidence -= cost;
        _state.GeneratorUpgrades.Add(upgradeId);
        OnTick?.Invoke();
        return true;
    }
}
