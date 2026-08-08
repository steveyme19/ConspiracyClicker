using ConspiracyClicker.Core;
using ConspiracyClicker.Data;
using Xunit;

namespace ConspiracyClicker.Tests;

/// <summary>
/// Regression cover for the engine defects fixed in the performance/correctness pass.
/// Each test pins one invariant that was silently broken.
///
/// Nothing here calls Stop() or Save(): SaveManager writes to the real save directory under
/// LocalApplicationData, and a test run must not touch a player's save slots.
/// </summary>
public class EngineTests
{
    private const string CheapGenerator = "red_string";           // cheapest, produces no believers
    private const string BelieverGenerator = "suspicious_neighbor"; // BelieverBonus = 1

    private static GameEngine EngineWithEvidence(double evidence)
    {
        var engine = new GameEngine();
        engine.State.Evidence = evidence;
        return engine;
    }

    // === Buy-max ===

    [Fact]
    public void BuyMax_ChargesTheSameAsBuyingOneAtATime()
    {
        var bulk = EngineWithEvidence(100_000);
        var single = EngineWithEvidence(100_000);

        int expected = single.GetMaxAffordable(CheapGenerator);
        Assert.True(expected > 1, "test needs a generator the engine can afford several of");

        for (int i = 0; i < expected; i++)
            Assert.True(single.PurchaseGenerator(CheapGenerator));

        Assert.True(bulk.PurchaseMaxGenerators(CheapGenerator));

        Assert.Equal(expected, bulk.State.GetGeneratorCount(CheapGenerator));
        Assert.Equal(single.State.GetGeneratorCount(CheapGenerator), bulk.State.GetGeneratorCount(CheapGenerator));
        Assert.Equal(single.State.Evidence, bulk.State.Evidence, precision: 6);
    }

    [Fact]
    public void BuyMax_NeverSpendsMoreEvidenceThanThePlayerHas()
    {
        var engine = EngineWithEvidence(100_000);

        engine.PurchaseMaxGenerators(CheapGenerator);

        Assert.True(engine.State.Evidence >= 0, $"buy max overdrew the balance to {engine.State.Evidence}");
        Assert.False(engine.CanAffordGenerator(CheapGenerator), "buy max should leave the next unit unaffordable");
    }

    [Fact]
    public void BuyMax_RaisesASingleTick()
    {
        var engine = EngineWithEvidence(100_000);
        int ticks = 0;
        engine.OnTick += () => ticks++;

        engine.PurchaseMaxGenerators(CheapGenerator);

        Assert.Equal(1, ticks);
    }

    // === Large-number accumulation ===

    [Fact]
    public void Evidence_KeepsAccruingWhenTheBankIsLarge()
    {
        // The old per-second rounding to three significant figures quantised the balance to
        // bank/1000 up here, so a tick's income vanished and progress stopped entirely.
        var engine = EngineWithEvidence(1e12);
        engine.State.Generators[CheapGenerator] = 5_000;

        double eps = engine.CalculateEvidencePerSecond();
        Assert.True(eps > 0, "test needs a positive EPS");

        double before = engine.State.Evidence;
        const int ticks = 20;
        for (int i = 0; i < ticks; i++)
            engine.TickForTests();

        double expected = eps * ticks * (GameConstants.TICK_RATE_MS / 1000.0);
        double gained = engine.State.Evidence - before;

        Assert.True(gained > expected * 0.99,
            $"expected roughly {expected} evidence over {ticks} ticks at {eps}/s, gained {gained}");
    }

    // === Believers lost to failed high-risk quests ===

    [Fact]
    public void LostBelievers_SurvivesTheBelieverRecalculation()
    {
        // Believers are recomputed from the generators on every tick, so the forfeit from a
        // failed high-risk quest has to be carried as its own term or it is erased immediately.
        var control = new GameEngine();
        control.State.Generators[BelieverGenerator] = 10_000;
        control.TickForTests();
        double full = control.State.Believers;
        Assert.True(full > 0, "test needs generators that produce believers");

        var penalised = new GameEngine();
        penalised.State.Generators[BelieverGenerator] = 10_000;
        penalised.State.LostBelievers = full / 4;

        penalised.TickForTests();
        penalised.TickForTests();

        Assert.Equal(full - full / 4, penalised.State.Believers, precision: 6);
    }

    [Fact]
    public void LostBelievers_CannotDriveTheBelieverCountNegative()
    {
        var engine = new GameEngine();
        engine.State.Generators[BelieverGenerator] = 10;
        engine.State.LostBelievers = 1e9;

        engine.TickForTests();

        Assert.Equal(0, engine.State.Believers);
    }

    [Fact]
    public void Ascending_ClearsBelieversLostDuringTheRun()
    {
        var engine = new GameEngine();
        engine.State.TotalEvidenceEarned = GameConstants.PRESTIGE_THRESHOLD * 10;
        engine.State.LostBelievers = 5_000;
        engine.State.BonusBelievers = 5_000;

        Assert.True(engine.PerformPrestige());

        Assert.Equal(0, engine.State.LostBelievers);
        Assert.Equal(0, engine.State.BonusBelievers);
    }

    // === Offline progress ===

    [Fact]
    public void Start_DoesNotGrantOfflineEvidence()
    {
        // Offline earnings belong to LoadSlot. Start used to pay them a second time at a
        // different rate, so every load handed out 75% of EPS for the time away.
        var engine = new GameEngine();
        engine.State.Generators[CheapGenerator] = 1_000;
        engine.State.LastSaveTime = DateTime.Now.AddHours(-5);
        engine.State.Evidence = 0;

        engine.Start();

        Assert.Equal(0, engine.State.Evidence);
    }

    // === Auto-clicks ===

    [Fact]
    public void AutoClicks_AreCreditedOncePerClickAndReportedAsOneBatch()
    {
        var engine = new GameEngine();
        engine.State.IlluminatiUpgrades.Add("auto_clicker"); // +20 clicks/sec

        double rate = engine.GetAutoClickRate();
        Assert.Equal(20.0, rate);

        int batches = 0;
        int clicksReported = 0;
        engine.OnAutoClickBatch += (_, clicks, _) => { batches++; clicksReported += clicks; };

        long before = engine.State.TotalClicks;
        engine.TickForTests(); // one 100 ms tick at 20 CPS == 2 clicks

        Assert.Equal(1, batches);
        Assert.Equal(2, clicksReported);
        Assert.Equal(before + 2, engine.State.TotalClicks);
        Assert.True(engine.State.Evidence > 0, "auto-clicks should pay out evidence");
    }

    [Fact]
    public void AutoClicks_DoNotFillTheComboMeter()
    {
        var engine = new GameEngine();
        engine.State.IlluminatiUpgrades.Add("auto_clicker");

        for (int i = 0; i < 10; i++)
            engine.TickForTests();

        Assert.Equal(0, engine.State.ComboMeter);
        Assert.Equal(0, engine.State.ComboClicks);
    }

    // === Data lookups ===

    [Theory]
    [MemberData(nameof(AllDataIds))]
    public void GetById_ResolvesEveryIdInEveryTable(string table, string id, Func<string, object?> lookup)
    {
        Assert.True(lookup(id) != null, $"{table} could not resolve id '{id}'");
    }

    public static IEnumerable<object[]> AllDataIds()
    {
        foreach (var g in GeneratorData.AllGenerators)
            yield return new object[] { "GeneratorData", g.Id, (Func<string, object?>)(id => GeneratorData.GetById(id)) };
        foreach (var u in UpgradeData.AllUpgrades)
            yield return new object[] { "UpgradeData", u.Id, (Func<string, object?>)(id => UpgradeData.GetById(id)) };
        foreach (var c in ConspiracyData.AllConspiracies)
            yield return new object[] { "ConspiracyData", c.Id, (Func<string, object?>)(id => ConspiracyData.GetById(id)) };
        foreach (var a in AchievementData.AllAchievements)
            yield return new object[] { "AchievementData", a.Id, (Func<string, object?>)(id => AchievementData.GetById(id)) };
        foreach (var t in TinfoilShopData.AllUpgrades)
            yield return new object[] { "TinfoilShopData", t.Id, (Func<string, object?>)(id => TinfoilShopData.GetById(id)) };
        foreach (var p in PrestigeData.IlluminatiUpgrades)
            yield return new object[] { "PrestigeData", p.Id, (Func<string, object?>)(id => PrestigeData.GetById(id)) };
        foreach (var q in QuestData.AllQuests)
            yield return new object[] { "QuestData", q.Id, (Func<string, object?>)(id => QuestData.GetById(id)) };
        foreach (var s in SkillTreeData.AllSkills)
            yield return new object[] { "SkillTreeData", s.Id, (Func<string, object?>)(id => SkillTreeData.GetById(id)) };
        foreach (var m in MatrixData.MatrixUpgrades)
            yield return new object[] { "MatrixData", m.Id, (Func<string, object?>)(id => MatrixData.GetById(id)) };
        foreach (var c in ChallengeModeData.AllChallenges)
            yield return new object[] { "ChallengeModeData", c.Id, (Func<string, object?>)(id => ChallengeModeData.GetById(id)) };
        foreach (var gu in GeneratorUpgradeData.AllUpgrades)
            yield return new object[] { "GeneratorUpgradeData", gu.Id, (Func<string, object?>)(id => GeneratorUpgradeData.GetById(id)) };
    }

    [Fact]
    public void GeneratorUpgradeIndex_GroupsEveryUpgradeUnderItsGenerator()
    {
        foreach (var generator in GeneratorData.AllGenerators)
        {
            var forGenerator = GeneratorUpgradeData.GetUpgradesForGenerator(generator.Id);

            Assert.All(forGenerator, u => Assert.Equal(generator.Id, u.GeneratorId));
            Assert.Equal(
                forGenerator.Select(u => u.UnlockLevel).OrderBy(level => level),
                forGenerator.Select(u => u.UnlockLevel));
        }

        int indexed = GeneratorData.AllGenerators.Sum(g => GeneratorUpgradeData.GetUpgradesForGenerator(g.Id).Count);
        Assert.Equal(GeneratorUpgradeData.AllUpgrades.Count, indexed);
    }
}
