using System.Diagnostics;
using ConspiracyClicker.Core;
using ConspiracyClicker.Data;
using ConspiracyClicker.Models;

namespace ConspiracyClicker.Simulator;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║          CONSPIRACY CLICKER - BALANCE SIMULATOR              ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Parse command line args
        var config = SimulationConfig.FromArgs(args);

        Console.WriteLine($"Configuration:");
        Console.WriteLine($"  Speed: {config.SpeedMultiplier}x | CPS: {config.TargetClicksPerSecond} | Strategy: {config.Strategy}");
        Console.WriteLine($"  Ascension: {config.Ascension} | Max Time: {config.MaxRealTimeMinutes}min | Interval: {config.ProgressIntervalMinutes}min");
        Console.WriteLine($"  Target: {(config.SimulatedHours > 0 ? $"{config.SimulatedHours}h" : "unlimited")} | Stop at: {config.StopAtConspiracy ?? "all 25"}");
        Console.WriteLine();

        var simulator = new GameSimulator(config);
        simulator.Run();

        Console.WriteLine();
        Console.WriteLine("Simulation complete!");

        // Only wait for key if running interactively
        if (!Console.IsInputRedirected)
        {
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}

public class SimulationConfig
{
    public double SpeedMultiplier { get; set; } = 10000; // 10000x game speed for fast simulation
    public double TargetClicksPerSecond { get; set; } = 5; // Human-like CPS
    public double ClickVariance { get; set; } = 0.3; // 30% variance in click timing
    public BuyStrategy Strategy { get; set; } = BuyStrategy.Optimal;
    public int MaxRealTimeMinutes { get; set; } = 5; // Max 5 minutes real time
    public string? StopAtConspiracy { get; set; } = null; // Stop when this conspiracy is proven
    public bool Verbose { get; set; } = false;
    public AscensionMode Ascension { get; set; } = AscensionMode.Optimal; // Default to optimal ascension
    public double SimulatedHours { get; set; } = 0; // If > 0, stop after this many simulated hours
    public int ProgressIntervalMinutes { get; set; } = 15; // Report progress every N simulated minutes

    public static SimulationConfig FromArgs(string[] args)
    {
        var config = new SimulationConfig();

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--speed" when i + 1 < args.Length:
                    config.SpeedMultiplier = double.Parse(args[++i]);
                    break;
                case "--cps" when i + 1 < args.Length:
                    config.TargetClicksPerSecond = double.Parse(args[++i]);
                    break;
                case "--strategy" when i + 1 < args.Length:
                    config.Strategy = Enum.Parse<BuyStrategy>(args[++i], true);
                    break;
                case "--maxtime" when i + 1 < args.Length:
                    config.MaxRealTimeMinutes = int.Parse(args[++i]);
                    break;
                case "--stopat" when i + 1 < args.Length:
                    config.StopAtConspiracy = args[++i];
                    break;
                case "--verbose":
                case "-v":
                    config.Verbose = true;
                    break;
                case "--ascension" when i + 1 < args.Length:
                    config.Ascension = Enum.Parse<AscensionMode>(args[++i], true);
                    break;
                case "--hours" when i + 1 < args.Length:
                    config.SimulatedHours = double.Parse(args[++i]);
                    break;
                case "--interval" when i + 1 < args.Length:
                    config.ProgressIntervalMinutes = int.Parse(args[++i]);
                    break;
                case "--help":
                case "-h":
                    PrintHelp();
                    Environment.Exit(0);
                    break;
            }
        }

        return config;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Usage: ConspiracyClicker.Simulator [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --speed <multiplier>   Game speed multiplier (default: 1000)");
        Console.WriteLine("  --cps <rate>           Target clicks per second (default: 5)");
        Console.WriteLine("  --strategy <name>      Buy strategy: Balanced, GeneratorsFirst, UpgradesFirst, Optimal (default: Balanced)");
        Console.WriteLine("  --maxtime <minutes>    Max real-time minutes to run (default: 10)");
        Console.WriteLine("  --hours <hours>        Stop after this many simulated hours (default: unlimited)");
        Console.WriteLine("  --interval <minutes>   Progress report interval in sim minutes (default: 5)");
        Console.WriteLine("  --ascension <mode>     Ascension mode: None, Optimal, Early, Late (default: None)");
        Console.WriteLine("  --stopat <conspiracy>  Stop when specific conspiracy is proven");
        Console.WriteLine("  --verbose, -v          Show detailed output");
        Console.WriteLine("  --help, -h             Show this help message");
    }
}

public enum BuyStrategy
{
    Balanced,        // Buy whatever is most cost-efficient
    GeneratorsFirst, // Prioritize generators over upgrades
    UpgradesFirst,   // Prioritize upgrades over generators
    Optimal          // Use payback time calculations
}

public enum AscensionMode
{
    None,           // Never ascend
    Optimal,        // Ascend when it would speed up progression
    Early,          // Ascend as soon as possible (1T evidence)
    Late            // Only ascend after hitting a wall
}

public class GameSimulator
{
    private readonly SimulationConfig _config;
    private readonly GameEngine _engine;
    private readonly MilestoneTracker _milestones;
    private readonly Random _random = new();

    private double _simulatedGameTime = 0; // In-game seconds
    private double _clickAccumulator = 0;
    private int _totalClicks = 0;
    private int _totalPurchases = 0;

    // Combo system
    private double _comboMeter = 0;
    private double _timeSinceLastClick = 0;
    private int _comboBursts = 0;

    // Random events
    private bool _goldenEyeActive = false;
    private double _goldenEyeTimeRemaining = 0;
    private int _goldenEyeCount = 0;
    private int _whistleBlowersCaught = 0;

    // Critical hits
    private int _criticalHits = 0;

    // Ascension tracking
    private int _timesAscended = 0;
    private int _totalTokensEarned = 0;
    private readonly List<(double time, int tokens)> _ascensionLog = new();
    private double _lastAscensionTime = 0;
    private double _timeAtLastPrestigeCheck = 0;
    private double _epsAtLastPrestigeCheck = 0;

    // Performance optimization
    private double _lastEps = 0;
    private int _stableEpsCount = 0;

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

    /// <summary>
    /// Determines optimal time step based on game state.
    /// When EPS is high and stable, we can skip many ticks.
    /// </summary>
    private double GetAdaptiveTimeStep(double baseTickSeconds)
    {
        double currentEps = _engine.CalculateEvidencePerSecond();
        var state = _engine.State;

        // Check if EPS is stable (within 20% of last check)
        if (_lastEps > 0 && Math.Abs(currentEps - _lastEps) / _lastEps < 0.2)
            _stableEpsCount++;
        else
            _stableEpsCount = 0;
        _lastEps = currentEps;

        // If EPS is stable and reasonable, we can fast-forward
        if (_stableEpsCount > 5 && currentEps > 100_000)
        {
            // Scale time step based on EPS magnitude
            // Higher EPS = bigger jumps (up to 300 seconds of game time per step)
            double epsMagnitude = Math.Log10(currentEps);
            double multiplier = Math.Min(300, Math.Pow(2, epsMagnitude - 5));
            return baseTickSeconds * multiplier;
        }

        return baseTickSeconds;
    }

    public GameSimulator(SimulationConfig config)
    {
        _config = config;
        _engine = new GameEngine();
        _milestones = new MilestoneTracker();
    }

    public void Run()
    {
        var stopwatch = Stopwatch.StartNew();
        var maxRealTimeMs = _config.MaxRealTimeMinutes * 60 * 1000;

        // Calculate tick timing
        double gameTickMs = 50; // Game ticks every 50ms
        double realTickMs = gameTickMs / _config.SpeedMultiplier;
        double ticksPerSecond = 1000.0 / gameTickMs;

        Console.WriteLine("Starting simulation...");
        Console.WriteLine("─────────────────────────────────────────────────────────────────");

        int lastReportedMinute = -1;
        const int BATCH_SIZE = 2000; // Process 2000 ticks before checking events (4x faster)
        double baseTickSeconds = gameTickMs / 1000.0;

        while (stopwatch.ElapsedMilliseconds < maxRealTimeMs)
        {
            // Get adaptive time step (larger when EPS is high and stable)
            double adaptiveStep = GetAdaptiveTimeStep(baseTickSeconds);

            // Batch process multiple ticks for speed
            for (int i = 0; i < BATCH_SIZE; i++)
            {
                SimulateTick(adaptiveStep);
                _simulatedGameTime += adaptiveStep;
            }

            // Normalize large numbers periodically
            var state = _engine.State;
            state.Evidence = NormalizeLargeNumber(state.Evidence);
            state.TotalEvidenceEarned = NormalizeLargeNumber(state.TotalEvidenceEarned);

            // Check milestones
            _milestones.Check(_engine.State, _simulatedGameTime);

            // Check for ascension opportunity
            if (_config.Ascension != AscensionMode.None)
            {
                CheckAndPerformAscension();
            }

            // Check simulated hours stop condition
            if (_config.SimulatedHours > 0 && _simulatedGameTime >= _config.SimulatedHours * 3600)
            {
                Console.WriteLine($"\n*** Reached {_config.SimulatedHours} simulated hours ***");
                break;
            }

            // Check stop condition
            if (_config.StopAtConspiracy != null &&
                _engine.State.ProvenConspiracies.Contains(_config.StopAtConspiracy))
            {
                Console.WriteLine($"\n*** Reached target conspiracy: {_config.StopAtConspiracy} ***");
                break;
            }

            // Progress report every N simulated minutes (configurable)
            int currentInterval = (int)(_simulatedGameTime / (60 * _config.ProgressIntervalMinutes));
            if (currentInterval > lastReportedMinute)
            {
                lastReportedMinute = currentInterval;
                PrintProgress();
            }

            // Small delay to prevent CPU spinning (only if running slower than real-time)
            if (realTickMs > 1)
            {
                Thread.Sleep((int)realTickMs);
            }
        }

        stopwatch.Stop();

        Console.WriteLine("─────────────────────────────────────────────────────────────────");
        Console.WriteLine();

        PrintFinalReport(stopwatch.Elapsed);
    }

    private void SimulateTick(double deltaSeconds)
    {
        var state = _engine.State;
        double eps = _engine.CalculateEvidencePerSecond();

        // FAST PATH: When EPS is extremely high, skip unnecessary processing
        bool fastMode = eps > 1_000_000_000_000; // > 1T EPS

        if (!fastMode)
        {
            // Update random events timing
            UpdateRandomEvents(deltaSeconds);

            // Simulate clicks with human-like variance
            double effectiveCps = _config.TargetClicksPerSecond *
                (1 + (_random.NextDouble() * 2 - 1) * _config.ClickVariance);

            _clickAccumulator += effectiveCps * deltaSeconds;
            _timeSinceLastClick += deltaSeconds;

            while (_clickAccumulator >= 1)
            {
                ProcessSimulatedClick();
                _clickAccumulator -= 1;
                _totalClicks++;
                _timeSinceLastClick = 0;
            }

            // Decay combo meter if no recent clicks
            if (_timeSinceLastClick > 0.5)
            {
                _comboMeter -= 0.15 * deltaSeconds;
                if (_comboMeter < 0) _comboMeter = 0;
            }

            // Check for random event spawns (after first conspiracy)
            CheckRandomEventSpawn();
        }

        // Always do these (core game progression)
        ExecuteBuyStrategy();
        TryProveConspiracies();

        // Add passive evidence (normalized for huge numbers)
        double evidenceGain = NormalizeLargeNumber(eps * deltaSeconds);
        state.Evidence = NormalizeLargeNumber(state.Evidence + evidenceGain);
        state.TotalEvidenceEarned = NormalizeLargeNumber(state.TotalEvidenceEarned + evidenceGain);
        state.TotalPlayTimeSeconds += deltaSeconds;

        // Update believers and quests less frequently in fast mode
        if (!fastMode || _random.NextDouble() < 0.1)
        {
            UpdateBelievers();
            TryStartQuests();
        }
        CheckQuestCompletion(deltaSeconds);
    }

    private void ProcessSimulatedClick()
    {
        var state = _engine.State;
        double clickPower = _engine.CalculateClickPower();

        // Critical hit check
        double critChance = _engine.GetCriticalChance();
        if (critChance > 0 && _random.NextDouble() < critChance)
        {
            clickPower *= 7.5; // Average crit multiplier (5-10x)
            _criticalHits++;
        }

        // Golden Eye bonus (5x clicks)
        if (_goldenEyeActive)
            clickPower *= 5;

        // Add evidence from click
        state.Evidence += clickPower;
        state.TotalEvidenceEarned += clickPower;

        // Combo system
        _comboMeter += 0.08; // COMBO_FILL_PER_CLICK
        if (_comboMeter >= 1.0)
        {
            TriggerComboBurst();
        }
    }

    private void TriggerComboBurst()
    {
        var state = _engine.State;
        double burstAmount = _engine.CalculateClickPower() * 10; // COMBO_BURST_CLICKS

        // Golden Eye also affects combo bursts (10x instead of 5x)
        if (_goldenEyeActive)
            burstAmount *= 10;

        state.Evidence += burstAmount;
        state.TotalEvidenceEarned += burstAmount;
        _comboMeter = 0;
        _comboBursts++;

        if (_config.Verbose)
            Console.WriteLine($"  [Combo] Burst! +{FormatNumber(burstAmount)} evidence");
    }

    private void UpdateRandomEvents(double deltaSeconds)
    {
        if (_goldenEyeActive)
        {
            _goldenEyeTimeRemaining -= deltaSeconds;
            if (_goldenEyeTimeRemaining <= 0)
            {
                _goldenEyeActive = false;
                if (_config.Verbose)
                    Console.WriteLine($"  [Event] Golden Eye ended");
            }
        }
    }

    private void CheckRandomEventSpawn()
    {
        var state = _engine.State;

        // Only spawn after first conspiracy
        if (state.ProvenConspiracies.Count == 0) return;

        // Don't spawn if event already active
        if (_goldenEyeActive) return;

        // 0.1% chance per tick (50ms tick = ~20 ticks/sec, so ~2% per second)
        if (_random.NextDouble() > 0.001) return;

        // 50% Golden Eye, 50% Whistle-blower
        if (_random.NextDouble() < 0.5)
        {
            // Golden Eye - 5x clicks for 10 seconds
            _goldenEyeActive = true;
            _goldenEyeTimeRemaining = 10;
            _goldenEyeCount++;
            if (_config.Verbose)
                Console.WriteLine($"  [Event] Golden Eye activated! (5x clicks for 10s)");
        }
        else
        {
            // Whistle-blower - instant reward (simulate immediate click)
            double bonus = _engine.CalculateEvidencePerSecond() * 120;
            state.Evidence += bonus;
            state.TotalEvidenceEarned += bonus;
            state.Tinfoil += 5;
            _whistleBlowersCaught++;
            if (_config.Verbose)
                Console.WriteLine($"  [Event] Whistle-blower caught! +{FormatNumber(bonus)} evidence +5 tinfoil");
        }
    }

    // Track simulated quest progress (questId -> remaining seconds)
    private readonly Dictionary<string, double> _questRemainingTime = new();

    private void TryStartQuests()
    {
        var state = _engine.State;

        // Try to start available quests (prioritize by reward)
        var availableQuests = QuestData.GetAvailable(state.AvailableBelievers)
            .Where(q => !_questRemainingTime.ContainsKey(q.Id))
            .Where(q => state.AvailableBelievers >= q.BelieversRequired)
            .OrderByDescending(q => q.EvidenceReward)
            .ToList();

        foreach (var quest in availableQuests)
        {
            if (state.AvailableBelievers >= quest.BelieversRequired)
            {
                // Track quest with simulated time instead of real DateTime
                _questRemainingTime[quest.Id] = quest.DurationSeconds;
                state.BusyBelievers += quest.BelieversRequired;

                if (_config.Verbose)
                    Console.WriteLine($"  [Quest] Started: {quest.Name} ({quest.DurationSeconds}s)");
            }
        }
    }

    private void CheckQuestCompletion(double deltaSeconds)
    {
        var state = _engine.State;
        var completedQuestIds = new List<string>();

        // Update remaining time for all active quests
        foreach (var questId in _questRemainingTime.Keys.ToList())
        {
            _questRemainingTime[questId] -= deltaSeconds;
            if (_questRemainingTime[questId] <= 0)
            {
                completedQuestIds.Add(questId);
            }
        }

        foreach (var questId in completedQuestIds)
        {
            var quest = QuestData.GetById(questId);
            if (quest == null) continue;

            _questRemainingTime.Remove(questId);
            state.BusyBelievers -= quest.BelieversRequired;

            // Simulate success/failure
            bool success = _random.NextDouble() < quest.SuccessChance;

            if (success)
            {
                double evidenceReward = quest.EvidenceReward + (_engine.CalculateEvidencePerSecond() * quest.EvidenceMultiplier);
                state.Evidence += evidenceReward;
                state.TotalEvidenceEarned += evidenceReward;
                state.Tinfoil += quest.TinfoilReward;
                state.QuestsCompleted++;

                if (_config.Verbose)
                    Console.WriteLine($"  [Quest] Completed: {quest.Name} (+{FormatNumber(evidenceReward)} evidence, +{quest.TinfoilReward} tinfoil)");
            }
            else
            {
                state.QuestsFailed++;
                if (quest.Risk == QuestRisk.High)
                {
                    state.Believers -= quest.BelieversRequired;
                    state.BelieversLost += quest.BelieversRequired;
                }
            }
        }
    }

    private void UpdateBelievers()
    {
        var state = _engine.State;
        double totalBelievers = 0;

        foreach (var (genId, count) in state.Generators)
        {
            var generator = GeneratorData.GetById(genId);
            if (generator != null)
                totalBelievers += generator.BelieverBonus * count;
        }

        // Apply Tinfoil Shop believer multiplier
        double believerMultiplier = 1.0;
        foreach (var purchaseId in state.TinfoilShopPurchases)
        {
            var upgrade = TinfoilShopData.GetById(purchaseId);
            if (upgrade?.Type == TinfoilUpgradeType.BelieverBonus)
                believerMultiplier *= upgrade.Value;
        }
        totalBelievers *= believerMultiplier;

        state.Believers = totalBelievers;
        // AvailableBelievers is computed automatically as Believers - BusyBelievers
    }

    private void ExecuteBuyStrategy()
    {
        var state = _engine.State;

        switch (_config.Strategy)
        {
            case BuyStrategy.Balanced:
                BuyBalanced();
                break;
            case BuyStrategy.GeneratorsFirst:
                BuyGeneratorsFirst();
                break;
            case BuyStrategy.UpgradesFirst:
                BuyUpgradesFirst();
                break;
            case BuyStrategy.Optimal:
                BuyOptimal();
                break;
        }
    }

    private void BuyBalanced()
    {
        // Buy the cheapest available thing
        var cheapestGen = GetCheapestAffordableGenerator();
        var cheapestUpgrade = GetCheapestAffordableUpgrade();

        if (cheapestGen != null && cheapestUpgrade != null)
        {
            double genCost = _engine.GetGeneratorCost(cheapestGen.Id);
            if (genCost <= cheapestUpgrade.EvidenceCost)
            {
                if (_engine.PurchaseGenerator(cheapestGen.Id))
                    _totalPurchases++;
            }
            else
            {
                if (_engine.PurchaseUpgrade(cheapestUpgrade.Id))
                    _totalPurchases++;
            }
        }
        else if (cheapestGen != null)
        {
            if (_engine.PurchaseGenerator(cheapestGen.Id))
                _totalPurchases++;
        }
        else if (cheapestUpgrade != null)
        {
            if (_engine.PurchaseUpgrade(cheapestUpgrade.Id))
                _totalPurchases++;
        }

        // Also buy tinfoil upgrades if affordable
        BuyTinfoilUpgrades();
    }

    private void BuyGeneratorsFirst()
    {
        // Buy generators first, then upgrades
        var cheapestGen = GetCheapestAffordableGenerator();
        if (cheapestGen != null)
        {
            if (_engine.PurchaseGenerator(cheapestGen.Id))
                _totalPurchases++;
        }
        else
        {
            var cheapestUpgrade = GetCheapestAffordableUpgrade();
            if (cheapestUpgrade != null)
            {
                if (_engine.PurchaseUpgrade(cheapestUpgrade.Id))
                    _totalPurchases++;
            }
        }

        BuyTinfoilUpgrades();
    }

    private void BuyUpgradesFirst()
    {
        // Buy upgrades first, then generators
        var cheapestUpgrade = GetCheapestAffordableUpgrade();
        if (cheapestUpgrade != null)
        {
            if (_engine.PurchaseUpgrade(cheapestUpgrade.Id))
                _totalPurchases++;
        }
        else
        {
            var cheapestGen = GetCheapestAffordableGenerator();
            if (cheapestGen != null)
            {
                if (_engine.PurchaseGenerator(cheapestGen.Id))
                    _totalPurchases++;
            }
        }

        BuyTinfoilUpgrades();
    }

    private void BuyOptimal()
    {
        // Calculate payback time for each option and buy the best one
        var state = _engine.State;
        double currentEps = _engine.CalculateEvidencePerSecond();
        if (currentEps <= 0) currentEps = 0.1;

        double bestPayback = double.MaxValue;
        string? bestGenId = null;
        string? bestUpgradeId = null;

        // Check generators
        foreach (var gen in GeneratorData.AllGenerators)
        {
            double cost = _engine.GetGeneratorCost(gen.Id);
            if (state.Evidence < cost) continue;

            // Estimate EPS gain from buying this generator
            int currentCount = state.GetGeneratorCount(gen.Id);
            double currentProd = gen.GetProduction(currentCount);
            double newProd = gen.GetProduction(currentCount + 1);
            double epsGain = newProd - currentProd;

            if (epsGain > 0)
            {
                double payback = cost / epsGain;
                if (payback < bestPayback)
                {
                    bestPayback = payback;
                    bestGenId = gen.Id;
                    bestUpgradeId = null;
                }
            }
        }

        // Check upgrades (simplified - just use cost as proxy for value)
        var availableUpgrades = _engine.GetAvailableUpgrades()
            .Where(u => state.Evidence >= u.EvidenceCost)
            .ToList();

        foreach (var upgrade in availableUpgrades)
        {
            // Estimate value based on type
            double estimatedEpsGain = currentEps * 0.1; // Rough estimate
            if (upgrade.Type == UpgradeType.GlobalBoost)
                estimatedEpsGain = currentEps * (upgrade.Value - 1);
            else if (upgrade.Type == UpgradeType.GeneratorBoost)
                estimatedEpsGain = currentEps * 0.2;

            if (estimatedEpsGain > 0)
            {
                double payback = upgrade.EvidenceCost / estimatedEpsGain;
                if (payback < bestPayback)
                {
                    bestPayback = payback;
                    bestGenId = null;
                    bestUpgradeId = upgrade.Id;
                }
            }
        }

        // Execute best purchase
        if (bestGenId != null)
        {
            if (_engine.PurchaseGenerator(bestGenId))
                _totalPurchases++;
        }
        else if (bestUpgradeId != null)
        {
            if (_engine.PurchaseUpgrade(bestUpgradeId))
                _totalPurchases++;
        }

        BuyTinfoilUpgrades();
    }

    private void BuyTinfoilUpgrades()
    {
        // Buy tinfoil upgrades when affordable
        var availableTinfoil = _engine.GetAvailableTinfoilUpgrades()
            .OrderBy(u => u.TinfoilCost)
            .FirstOrDefault();

        if (availableTinfoil != null && _engine.CanAffordTinfoilUpgrade(availableTinfoil.Id))
        {
            if (_engine.PurchaseTinfoilUpgrade(availableTinfoil.Id))
            {
                _totalPurchases++;
                if (_config.Verbose)
                    Console.WriteLine($"  [Tinfoil] Bought: {availableTinfoil.Name}");
            }
        }
    }

    private Generator? GetCheapestAffordableGenerator()
    {
        var state = _engine.State;
        return GeneratorData.AllGenerators
            .Where(g => state.Evidence >= _engine.GetGeneratorCost(g.Id))
            .OrderBy(g => _engine.GetGeneratorCost(g.Id))
            .FirstOrDefault();
    }

    private Upgrade? GetCheapestAffordableUpgrade()
    {
        var state = _engine.State;
        return _engine.GetAvailableUpgrades()
            .Where(u => state.Evidence >= u.EvidenceCost)
            .OrderBy(u => u.EvidenceCost)
            .FirstOrDefault();
    }

    private void TryProveConspiracies()
    {
        var state = _engine.State;
        var available = ConspiracyData.GetAvailable(state.Evidence, state.ProvenConspiracies);

        foreach (var conspiracy in available)
        {
            if (state.Evidence >= conspiracy.EvidenceCost)
            {
                if (_engine.ProveConspiracy(conspiracy.Id))
                {
                    Console.WriteLine($"  ★ CONSPIRACY PROVEN: {conspiracy.Name} @ {FormatTime(_simulatedGameTime)}");
                }
            }
        }
    }

    private void CheckAndPerformAscension()
    {
        if (!_engine.CanPrestige()) return;

        var state = _engine.State;
        int potentialTokens = _engine.GetTokensFromPrestige();
        double currentEps = _engine.CalculateEvidencePerSecond();
        double timeSinceLastAscension = _simulatedGameTime - _lastAscensionTime;

        bool shouldAscend = false;

        switch (_config.Ascension)
        {
            case AscensionMode.Early:
                // Ascend as soon as we can (1+ tokens)
                shouldAscend = potentialTokens >= 1;
                break;

            case AscensionMode.Late:
                // Only ascend when we've hit a plateau (EPS barely increasing)
                // Check every 5 minutes of game time
                if (_simulatedGameTime - _timeAtLastPrestigeCheck >= 300)
                {
                    double epsGrowth = _epsAtLastPrestigeCheck > 0
                        ? (currentEps / _epsAtLastPrestigeCheck)
                        : 100;

                    // If EPS hasn't doubled in 5 minutes, time to ascend
                    shouldAscend = epsGrowth < 2.0 && potentialTokens >= 1;

                    _timeAtLastPrestigeCheck = _simulatedGameTime;
                    _epsAtLastPrestigeCheck = currentEps;
                }
                break;

            case AscensionMode.Optimal:
                // Ascend when the expected speedup from tokens outweighs restart cost
                // Rule of thumb: Ascend when tokens would give significant multiplier boost
                // and we've been playing at least 30 mins since last ascension

                if (timeSinceLastAscension < 1800) break; // Min 30 min between ascensions

                // Calculate potential multiplier gain from new tokens
                double currentMultiplier = CalculateIlluminatiMultiplier(state);
                int currentTokens = state.IlluminatiTokens;

                // Estimate multiplier after buying best upgrades with new tokens
                double potentialMultiplier = EstimateMultiplierAfterAscension(currentTokens + potentialTokens, state);

                // If we'd get at least 50% more multiplier, and we have decent tokens, ascend
                double multiplierGain = potentialMultiplier / currentMultiplier;

                // Also check if we're earning tokens efficiently (at least 1 token per 30 min)
                double tokensPerHour = potentialTokens / (timeSinceLastAscension / 3600.0);

                // Ascend if: multiplier gain > 1.5x OR earning < 0.5 tokens/hour (hitting diminishing returns)
                shouldAscend = (multiplierGain > 1.5 && potentialTokens >= 2) ||
                              (tokensPerHour < 0.5 && potentialTokens >= 1 && timeSinceLastAscension > 3600);
                break;
        }

        if (shouldAscend)
        {
            PerformAscension(potentialTokens);
        }
    }

    private double CalculateIlluminatiMultiplier(GameState state)
    {
        double mult = 1.0;
        // Values from PrestigeData.cs - actual game multipliers
        if (state.IlluminatiUpgrades.Contains("pyramid_scheme")) mult *= 100.0;
        if (state.IlluminatiUpgrades.Contains("reptilian_dna")) mult *= 100.0;
        if (state.IlluminatiUpgrades.Contains("deep_state_connections")) mult *= 50.0;
        if (state.IlluminatiUpgrades.Contains("ancient_knowledge")) mult *= 100.0;
        if (state.IlluminatiUpgrades.Contains("parallel_universe_access")) mult *= 10.0;
        if (state.IlluminatiUpgrades.Contains("cosmic_alignment")) mult *= 200.0;
        if (state.IlluminatiUpgrades.Contains("illuminati_council_seat")) mult *= 500.0;
        if (state.IlluminatiUpgrades.Contains("eternal_conspiracy")) mult *= 1000.0;
        if (state.IlluminatiUpgrades.Contains("reality_overwrite")) mult *= 25.0;
        if (state.IlluminatiUpgrades.Contains("entropy_mastery")) mult *= 2000.0;
        if (state.IlluminatiUpgrades.Contains("evidence_singularity")) mult *= 5000.0;
        if (state.IlluminatiUpgrades.Contains("omnipresent_network")) mult *= 50.0;
        if (state.IlluminatiUpgrades.Contains("final_truth")) mult *= 200.0;
        return mult;
    }

    private double EstimateMultiplierAfterAscension(int totalTokens, GameState state)
    {
        // Simulate buying upgrades optimally with available tokens
        double mult = 1.0;
        int tokensRemaining = totalTokens;

        // Priority order for EPS multiplier upgrades - actual values from PrestigeData.cs
        var upgradePriority = new (string id, int cost, double multiplier)[]
        {
            ("pyramid_scheme", 1, 100.0),           // 1 token - x100 EPS
            ("reptilian_dna", 2, 100.0),            // 2 tokens - x100 EPS
            ("deep_state_connections", 3, 50.0),    // 3 tokens - x50 EPS
            ("ancient_knowledge", 4, 100.0),        // 4 tokens - x100 EPS
            ("parallel_universe_access", 18, 10.0), // 18 tokens - x10 EPS
            ("cosmic_alignment", 25, 200.0),        // 25 tokens - x200 EPS
            ("illuminati_council_seat", 60, 500.0), // 60 tokens - x500 EPS
            ("eternal_conspiracy", 125, 1000.0),    // 125 tokens - x1000 EPS
            ("reality_overwrite", 150, 25.0),       // 150 tokens - x25 ALL
            ("entropy_mastery", 200, 2000.0),       // 200 tokens - x2000 EPS
            ("evidence_singularity", 350, 5000.0),  // 350 tokens - x5000 EPS
            ("omnipresent_network", 450, 50.0),     // 450 tokens - x50 ALL
            ("final_truth", 600, 200.0),            // 600 tokens - x200 ALL
        };

        foreach (var (id, cost, multiplier) in upgradePriority)
        {
            if (state.IlluminatiUpgrades.Contains(id))
            {
                mult *= multiplier; // Already have it
            }
            else if (tokensRemaining >= cost)
            {
                mult *= multiplier;
                tokensRemaining -= cost;
            }
        }

        return mult;
    }

    private void PerformAscension(int tokensEarned)
    {
        var state = _engine.State;

        Console.WriteLine($"  ⬆ ASCENDING @ {FormatTime(_simulatedGameTime)} - Earning {tokensEarned} tokens (Total: {state.IlluminatiTokens + tokensEarned})");

        // Track ascension
        _timesAscended++;
        _totalTokensEarned += tokensEarned;
        _ascensionLog.Add((_simulatedGameTime, tokensEarned));

        // Perform the prestige
        _engine.PerformPrestige();

        // Buy Illuminati upgrades strategically
        BuyIlluminatiUpgrades();

        // Reset simulation tracking that depends on current run
        _questRemainingTime.Clear();
        _comboMeter = 0;
        _goldenEyeActive = false;
        _lastAscensionTime = _simulatedGameTime;
    }

    private void BuyIlluminatiUpgrades()
    {
        var state = _engine.State;

        // Priority order for Illuminati upgrades - values from PrestigeData.cs
        var priorityOrder = new string[]
        {
            // TIER 1: Essential first (1-3 tokens) - Massive boosts
            "pyramid_scheme",      // 1 token - x100 EPS (CRITICAL FIRST BUY)
            "reptilian_dna",       // 2 tokens - x100 EPS
            "secret_handshake",    // 2 tokens - x50 click power
            "deep_state_connections", // 3 tokens - x50 EPS
            "new_world_order_discount", // 3 tokens - -90% generator costs

            // TIER 2: Secondary boosts (4-10 tokens)
            "ancient_knowledge",   // 4 tokens - x100 EPS
            "auto_clicker",        // 4 tokens - +20 auto clicks/sec
            "time_manipulation",   // 5 tokens - -90% quest duration
            "moon_base_alpha",     // 5 tokens - +500% quest rewards
            "golden_eye_magnetism", // 6 tokens - 5x golden eyes, 10x rewards
            "believer_magnetism",  // 6 tokens - +500% believers
            "mind_control_mastery", // 7 tokens - 5x faster quest work
            "all_seeing_investment", // 8 tokens - +25% per token
            "infinite_tinfoil",    // 10 tokens - +200 tinfoil/min

            // TIER 3: Advanced (12-30 tokens)
            "third_eye_awakening", // 12 tokens - +50% crit chance, 10x crit
            "instant_indoctrination", // 14 tokens - 50% instant quests
            "shadow_network",      // 15 tokens - -95% generator costs
            "parallel_universe_access", // 18 tokens - x10 all generators
            "reality_distortion",  // 20 tokens - x100 click power
            "cosmic_alignment",    // 25 tokens - x200 EPS
            "conspiracy_cascade",  // 28 tokens - 10 min 50x EPS on conspiracy
            "global_awakening",    // 30 tokens - +2000% believers

            // TIER 4: Endgame (40+ tokens)
            "temporal_fold",       // 40 tokens - -98% quest duration
            "whistle_blower_network", // 50 tokens - 500x whistle-blower, 10x spawn
            "illuminati_council_seat", // 60 tokens - x500 EPS
            "time_dilation_field", // 75 tokens - 5x game timers
            "omniscient_vision",   // 100 tokens - 100% quest success, +80% bonus
            "eternal_conspiracy",  // 125 tokens - x1000 EPS
            "reality_overwrite",   // 150 tokens - x25 ALL production

            // TIER 5: Transcendent (200+ tokens)
            "entropy_mastery",     // 200 tokens - x2000 EPS
            "probability_control", // 225 tokens - always succeed, 90% faster
            "tinfoil_transmutation", // 250 tokens - +2000 tinfoil/min
            "believer_singularity", // 275 tokens - +25000% believers
            "click_transcendence", // 300 tokens - x500 click, +200 auto/sec

            // TIER 6: Omega (350+ tokens)
            "evidence_singularity", // 350 tokens - x5000 EPS
            "temporal_loop",       // 400 tokens - 10x all timers
            "omnipresent_network", // 450 tokens - x50 ALL production
            "cosmic_tinfoil",      // 500 tokens - +10000 tinfoil/min, x10 tinfoil
            "final_truth",         // 600 tokens - x200 ALL production
        };

        foreach (var upgradeId in priorityOrder)
        {
            if (_engine.CanAffordIlluminatiUpgrade(upgradeId))
            {
                var upgrade = PrestigeData.GetById(upgradeId);
                if (_engine.PurchaseIlluminatiUpgrade(upgradeId))
                {
                    if (_config.Verbose)
                        Console.WriteLine($"    [Illuminati] Bought: {upgrade?.Name} ({upgrade?.TokenCost} tokens)");
                }
            }
        }
    }

    private void PrintProgress()
    {
        var state = _engine.State;
        double eps = _engine.CalculateEvidencePerSecond();

        Console.WriteLine($"[{FormatTime(_simulatedGameTime)}] " +
            $"Evidence: {FormatNumber(state.Evidence)} | " +
            $"EPS: {FormatNumber(eps)} | " +
            $"Generators: {state.Generators.Values.Sum()} | " +
            $"Conspiracies: {state.ProvenConspiracies.Count}/25 | " +
            $"Tinfoil: {state.Tinfoil}");
    }

    private void PrintFinalReport(TimeSpan realTime)
    {
        var state = _engine.State;

        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    SIMULATION REPORT                         ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        Console.WriteLine("=== TIMING ===");
        Console.WriteLine($"  Real Time Elapsed: {realTime.TotalMinutes:F1} minutes");
        Console.WriteLine($"  Simulated Game Time: {FormatTime(_simulatedGameTime)}");
        Console.WriteLine($"  Speed Achieved: {(_simulatedGameTime / realTime.TotalSeconds):F0}x");
        Console.WriteLine();

        Console.WriteLine("=== FINAL STATS ===");
        Console.WriteLine($"  Total Evidence: {FormatNumber(state.TotalEvidenceEarned)}");
        Console.WriteLine($"  Current Evidence: {FormatNumber(state.Evidence)}");
        Console.WriteLine($"  Evidence Per Second: {FormatNumber(_engine.CalculateEvidencePerSecond())}");
        Console.WriteLine($"  Total Clicks: {_totalClicks:N0}");
        Console.WriteLine($"  Total Purchases: {_totalPurchases:N0}");
        Console.WriteLine($"  Tinfoil Earned: {state.Tinfoil}");
        Console.WriteLine($"  Believers: {FormatNumber(state.Believers)}");
        Console.WriteLine();

        Console.WriteLine("=== COMBAT MECHANICS ===");
        Console.WriteLine($"  Critical Hits: {_criticalHits:N0}");
        Console.WriteLine($"  Combo Bursts: {_comboBursts:N0}");
        Console.WriteLine($"  Golden Eye Events: {_goldenEyeCount}");
        Console.WriteLine($"  Whistle-blowers Caught: {_whistleBlowersCaught}");
        Console.WriteLine();

        Console.WriteLine("=== QUEST STATS ===");
        Console.WriteLine($"  Quests Completed: {state.QuestsCompleted}");
        Console.WriteLine($"  Quests Failed: {state.QuestsFailed}");
        Console.WriteLine($"  Believers Lost: {state.BelieversLost:N0}");
        Console.WriteLine();

        Console.WriteLine("=== ASCENSION STATS ===");
        Console.WriteLine($"  Ascension Mode: {_config.Ascension}");
        Console.WriteLine($"  Times Ascended: {_timesAscended}");
        Console.WriteLine($"  Total Tokens Earned: {_totalTokensEarned}");
        Console.WriteLine($"  Current Tokens: {state.IlluminatiTokens}");
        Console.WriteLine($"  Illuminati Upgrades: {state.IlluminatiUpgrades.Count}/{PrestigeData.IlluminatiUpgrades.Count}");
        if (_timesAscended > 0)
        {
            Console.WriteLine($"  EPS Multiplier from Illuminati: {CalculateIlluminatiMultiplier(state):F1}x");
        }
        Console.WriteLine();

        if (_ascensionLog.Count > 0)
        {
            Console.WriteLine("=== ASCENSION LOG ===");
            foreach (var (time, tokens) in _ascensionLog)
            {
                Console.WriteLine($"  [{FormatTime(time)}] Ascended - Earned {tokens} tokens");
            }
            Console.WriteLine();
        }

        Console.WriteLine("=== GENERATORS ===");
        foreach (var gen in GeneratorData.AllGenerators)
        {
            int count = state.GetGeneratorCount(gen.Id);
            if (count > 0)
            {
                Console.WriteLine($"  {gen.Name}: {count}");
            }
        }
        Console.WriteLine();

        Console.WriteLine("=== CONSPIRACIES PROVEN ===");
        foreach (var cId in state.ProvenConspiracies)
        {
            var conspiracy = ConspiracyData.GetById(cId);
            if (conspiracy != null)
            {
                var milestone = _milestones.GetMilestone($"conspiracy_{cId}");
                string timeStr = milestone.HasValue ? FormatTime(milestone.Value) : "N/A";
                Console.WriteLine($"  [{timeStr}] {conspiracy.Name}");
            }
        }
        Console.WriteLine();

        Console.WriteLine("=== MILESTONE TIMES ===");
        _milestones.PrintReport();
    }

    private string FormatTime(double seconds)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        if (ts.TotalHours >= 1)
            return $"{(int)ts.TotalHours}h {ts.Minutes}m {ts.Seconds}s";
        else if (ts.TotalMinutes >= 1)
            return $"{ts.Minutes}m {ts.Seconds}s";
        else
            return $"{ts.Seconds}s";
    }

    private string FormatNumber(double value)
    {
        if (value >= 1e24) return $"{value / 1e24:F2}Y";
        if (value >= 1e21) return $"{value / 1e21:F2}Z";
        if (value >= 1e18) return $"{value / 1e18:F2}E";
        if (value >= 1e15) return $"{value / 1e15:F2}P";
        if (value >= 1e12) return $"{value / 1e12:F2}T";
        if (value >= 1e9) return $"{value / 1e9:F2}B";
        if (value >= 1e6) return $"{value / 1e6:F2}M";
        if (value >= 1e3) return $"{value / 1e3:F2}K";
        return value.ToString("F0");
    }
}

public class MilestoneTracker
{
    private readonly Dictionary<string, double> _milestones = new();
    private readonly HashSet<string> _recorded = new();

    // Evidence thresholds to track
    private static readonly (double threshold, string name)[] EvidenceThresholds = new (double, string)[]
    {
        (100.0, "100 Evidence"),
        (1_000.0, "1K Evidence"),
        (10_000.0, "10K Evidence"),
        (50_000.0, "50K Evidence (Prestige Available)"),
        (100_000.0, "100K Evidence"),
        (500_000.0, "500K Evidence (1st Conspiracy)"),
        (1_000_000.0, "1M Evidence"),
        (10_000_000.0, "10M Evidence"),
        (100_000_000.0, "100M Evidence"),
        (1_000_000_000.0, "1B Evidence"),
        (10_000_000_000.0, "10B Evidence"),
        (100_000_000_000.0, "100B Evidence (5th Conspiracy)"),
        (1_000_000_000_000.0, "1T Evidence"),
        (10_000_000_000_000.0, "10T Evidence"),
        (100_000_000_000_000.0, "100T Evidence"),
        (1_000_000_000_000_000.0, "1Q Evidence"),
    };

    public void Check(GameState state, double gameTime)
    {
        // Check evidence thresholds
        foreach (var (threshold, name) in EvidenceThresholds)
        {
            string key = $"evidence_{threshold}";
            if (!_recorded.Contains(key) && state.TotalEvidenceEarned >= threshold)
            {
                _milestones[key] = gameTime;
                _recorded.Add(key);
            }
        }

        // Check conspiracies
        foreach (var cId in state.ProvenConspiracies)
        {
            string key = $"conspiracy_{cId}";
            if (!_recorded.Contains(key))
            {
                _milestones[key] = gameTime;
                _recorded.Add(key);
            }
        }

        // Check first generator purchases
        foreach (var (genId, count) in state.Generators)
        {
            string key = $"first_{genId}";
            if (!_recorded.Contains(key) && count > 0)
            {
                _milestones[key] = gameTime;
                _recorded.Add(key);
            }
        }

        // Check believers
        if (!_recorded.Contains("believers_100") && state.Believers >= 100)
        {
            _milestones["believers_100"] = gameTime;
            _recorded.Add("believers_100");
        }
        if (!_recorded.Contains("believers_1000") && state.Believers >= 1000)
        {
            _milestones["believers_1000"] = gameTime;
            _recorded.Add("believers_1000");
        }
    }

    public double? GetMilestone(string key)
    {
        return _milestones.TryGetValue(key, out var time) ? time : null;
    }

    public void PrintReport()
    {
        var sorted = _milestones
            .OrderBy(kv => kv.Value)
            .ToList();

        foreach (var (key, time) in sorted)
        {
            string name = FormatMilestoneName(key);
            Console.WriteLine($"  [{FormatTime(time)}] {name}");
        }
    }

    private string FormatMilestoneName(string key)
    {
        if (key.StartsWith("evidence_"))
        {
            double val = double.Parse(key.Replace("evidence_", ""));
            return $"Reached {FormatNumber(val)} evidence";
        }
        if (key.StartsWith("conspiracy_"))
        {
            string cId = key.Replace("conspiracy_", "");
            var c = ConspiracyData.GetById(cId);
            return c != null ? $"Proved: {c.Name}" : key;
        }
        if (key.StartsWith("first_"))
        {
            string gId = key.Replace("first_", "");
            var g = GeneratorData.AllGenerators.FirstOrDefault(x => x.Id == gId);
            return g != null ? $"First {g.Name}" : key;
        }
        if (key == "believers_100") return "100 Believers";
        if (key == "believers_1000") return "1,000 Believers";
        return key;
    }

    private string FormatTime(double seconds)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        if (ts.TotalHours >= 1)
            return $"{(int)ts.TotalHours}h {ts.Minutes}m";
        else if (ts.TotalMinutes >= 1)
            return $"{ts.Minutes}m {ts.Seconds}s";
        else
            return $"{ts.Seconds}s";
    }

    private string FormatNumber(double value)
    {
        if (value >= 1e12) return $"{value / 1e12:F0}T";
        if (value >= 1e9) return $"{value / 1e9:F0}B";
        if (value >= 1e6) return $"{value / 1e6:F0}M";
        if (value >= 1e3) return $"{value / 1e3:F0}K";
        return value.ToString("F0");
    }
}
