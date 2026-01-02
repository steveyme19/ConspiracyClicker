using System.Windows.Threading;
using ConspiracyClicker.Data;
using ConspiracyClicker.Models;

namespace ConspiracyClicker.Core;

public class GameEngine
{
    private readonly GameState _state;
    private readonly SaveManager _saveManager;
    private readonly DispatcherTimer _gameLoop;
    private readonly DispatcherTimer _autoSaveTimer;
    private readonly Random _random = new();

    private const int TICK_RATE_MS = 100;
    private const int AUTO_SAVE_INTERVAL_MS = 30000;
    private const double COMBO_DECAY_RATE = 0.15;
    private const double COMBO_FILL_PER_CLICK = 0.08;
    private const int COMBO_BURST_CLICKS = 10; // Combo gives value of 10 clicks
    private const double CRITICAL_MULTIPLIER = 7.5; // Average of 5x-10x
    private const double PRESTIGE_THRESHOLD = 1_000_000_000_000; // 1 trillion

    private double _autoClickAccumulator = 0;

    public event Action? OnTick;
    public event Action<Achievement>? OnAchievementUnlocked;
    public event Action<string>? OnFlavorMessage;
    public event Action<double>? OnComboBurst;
    public event Action<double, bool>? OnClickProcessed; // clickPower, isCritical
    public event Action<string, bool, double, int>? OnQuestComplete;
    public event Action? OnGoldenEyeStart;
    public event Action? OnGoldenEyeEnd;
    public event Action? OnWhistleBlowerSpawn;
    public event Action? OnPrestigeAvailable;
    public event Action<StoredChallenge>? OnDailyChallengeComplete;
    public event Action? OnPrestigeComplete;

    private bool _prestigeNotified = false;
    private DateTime _lastDailyCheck = DateTime.MinValue;

    public GameState State => _state;

    public GameEngine()
    {
        _saveManager = new SaveManager();
        _state = _saveManager.Load();

        _gameLoop = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(TICK_RATE_MS) };
        _gameLoop.Tick += GameLoop_Tick;

        _autoSaveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(AUTO_SAVE_INTERVAL_MS) };
        _autoSaveTimer.Tick += (s, e) => Save();
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
        double evidenceThisTick = eps * (TICK_RATE_MS / 1000.0);

        _state.Evidence += evidenceThisTick;
        _state.TotalEvidenceEarned += evidenceThisTick;
        _state.TotalPlayTimeSeconds += TICK_RATE_MS / 1000.0;

        UpdateBelievers();
        UpdateComboMeter();
        ProcessAutoClicks();
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
                totalBelievers += generator.BelieverBonus * count;
        }

        // Apply Tinfoil Shop believer multiplier at the end
        totalBelievers *= GetTinfoilBelieverMultiplier();

        // Apply Skill Tree believer multiplier
        totalBelievers *= GetSkillBelieverMultiplier();

        // Apply Prestige believer bonus
        if (_state.IlluminatiUpgrades.Contains("believer_magnetism")) totalBelievers *= 1.25;

        _state.Believers = totalBelievers;
    }

    private void UpdateComboMeter()
    {
        double timeSinceClick = (DateTime.Now - _state.LastClickTime).TotalSeconds;
        if (timeSinceClick > 0.5)
        {
            _state.ComboMeter -= COMBO_DECAY_RATE * (TICK_RATE_MS / 1000.0);
            if (_state.ComboMeter < 0) _state.ComboMeter = 0;
            if (timeSinceClick > 2) _state.ComboClicks = 0;
        }
    }

    private void ProcessAutoClicks()
    {
        double autoClickRate = GetAutoClickRate();
        if (autoClickRate <= 0) return;

        _autoClickAccumulator += autoClickRate * (TICK_RATE_MS / 1000.0);

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
            _state.ComboMeter += COMBO_FILL_PER_CLICK;
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
        double burstAmount = CalculateClickPower() * COMBO_BURST_CLICKS;

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
        double epsBonus = 0.0;

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
        if (_state.IlluminatiUpgrades.Contains("secret_handshake")) multiplier *= 1.10;
        if (_state.IlluminatiUpgrades.Contains("all_seeing_investment"))
            multiplier *= 1.0 + (_state.IlluminatiTokens * 0.01);

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

        // Prestige bonuses
        if (_state.IlluminatiUpgrades.Contains("pyramid_scheme")) multiplier *= 1.05;
        if (_state.IlluminatiUpgrades.Contains("reptilian_dna")) multiplier *= 2.0;

        // Conspiracy bonus
        if (_state.ProvenConspiracies.Contains("you_are_conspiracy")) multiplier *= 2.0;

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
        return rate;
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
        return chance;
    }

    public IEnumerable<TinfoilUpgrade> GetAvailableTinfoilUpgrades()
    {
        return TinfoilShopData.AllUpgrades.Where(u => !_state.TinfoilShopPurchases.Contains(u.Id));
    }

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

        var activeQuest = new ActiveQuest
        {
            QuestId = questId,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddSeconds(quest.DurationSeconds),
            BelieversSent = quest.BelieversRequired
        };

        _state.ActiveQuests.Add(activeQuest);
        _state.BusyBelievers += quest.BelieversRequired;

        OnTick?.Invoke();
        return true;
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

            // Apply quest success bonus from Tinfoil Shop and Skills
            double adjustedSuccessChance = quest.SuccessChance + GetTinfoilQuestSuccessBonus() + GetSkillQuestSuccessBonus();
            adjustedSuccessChance = Math.Min(adjustedSuccessChance, 0.95);
            bool success = SkillQuestsNeverFail() || _random.NextDouble() < adjustedSuccessChance;
            double evidenceReward = 0;
            int tinfoilReward = 0;

            if (success)
            {
                evidenceReward = quest.EvidenceReward + (CalculateEvidencePerSecond() * quest.EvidenceMultiplier);

                // Apply prestige and skill quest reward bonuses
                double rewardMultiplier = 1.0;
                if (_state.IlluminatiUpgrades.Contains("moon_base_alpha")) rewardMultiplier *= 1.50;
                if (_state.UnlockedSkills.Contains("cult_of_personality")) rewardMultiplier *= 1.25;
                evidenceReward *= rewardMultiplier;

                tinfoilReward = quest.TinfoilReward;
                _state.Evidence += evidenceReward;
                _state.TotalEvidenceEarned += evidenceReward;
                _state.Tinfoil += tinfoilReward;
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
        if (_random.NextDouble() > 0.001) return;
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
        if (!_prestigeNotified && _state.TotalEvidenceEarned >= PRESTIGE_THRESHOLD)
        {
            _prestigeNotified = true;
            OnPrestigeAvailable?.Invoke();
        }
    }

    public bool CanPrestige() => _state.TotalEvidenceEarned >= PRESTIGE_THRESHOLD;

    // === BELIEVER INFO ===
    public Dictionary<string, int> GetBelieverBreakdown()
    {
        var breakdown = new Dictionary<string, int>();
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
        if (_state.IlluminatiUpgrades.Contains("new_world_order_discount")) cost *= 0.9;
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

        while (count < 1000)
        {
            double cost = generator.GetCost(owned + count);
            if (_state.IlluminatiUpgrades.Contains("new_world_order_discount")) cost *= 0.9;
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
        double startingEvidence = _state.IlluminatiUpgrades.Contains("starting_evidence") ? 1_000_000 : 0;

        _state.Evidence = startingEvidence;
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
}
